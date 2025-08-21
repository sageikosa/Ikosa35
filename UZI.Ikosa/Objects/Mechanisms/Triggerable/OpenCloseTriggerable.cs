using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    /// <summary>Defines a triggerable mechanism that can open or close something in response to a trigger</summary>
    [Serializable]
    public class OpenCloseTriggerable : Mechanism, ITriggerable
    {
        #region ctor()
        /// <summary>Defines a triggerable mechanism that can open or close something in response to a trigger</summary>
        public OpenCloseTriggerable(string name, Material material, int seedDifficulty,
            OpenCloseTriggerOperation operation, IOpenable openable)
            : base(name, material, seedDifficulty)
        {
            _Operation = operation;

            // create an adjunct to link mechanism to openable
            var _octGroup = new OpenCloseTriggerableGroup();
            AddAdjunct(new OpenCloseTriggerableMaster(_octGroup));

            // set target
            openable?.AddAdjunct(new OpenCloseTriggerableTarget(_octGroup));
            Activation = new Activation(this, true);
            AddAdjunct(new TrapPart(true));

            _PostState = PostTriggerState.AutoReset;
        }
        #endregion

        #region data
        private OpenCloseTriggerOperation _Operation;
        private PostTriggerState _PostState;
        #endregion

        public OpenCloseTriggerableMaster OpenCloseTriggerableMaster
            => Adjuncts.OfType<OpenCloseTriggerableMaster>().FirstOrDefault();

        #region public IOpenable OpenableTarget { get; set; }
        public IOpenable OpenableTarget
        {
            get => OpenCloseTriggerableMaster?.OpenCloseTriggerableGroup.Target?.Openable;
            set
            {
                var _target = OpenCloseTriggerableMaster?.OpenCloseTriggerableGroup.Target;
                if (_target?.Openable != null)
                {
                    // stop monitoring the old one, and de-link it
                    _target.Eject();
                }

                // link the new one
                value?.AddAdjunct(_target);
            }
        }
        #endregion

        public PostTriggerState PostTriggerState
        {
            get => _PostState;
            set
            {
                _PostState = value;
                DoPropertyChanged(nameof(PostTriggerState));
            }
        }

        public override IEnumerable<IActivatable> Dependents
        {
            get
            {
                if (OpenableTarget is IActivatable _act)
                {
                    yield return _act;
                }

                yield break;
            }
        }

        public OpenCloseTriggerOperation OpenCloseTriggerOperation
        {
            get => _Operation;
            set
            {
                _Operation = value;
                DoPropertyChanged(nameof(OpenCloseTriggerOperation));
            }
        }

        protected override string ClassIconKey => nameof(OpenCloseTriggerable);

        public void DoTrigger(IActivatableObject mechanism, IEnumerable<Locator> locators)
        {
            if (Activation.IsActive && !IsDisabled)
            {
                var _open = OpenableTarget;
                if (_open != null)
                {
                    void _doProcess(double value, string state)
                    {
                        ConfusedDisablers.Clear();
                        var _process = new CoreProcess(
                            new StartOpenCloseStep(null, _open, null, this, value), $@"Trigger {state}");
                        _process.AppendCompletion(
                            new OpenCloseTriggerablePostTriggerStep(_process, this));
                        Setting?.ContextSet?.ProcessManager?.StartProcess(_process);
                    }

                    switch (OpenCloseTriggerOperation)
                    {
                        case OpenCloseTriggerOperation.Close:
                            if (!_open.OpenState.IsClosed)
                            {
                                _doProcess(0, @"Close");
                            }
                            break;

                        case OpenCloseTriggerOperation.Open:
                            if (_open.OpenState.IsClosed)
                            {
                                _doProcess(1, @"Open");
                            }
                            break;

                        case OpenCloseTriggerOperation.Flip:
                            {
                                _doProcess(
                                    _open.OpenState.IsClosed ? 1 : 0,
                                    _open.OpenState.IsClosed ? @"Open" : @"Close");
                            }
                            break;
                    }
                }
            }
        }

        public void DoPostTrigger()
        {
            switch (PostTriggerState)
            {
                case PostTriggerState.Destroy:
                    StructurePoints = 0;
                    break;

                case PostTriggerState.Damage:
                    // NOTE: implicit disable due to damage
                    StructurePoints = 1;
                    break;

                case PostTriggerState.Disable:
                    if (!this.HasAdjunct<DisabledObject>())
                    {
                        AddAdjunct(new DisabledObject());
                    }

                    break;

                case PostTriggerState.DeActivate:
                    // NOTE: no implicit re-activation action for a trigger
                    //       must attach an activation mechanism to re-enable the trigger
                    //       or the specific trigger must provide an action itself
                    Activation = new Activation(this, false);
                    break;

                default:
                    // includes AutoReset
                    break;
            }
        }
    }

    [Serializable]
    public enum OpenCloseTriggerOperation
    {
        Close,
        Open,
        Flip
    }

    [Serializable]
    public class OpenCloseTriggerableGroup : AdjunctGroup
    {
        public OpenCloseTriggerableGroup()
            : base(typeof(OpenCloseTriggerable))
        {
        }

        public OpenCloseTriggerableMaster Master => Members.OfType<OpenCloseTriggerableMaster>().FirstOrDefault();
        public OpenCloseTriggerableTarget Target => Members.OfType<OpenCloseTriggerableTarget>().FirstOrDefault();

        public override void ValidateGroup()
            => this.ValidateMasteredPlanarLink();
    }

    [Serializable]
    public class OpenCloseTriggerableMaster : GroupMasterAdjunct, IPathDependent
    {
        public OpenCloseTriggerableMaster(OpenCloseTriggerableGroup group)
            : base(group, group)
        {
        }

        /// <summary>Can only anchor on an OpenerCloser (or derived class)</summary>
        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is OpenCloseTriggerable) && base.CanAnchor(newAnchor);

        public OpenCloseTriggerable OpenCloseTriggerable
            => Anchor as OpenCloseTriggerable;

        public OpenCloseTriggerableGroup OpenCloseTriggerableGroup
            => Group as OpenCloseTriggerableGroup;

        public override object Clone()
            => new OpenCloseTriggerableMaster(OpenCloseTriggerableGroup);

        public override void PathChanged(Pathed source)
        {
            if ((source is ObjectBound) && (source.Anchor == null))
            {
                // no longer object bound...get rid of group
                // don't re-use this mechanism
                Eject();
            }
            else
            {
                base.PathChanged(source);
            }
        }
    }

    /// <summary>Blocks direct opening and closing</summary>
    [Serializable]
    public class OpenCloseTriggerableTarget : GroupMemberAdjunct
    {
        public OpenCloseTriggerableTarget(OpenCloseTriggerableGroup group)
            : base(typeof(OpenCloseTriggerable), group)
        {
        }

        /// <summary>Can only anchor to an IOpenable</summary>
        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is IOpenable) && base.CanAnchor(newAnchor);

        public IOpenable Openable
            => Anchor as IOpenable;

        public OpenCloseTriggerableGroup OpenCloseTriggerableGroup
            => Group as OpenCloseTriggerableGroup;

        public override object Clone()
            => new OpenCloseTriggerableTarget(OpenCloseTriggerableGroup);
    }

    [Serializable]
    public class OpenCloseTriggerablePostTriggerStep : CoreStep
    {
        public OpenCloseTriggerablePostTriggerStep(CoreProcess process, OpenCloseTriggerable openCloseTriggerable)
            : base(process)
        {
            _OCT = openCloseTriggerable;
        }

        #region state
        private OpenCloseTriggerable _OCT;
        #endregion

        protected override StepPrerequisite OnNextPrerequisite() => null;
        public override bool IsDispensingPrerequisites => false;

        protected override bool OnDoStep()
        {
            _OCT.DoPostTrigger();
            return true;
        }

    }
}
