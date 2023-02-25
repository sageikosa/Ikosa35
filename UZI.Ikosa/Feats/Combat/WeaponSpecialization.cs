using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Feats
{
    #region Easier to find feats if they are non-generic
    [Serializable]
    public abstract class WeaponSpecializationBase : FeatBase
    {
        protected WeaponSpecializationBase(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }
    }
    #endregion

    [
    Serializable,
    ParameterizedFeatInfo(@"Weapon Specialization", @"+2 damage rolls with the weapon", typeof(FocusWeaponsLister)),
    FighterLevelRequirement(4),
    FighterBonusFeat
    ]
    public class WeaponSpecialization<WpnType> : WeaponSpecializationBase, IQualifyDelta
        where WpnType : IWeapon
    {
        public WeaponSpecialization(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _Delta = new QualifyingDelta(2, GetType(), @"Weapon Specialization");
            _Terminator = new TerminateController(this);
        }

        public override string Benefit => @"+2 damage rolls with this weapon";
        public override string Name => $@"Weapon Specialization: {typeof(WpnType).Name}";
        public override string PreRequisite => $@"Fighter Level 4 and Weapon Focus with {typeof(WpnType).Name}";

        protected override void OnAdd()
        {
            base.OnAdd();
            Creature.ExtraWeaponDamage.Deltas.Add(this);
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
                && creature.Proficiencies.IsProficientWith<WpnType>(PowerLevel)
                && creature.Feats.Contains(typeof(WeaponFocusFeat<WpnType>), PowerLevel);
        }

        public override bool MeetsRequirementsAtPowerLevel
        {
            get
            {
                if (Creature == null)
                    return false;
                if (IgnorePreRequisite)
                    return true;
                return base.MeetsRequirementsAtPowerLevel
                    && Creature.Proficiencies.IsProficientWith<WpnType>(PowerLevel)
                    && Creature.Feats.Contains(typeof(WeaponFocusFeat<WpnType>), PowerLevel);
            }
        }

        #region IQualifyDelta Members
        private IDelta _Delta;
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => QualifiedDelta(qualify.Source).ToEnumerable().Where(_d => _d != null);

        public IDelta QualifiedDelta(object source)
        {
            if ((source is IWeaponHead _head)
                && typeof(WpnType).IsAssignableFrom(_head.ContainingWeapon.GetType()))
                return _Delta;
            return null;
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

    public class SpecializedWeaponsLister : IFeatParameterProvider
    {
        #region IFeatParameterProvider Members
        public IEnumerable<FeatParameter> AvailableParameters(ParameterizedFeatListItem target,
            Creature creature, int powerDie)
        {
            foreach (FeatBase _feat in from _f in creature.Feats
                                       where _f.PowerLevel <= powerDie
                                       select _f)
            {
                Type _fType = _feat.GetType();
                if (typeof(WeaponSpecializationBase).IsAssignableFrom(_fType))
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
}
