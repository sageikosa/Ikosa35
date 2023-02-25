using System;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using System.Collections.Generic;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    ParameterizedFeatInfo(@"Improved Critical", @"Critical threat range doubled", typeof(ProficientWeaponsLister)),
    BaseAttackRequirement(8)
    ]
    public class ImprovedCriticalFeat<Wpn> : FeatBase, IQualifyDelta where Wpn : IWeapon
    {
        public ImprovedCriticalFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _Terminator = new TerminateController(this);
            _Delta = new QualifyingDelta(1, typeof(Deltas.CriticalRange), @"Improved Critical");
        }

        public override string Name => $@"Improved Critical: {nameof(Wpn)}";
        public override string Benefit => $@"Critical threat range doubled for {nameof(Wpn)}";
        public override string PreRequisite
            => $@"Proficiency with {nameof(Wpn)}, and Base Attack +8 or Greater";

        public override bool MeetsPreRequisite(Creature creature)
        {
            if (IgnorePreRequisite)
                return true;

            return (creature.Proficiencies.IsProficientWith<Wpn>(PowerLevel))
                && base.MeetsPreRequisite(creature);
        }

        protected override void OnAdd()
        {
            base.OnAdd();
            Creature.CriticalRangeFactor.Deltas.Add(this);
        }

        protected override void OnRemove()
        {
            DoTerminate();
            base.OnRemove();
        }

        #region IQualifyDelta Members
        private IDelta _Delta;
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if ((qualify.Source is IWeaponHead _head)
                && typeof(Wpn).IsAssignableFrom(_head.ContainingWeapon.GetType()))
                yield return _Delta;
            yield break;
        }
        #endregion

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  
        /// Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Terminator.DoTerminate();
        }

        #region IControlTerminate Members
        private readonly TerminateController _Terminator;
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
        #endregion
        #endregion
    }
}
