using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    /// <summary>Mechanism that can cause ITriggerables to perform their triggering responses</summary>
    [Serializable]
    public abstract class TriggerMechanism : Mechanism
    {
        #region ctor()
        /// <summary>Mechanism that can cause ITriggerables to perform their triggering responses</summary>
        protected TriggerMechanism(string name, Material material, int disableDifficulty, PostTriggerState postState)
            : base(name, material, disableDifficulty)
        {
            // create an adjunct group to define the mechanism in the environment
            var _trgGroup = new TriggerGroup();
            var _trgMaster = new TriggerMaster(_trgGroup);
            AddAdjunct(_trgMaster);

            _PostState = postState;
            Activation = new Activation(this, true);
            AddAdjunct(new TrapPart(true));
        }
        #endregion

        #region data
        private PostTriggerState _PostState;
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

        public TriggerMaster TriggerMaster
            => Adjuncts.OfType<TriggerMaster>().FirstOrDefault();

        public IEnumerable<ITriggerable> Triggerables
            => TriggerMaster.TriggerGroup.Targets.Select(_t => _t.Triggerable);

        public override IEnumerable<IActivatable> Dependents
            => Triggerables;

        #region public override void FailedDisable(CoreActivity activity)
        public override void FailedDisable(CoreActivity activity)
        {
            base.FailedDisable(activity);

            // fail, trigger
            var _loc = activity?.Actor?.GetLocated()?.Locator;
            if (_loc != null)
            {
                DoTrigger(_loc.ToEnumerable());
            }
            else
            {
                DoTrigger(new List<Locator>());
            }
        }
        #endregion

        protected override string ClassIconKey => nameof(TriggerMechanism);

        public void DoTrigger(IEnumerable<Locator> locators)
        {
            if (Activation.IsActive && !IsDisabled)
            {
                ConfusedDisablers.Clear();
                foreach (var _t in Triggerables.ToList())
                {
                    _t.DoTrigger(this, locators);
                }

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
    }

    [Serializable]
    public class TriggerGroup : AdjunctGroup
    {
        public TriggerGroup()
            : base(typeof(TriggerMechanism))
        {
        }

        public TriggerMaster Master => Members.OfType<TriggerMaster>().FirstOrDefault();
        public IEnumerable<TriggerTarget> Targets => Members.OfType<TriggerTarget>();

        public override void ValidateGroup()
            => this.ValidateMasteredPlanarLink();
    }

    [Serializable]
    public class TriggerMaster : GroupMasterAdjunct, IPathDependent
    {
        public TriggerMaster(TriggerGroup group)
            : base(group, group)
        {
        }


        /// <summary>Can only anchor on a TriggerMechanism (or derived class)</summary>
        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is TriggerMechanism) && base.CanAnchor(newAnchor);

        public TriggerMechanism TriggerMechanism
            => Anchor as TriggerMechanism;

        public TriggerGroup TriggerGroup
            => Group as TriggerGroup;

        public override object Clone()
            => new TriggerMaster(TriggerGroup);

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

    [Serializable]
    public class TriggerTarget : GroupMemberAdjunct, IPathDependent
    {
        public TriggerTarget(TriggerGroup group)
            : base(typeof(TriggerMechanism), group)
        {
        }

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is ITriggerable _trg)                             // must be triggerable
              && !_trg.HasDepenent(TriggerGroup.Master.TriggerMechanism)    // cannot be controlling the master
              && base.CanAnchor(newAnchor);                                 // anything else...

        public ITriggerable Triggerable
            => Anchor as ITriggerable;

        public TriggerGroup TriggerGroup
            => Group as TriggerGroup;

        public override object Clone()
            => new TriggerTarget(TriggerGroup);

        public override void PathChanged(Pathed source)
        {
            if ((source is ObjectBound) && (source.Anchor == null))
            {
                // no longer object bound...
                Eject();
            }
            else
            {
                base.PathChanged(source);
            }
        }
    }
}
