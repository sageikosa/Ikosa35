using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public abstract class TriggerMountGroup<Mech> : AdjunctGroup
        where Mech : TriggerMechanism
    {
        protected TriggerMountGroup(object source)
            : base(source)
        {
        }

        public TriggerMountMaster<Mech> Master => Members.OfType<TriggerMountMaster<Mech>>().FirstOrDefault();
        public TriggerMountTarget<Mech> Target => Members.OfType<TriggerMountTarget<Mech>>().FirstOrDefault();
    }

    [Serializable]
    public abstract class TriggerMountMaster<Mech> : GroupMasterAdjunct, IPathDependent, IMonitorChange<Activation>
        where Mech : TriggerMechanism
    {
        protected TriggerMountMaster(TriggerMountGroup<Mech> group)
            : base(group, group)
        {
        }

        protected override void OnActivate(object source)
        {
            TriggerMechanism.AddChangeMonitor(this);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            TriggerMechanism.RemoveChangeMonitor(this);
            base.OnDeactivate(source);
        }

        public Mech TriggerMechanism 
            => Anchor as Mech;

        protected TriggerMountGroup<Mech> TriggerGroup 
            => Group as TriggerMountGroup<Mech>;

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

        // IMonitorChange<Activation>
        public void PreTestChange(object sender, AbortableChangeEventArgs<Activation> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
            if (args.NewValue.IsActive)
            {
                TriggerGroup.Target.CheckTriggering();
            }
        }
    }

    [Serializable]
    public abstract class TriggerMountTarget<Mech> : GroupMemberAdjunct
        where Mech : TriggerMechanism
    {
        protected TriggerMountTarget(TriggerMountGroup<Mech> group)
            : base(typeof(Mech), group)
        {
        }

        public abstract void CheckTriggering();

        protected TriggerMountGroup<Mech> TriggerGroup
            => Group as TriggerMountGroup<Mech>;
    }
}
