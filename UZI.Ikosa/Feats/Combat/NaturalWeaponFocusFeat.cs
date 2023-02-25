using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    ParameterizedFeatInfo(@"Natural Weapon Focus", @"+1 on Attack Rolls with the natural weapon", typeof(EligibleNaturalWeaponFocusLister)),
    FighterBonusFeat,
    NaturalWeaponRequirement(1)
    ]
    public class NaturalWeaponFocusFeat<WpnType> : WeaponFocusBase, IQualifyDelta
        where WpnType : NaturalWeapon
    {
        public NaturalWeaponFocusFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _Delta = new QualifyingDelta(1, GetType(), Name);
            _Terminator = new TerminateController(this);
        }

        #region data
        private IDelta _Delta;
        private readonly TerminateController _Terminator;
        #endregion

        public override string Benefit => @"+1 on Attack Rolls with the natural weapon";
        public override string Name => $@"Natural Weapon Focus: {typeof(WpnType).Name}";
        public override string PreRequisite => $@"Natural weapon {typeof(WpnType).Name}";

        protected override void OnAdd()
        {
            base.OnAdd();
            Creature.MeleeDeltable.Deltas.Add(this);
            Creature.OpposedDeltable.Deltas.Add(this);
            Creature.RangedDeltable.Deltas.Add(this);
        }

        protected override void OnRemove()
        {
            DoTerminate();
            base.OnRemove();
        }

        public override bool MeetsPreRequisite(Creature creature)
        {
            if (IgnorePreRequisite)
                return true;

            return base.MeetsPreRequisite(creature)
                && creature.Proficiencies.IsProficientWith<WpnType>(PowerLevel);
        }

        #region IQualifyDelta Members
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if ((qualify?.Source is IWeaponHead _head)
                && typeof(WpnType).IsAssignableFrom(_head.ContainingWeapon.GetType()))
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

    public class EligibleNaturalWeaponFocusLister : IFeatParameterProvider
    {
        public IEnumerable<FeatParameter> AvailableParameters(ParameterizedFeatListItem target, Creature creature, int powerDie)
        {
            // current weapon focus
            var _focus = (new FocusWeaponsLister()).AvailableParameters(target, creature, powerDie).ToList();
            foreach (var _param in (new NaturalWeaponLister()).AvailableParameters(target, creature, powerDie).ToList())
            {
                // ensure proficient weapon not already focus weapon
                if (!_focus.Any(_fp => _fp.Type == _param.Type))
                    yield return _param;
            }
            yield break;

        }
    }
}
