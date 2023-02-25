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
    [Serializable]
    public abstract class ActivationMechanism : Mechanism
    {
        protected ActivationMechanism(string name, Material material, int disableDifficulty,
            ActivationMechanismStyle activationMechanismStyle)
            : base(name, material, disableDifficulty)
        {
            _ActivationMechanismStyle = activationMechanismStyle;

            // create an adjunct group to define the mechanism in the environment
            var _actGroup = new ActivationGroup();
            var _actMaster = new ActivationMaster(_actGroup);
            AddAdjunct(_actMaster);

            Activation = new Activation(this, true);
        }

        #region data
        private ActivationMechanismStyle _ActivationMechanismStyle;
        #endregion

        public ActivationMechanismStyle ActivationMechanismStyle
        {
            get => _ActivationMechanismStyle;
            set
            {
                _ActivationMechanismStyle = value;
                // TODO: stuff?
            }
        }

        public ActivationMaster ActivationMaster
            => Adjuncts.OfType<ActivationMaster>().FirstOrDefault();

        public IEnumerable<IActivatableObject> ActivatableObjects
            => ActivationMaster.ActivationGroup.Targets.Select(_t => _t.ActivatableObject);

        public override IEnumerable<IActivatable> Dependents
            => ActivatableObjects;

        protected override string ClassIconKey => nameof(ActivationMechanism);
    }

    [Serializable]
    public class ActivationGroup : AdjunctGroup
    {
        public ActivationGroup()
            : base(typeof(ActivationMechanism))
        {
        }

        public ActivationMaster Master => Members.OfType<ActivationMaster>().FirstOrDefault();
        public IEnumerable<ActivationTarget> Targets => Members.OfType<ActivationTarget>();

        public override void ValidateGroup()
            => this.ValidateMasteredPlanarLink();
    }

    [Serializable]
    public class ActivationMaster : GroupMasterAdjunct, IPathDependent
    {
        public ActivationMaster(ActivationGroup group)
            : base(group, group)
        {
        }

        /// <summary>Can only anchor on an ActivationMechanism (or derived class)</summary>
        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is ActivationMechanism) && base.CanAnchor(newAnchor);

        public ActivationMechanism ActivationMechanism
            => Anchor as ActivationMechanism;

        public ActivationGroup ActivationGroup
            => Group as ActivationGroup;

        public override object Clone()
            => new ActivationMaster(ActivationGroup);

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
    public class ActivationTarget : GroupMemberAdjunct
    {
        public ActivationTarget(ActivationGroup group)
            : base(typeof(ActivationMechanism), group)
        {
        }

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is IActivatableObject _act)                           // must be an activatable object
            && !_act.HasDepenent(ActivationGroup.Master.ActivationMechanism)    // cannot be controlling the master
            && base.CanAnchor(newAnchor);                                       // anything else...

        public IActivatableObject ActivatableObject
            => Anchor as IActivatableObject;

        public ActivationGroup ActivationGroup
            => Group as ActivationGroup;

        public override object Clone()
            => new ActivationTarget(ActivationGroup);
    }
}
