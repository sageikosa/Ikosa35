using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class GraspAwareness : AdjunctGroup
    {
        public GraspAwareness(object source, IEnumerable<Guid> awarenesses)
            : base(source)
        {
            _Aware = awarenesses;
        }

        private IEnumerable<Guid> _Aware;
        public IEnumerable<Guid> Awarenesses => _Aware;

        public GraspAwarenessMaster GraspAwarenessMaster
            => Members.OfType<GraspAwarenessMaster>().FirstOrDefault();

        public static void CreateGraspAwareness(Creature critter, IAdjunctable target, IEnumerable<Guid> awarenesses)
        {
            if (critter != null && target != null)
            {
                var _graspAware = new GraspAwareness(typeof(Grasp), awarenesses.ToList());
                var _graspMaster = new GraspAwarenessMaster(_graspAware, _graspAware);
                critter.AddAdjunct(_graspMaster);
                var _graspTarget = new GraspAwarenessTarget(_graspAware, _graspAware);
                target.AddAdjunct(_graspTarget);
            }
        }

        public static IEnumerable<Guid> GetGraspAwarenesses(Creature critter)
            => critter.Adjuncts.OfType<GraspAwarenessMaster>().SelectMany(_gam => _gam.GraspAwareness.Awarenesses)
            .Distinct();

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();
    }

    [Serializable]
    public class GraspAwarenessMaster : GraspAwarenessTarget, IActionAwareProvider, IQualifyDelta
    {
        public GraspAwarenessMaster(object source, GraspAwareness group)
            : base(source, group)
        {
            _Term = new TerminateController(this);
            _Delta = new QualifyingDelta(12, typeof(GraspAwareness), @"Grasped");
        }

        #region data
        private readonly TerminateController _Term;
        private QualifyingDelta _Delta;
        #endregion

        public override object Clone()
            => new GraspAwarenessMaster(Source, GraspAwareness);

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
            => ((IsActionAware(qualify?.Target.ID ?? Guid.Empty) ?? false)
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
            => GraspAwareness.Awarenesses.Contains(guid);

        #endregion
    }

    [Serializable]
    public class GraspAwarenessTarget : GroupMemberAdjunct, IMonitorChange<IGeometricRegion>
    {
        public GraspAwarenessTarget(object source, GraspAwareness group)
            : base(source, group)
        {
        }

        public GraspAwareness GraspAwareness => Group as GraspAwareness;

        public override object Clone()
            => new GraspAwarenessTarget(Source, GraspAwareness);

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
            GraspAwareness.GraspAwarenessMaster.Eject();
        }

        #endregion
    }
}