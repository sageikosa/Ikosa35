using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Movement;
using Uzi.Visualize;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class BumpAwareness : AdjunctGroup
    {
        public BumpAwareness(object source, IEnumerable<Guid> awarenesses)
            : base(source)
        {
            _Aware = awarenesses;
        }

        private IEnumerable<Guid> _Aware;
        public IEnumerable<Guid> Awarenesses => _Aware;

        public BumpAwarenessMaster BumpAwarenessMaster
            => Members.OfType<BumpAwarenessMaster>().FirstOrDefault();

        public static void CreateBumpAwareness(Creature critter, IAdjunctable target, IEnumerable<Guid> awarenesses)
        {
            if (critter != null && target != null)
            {
                var _bumpAwareness = new BumpAwareness(typeof(MovementBase), awarenesses.ToList());
                var _bumpMaster = new BumpAwarenessMaster(_bumpAwareness, _bumpAwareness);
                critter.AddAdjunct(_bumpMaster);
                var _bumpTarget = new BumpAwarenessTarget(_bumpAwareness, _bumpAwareness);
                target.AddAdjunct(_bumpTarget);
            }
        }

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();
    }

    [Serializable]
    public class BumpAwarenessMaster : BumpAwarenessTarget, IActionAwareProvider, IQualifyDelta
    {
        public BumpAwarenessMaster(object source, BumpAwareness group)
            : base(source, group)
        {
            _Term = new TerminateController(this);
            _Delta = new QualifyingDelta(6, typeof(BumpAwareness), @"Bumped");
        }

        #region data
        private readonly TerminateController _Term;
        private IDelta _Delta;
        #endregion

        public override object Clone()
            => new BumpAwarenessMaster(Source, BumpAwareness);

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as Creature)?.MeleeDeltable.Deltas.Add(this);
        }

        protected override void OnDeactivate(object source)
        {
            DoTerminate();
            base.OnDeactivate(source);
        }

        #region IQualifyDelta
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.AddTerminateDependent(subscriber);
        }

        public void DoTerminate()
        {
            _Term.DoTerminate();
        }

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => ((IsActionAware(qualify?.Target.ID ?? Guid.Empty) ?? true)
            && (((qualify as Interaction)?.InteractData as AttackData)?.Action is Grasp
                || ((qualify as Interaction)?.InteractData as AttackData)?.Action is Probe)
            ? _Delta
            : null).ToEnumerable().Where(_d => _d != null);

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;
        #endregion

        #region IActionAwareProvider Members

        public bool? IsActionAware(Guid guid)
            => BumpAwareness.Awarenesses.Contains(guid)
            ? (bool?)null
            : false;

        #endregion
    }

    [Serializable]
    public class BumpAwarenessTarget : GroupMemberAdjunct, IMonitorChange<IGeometricRegion>
    {
        public BumpAwarenessTarget(object source, BumpAwareness group)
            : base(source, group)
        {
        }

        public BumpAwareness BumpAwareness => Group as BumpAwareness;

        public override object Clone()
            => new BumpAwarenessTarget(Source, BumpAwareness);

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            Anchor.GetLocated()?.Locator.AddChangeMonitor(this);
            base.OnActivate(source);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            Anchor.GetLocated()?.Locator.RemoveChangeMonitor(this);
            base.OnDeactivate(source);
        }
        #endregion

        #region IMonitorChange<IGeometricRegion> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<IGeometricRegion> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args)
        {
            BumpAwareness.BumpAwarenessMaster?.Eject();
        }

        #endregion
    }
}
