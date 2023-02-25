using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Objects
{
    /// <summary>
    /// Blocks self-sourced Open/Close actions from an IOpenable.
    /// Supplies its own open/close action.
    /// </summary>
    [Serializable]
    public class OpenerCloser : Mechanism
    {
        #region ctor()
        public OpenerCloser(string name, Material material, int disableDifficulty)
            : base(name, material, disableDifficulty)
        {
            // create an adjunct group to define the mechanism in the environment
            var _mechGroup = new OpenerCloserGroup();
            var _mechMaster = new OpenerCloserMaster(_mechGroup);
            AddAdjunct(_mechMaster);
            Activation = new Activation(this, true);
            ObjectSizer.NaturalSize = Size.Miniature;
        }
        #endregion

        public OpenerCloserMaster OpenerCloserMaster
            => Adjuncts.OfType<OpenerCloserMaster>().FirstOrDefault();

        #region public virtual IOpenable Openable { get; set; }
        public virtual IOpenable Openable
        {
            get => OpenerCloserMaster?.OpenerCloserGroup.Target?.Openable;
            set
            {
                if (value != Openable)
                {
                    OpenerCloserMaster.OpenerCloserGroup.Target?.Eject();
                    value?.AddAdjunct(new OpenerCloserTarget(OpenerCloserMaster.OpenerCloserGroup));
                }
            }
        }
        #endregion

        #region IActionProvider Members
        public override IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;

            if ((Openable != null) && Activation.IsActive && (_budget?.CanPerformBrief ?? false))
            {
                yield return new OpenCloseAction(this, Openable, @"101");
            }

            foreach (var _act in BaseMechanismActions(budget))
                yield return _act;
            yield break;
        }
        #endregion

        public override IEnumerable<IActivatable> Dependents { get { yield break; } }
        protected override string ClassIconKey
            => nameof(OpenerCloser);
    }

    [Serializable]
    public class OpenerCloserGroup : AdjunctGroup
    {
        public OpenerCloserGroup()
            : base(typeof(OpenerCloser))
        {
        }

        public OpenerCloserMaster Master => Members.OfType<OpenerCloserMaster>().FirstOrDefault();
        public OpenerCloserTarget Target => Members.OfType<OpenerCloserTarget>().FirstOrDefault();

        public override void ValidateGroup()
            => this.ValidateMasteredPlanarLink();
    }

    [Serializable]
    public class OpenerCloserMaster : GroupMasterAdjunct, IPathDependent
    {
        public OpenerCloserMaster(OpenerCloserGroup group)
            : base(group, group)
        {
        }

        /// <summary>Can only anchor on an OpenerCloser (or derived class)</summary>
        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is OpenerCloser) && base.CanAnchor(newAnchor);

        public OpenerCloser OpenerCloser
            => Anchor as OpenerCloser;

        public OpenerCloserGroup OpenerCloserGroup
            => Group as OpenerCloserGroup;

        public override object Clone()
            => new OpenerCloserMaster(OpenerCloserGroup);

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
    public class OpenerCloserTarget : GroupMemberAdjunct, IMonitorChange<OpenStatus>
    {
        public OpenerCloserTarget(OpenerCloserGroup group)
            : base(typeof(OpenerCloser), group)
        {
        }

        /// <summary>Can only anchor to an IOpenable</summary>
        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is IOpenable) && base.CanAnchor(newAnchor);

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (IsActive)
            {
                Openable?.AddChangeMonitor(this);
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);
            if (!IsActive)
            {
                Openable?.RemoveChangeMonitor(this);
            }
        }
        #endregion

        public IOpenable Openable
            => Anchor as IOpenable;

        public OpenerCloserGroup OpenerCloserGroup
            => Group as OpenerCloserGroup;

        public override object Clone()
            => new OpenerCloserTarget(OpenerCloserGroup);

        #region IMonitorChange<OpenStatus> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<OpenStatus> args)
        {
            if ((sender == Openable) && (args.NewValue.Source == Openable))
                args.DoAbort(@"Cannot open directly");
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<OpenStatus> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<OpenStatus> args)
        {
        }
        #endregion
    }
}
