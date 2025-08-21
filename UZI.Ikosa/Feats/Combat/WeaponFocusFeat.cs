using System;
using System.Collections.Generic;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using System.Linq;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Feats
{
    #region Easier to Find (and abstractly use) Feats if they are non-generic
    [Serializable]
    public abstract class WeaponFocusBase : FeatBase
    {
        protected WeaponFocusBase(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }
    }
    #endregion

    [
    Serializable,
    ParameterizedFeatInfo(@"Weapon Focus", @"+1 on Attack Rolls with the weapon", typeof(EligibleWeaponFocusLister)),
    FighterBonusFeat,
    BaseAttackRequirement(1)
    ]
    public class WeaponFocusFeat<WpnType> : WeaponFocusBase, IQualifyDelta
        where WpnType : IWeapon
    {
        public WeaponFocusFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _Delta = new QualifyingDelta(1, GetType(), Name);
            _Terminator = new TerminateController(this);
        }

        public override string Benefit => @"+1 on Attack Rolls with the weapon";
        public override string Name => $@"Weapon Focus: {typeof(WpnType).Name}";
        public override string PreRequisite => $@"Base attack +1 and proficiency with {typeof(WpnType).Name}";

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
            {
                return true;
            }

            return base.MeetsPreRequisite(creature)
                && creature.Proficiencies.IsProficientWith<WpnType>(PowerLevel);
        }

        #region IQualifyDelta Members
        private IDelta _Delta;
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if ((qualify?.Source is IWeaponHead _head)
                && typeof(WpnType).IsAssignableFrom(_head.ContainingWeapon.GetType()))
            {
                yield return _Delta;
            }

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

    public class FocusWeaponsLister : IFeatParameterProvider
    {
        #region IFeatParameterProvider Members
        public IEnumerable<FeatParameter> AvailableParameters(ParameterizedFeatListItem target, Creature creature, int powerDie)
        {
            foreach (FeatBase _feat in from _f in creature.Feats
                                       where _f.PowerLevel <= powerDie
                                       select _f)
            {
                Type _fType = _feat.GetType();
                if (typeof(WeaponFocusBase).IsAssignableFrom(_fType))
                {
                    foreach (Type _gType in _fType.GetGenericArguments())
                    {
                        ItemInfoAttribute _info = ItemBase.GetInfo(_gType);
                        yield return new FeatParameter(target, _gType, _info.Name, _info.Description, powerDie);
                    }
                }
            }
            yield break;
        }
        #endregion
    }

    public class EligibleWeaponFocusLister : IFeatParameterProvider
    {
        public IEnumerable<FeatParameter> AvailableParameters(ParameterizedFeatListItem target, Creature creature, int powerDie)
        {
            // current weapon focus
            var _focus = (new FocusWeaponsLister()).AvailableParameters(target, creature, powerDie).ToList();
            foreach (var _param in (new ProficientWeaponsLister()).AvailableParameters(target, creature, powerDie).ToList())
            {
                // ensure proficient weapon not already focus weapon
                if (!_focus.Any(_fp => _fp.Type == _param.Type))
                {
                    yield return _param;
                }
            }
            yield break;

        }
    }
}
