using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.TypeListers;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Advancement.CharacterClasses
{
    [ClassInfo(@"Fighter", 10, 1d, 2, true, false, false)]
    [Serializable]
    public class Fighter : CharacterClass, IWeaponProficiency, IArmorProficiency, IShieldProficiency
    {
        #region Construction
        public Fighter()
            : base(10, PowerDieCalcMethod.Average)
        {
        }

        public Fighter(PowerDieCalcMethod method)
            : base(10, method)
        {
        }
        #endregion

        #region public override IEnumerable<Type> ClassSkills()
        public override IEnumerable<Type> ClassSkills()
        {
            yield return typeof(ClimbSkill);
            yield return typeof(HandleAnimalSkill);
            yield return typeof(IntimidateSkill);
            yield return typeof(JumpSkill);
            yield return typeof(RideSkill);
            yield return typeof(SwimSkill);
            foreach (Type _skillType in SubSkillLister.SubSkillTypes<CraftFocus>(typeof(CraftSkill<>)))
            {
                yield return _skillType;
            }
            yield break;
        }
        #endregion

        public override string ClassName => @"Fighter";
        public override int SkillPointsPerLevel => 2;
        public override double BABProgression => 1d;
        public override bool HasGoodFortitude => true;
        public override bool HasGoodWill => false;
        public override bool HasGoodReflex => false;

        public bool IsBonusFeatLevel(int level)
            => (level == 1) || ((level % 2) == 0);

        public FeatBase BonusFeat(int level)
        {
            if (IsBonusFeatLevel(level))
            {
                var _powerLevel = Creature.AdvancementLog[this, level].PowerDie.Level;
                return Creature.Feats.Where(_f => (_f.PowerLevel == _powerLevel) && _f.IsBonusFeat(this)).FirstOrDefault();
            }
            return null;
        }

        #region private IFeature BonusFeature(int level)
        private IFeature BonusFeature(int level)
        {
            if (IsBonusFeatLevel(level))
            {
                var _feat = BonusFeat(level);
                if (_feat != null)
                {
                    return new Feature(_feat.Name, _feat.Benefit);
                }
            }
            return null;
        }
        #endregion

        #region public override bool CanLockLevel(int level)
        public override bool CanLockLevel(int level)
        {
            var _feat = BonusFeat(level);
            if (IsBonusFeatLevel(level) && ((_feat == null) || !_feat.MeetsRequirementsAtPowerLevel))
                return false;
            return base.CanLockLevel(level);
        }
        #endregion

        #region public override IEnumerable<IFeature> Features(int level)
        public override IEnumerable<IFeature> Features(int level)
        {
            // if the bonus feat is not set, do not list it as a feature
            if (IsBonusFeatLevel(level))
            {
                var _feat = BonusFeat(level);
                if (_feat != null)
                {
                    yield return new Feature(_feat.Name, _feat.Benefit);
                }
                else
                {
                    yield return new Feature(@"Bonus Feat", @"SELECTION REQUIRED");
                }
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<AdvancementRequirement> Requirements(int level)
        /// <summary>
        /// The requirements have options that are either GenericAdvancementOption&lt;FeatBase&gt;, 
        /// or IParameterizedAdvancementOption
        /// </summary>
        public override IEnumerable<AdvancementRequirement> Requirements(int level)
        {
            if (IsBonusFeatLevel(level) && (LockedLevel < level))
            {
                yield return new AdvancementRequirement(new LevelRequirementKey(@"Fighter.BonusFeat", level), @"Bonus Feat", @"Fighter receives combat related bonus feats",
                    FighterBonusFeats, SetBonusFeat, CheckBonusFeat)
                { CurrentValue = BonusFeature(level) };
            }
            yield break;
        }
        #endregion

        #region private IEnumerable<IAdvancementOption> FighterBonusFeats(IResolveRequirement target, RequirementKey key)
        private IEnumerable<IAdvancementOption> FighterBonusFeats(IResolveRequirement target, RequirementKey key)
        {
            var _levelKey = key as LevelRequirementKey;
            var _powerLevel = Creature.AdvancementLog[this, _levelKey.Level].PowerDie.Level;
            foreach (var _item in from _available in FeatLister.AvailableFeats(Creature, this, _powerLevel)
                                  orderby _available.Name
                                  select _available)
            {
                if ((_item.Feat != null) && FeatBase.IsFighterBonus(_item.Feat.GetType()))
                {
                    // GenericAdvancementOption<> carries to the selection UI as an IAdvancementOption, ...
                    // ...but back to SetBonusFeat as a GenericAdvancementOption<>
                    yield return new AdvancementParameter<FeatBase>(target, _item.Name, _item.Benefit, _item.Feat);
                }
                else
                {
                    // ParameterizedAdvancementOption carries to the selection UI as an IParameterizedAdvancementOption 
                    // (with option values of GenericAdvancementOption<Type>)
                    if ((_item is ParameterizedFeatListItem _pItem)
                        && FeatBase.IsFighterBonus(_pItem.GenericType))
                    {
                        // convert feat type list to generic advancement option list
                        yield return new GenericTypeAdvancementOption(target, _pItem.Name, _pItem.Benefit, _pItem.GenericType,
                            from p in _pItem.AvailableParameters
                            select (IAdvancementOption)(new AdvancementParameter<Type>(p.Name, p.Description, p.Type)));
                    }
                }
            }
            yield break;
        }
        #endregion

        #region private bool DoBindBonusFeat(FeatBase bonusFeat, int classLevel)
        private bool DoBindBonusFeat(FeatBase bonusFeat, int classLevel)
        {
            if (LockedLevel < classLevel)
            {
                var _feat = BonusFeat(classLevel);
                if (_feat != null)
                {
                    // tries to remove an existing bonus feat for this level
                    if (_feat.CanRemove())
                    {
                        // even if the new feat cannot be added, the old will be removed
                        _feat.UnbindFromCreature();
                    }
                    else
                        return false;
                }

                if (bonusFeat.CanAdd(Creature))
                {
                    bonusFeat.BindTo(Creature);
                    return true;
                }
                else
                {
                    // indicate this feat cannot be added
                    return false;
                }
            }
            else
                return false;
        }
        #endregion

        #region private bool SetBonusFeat(RequirementKey key, IAdvancementOption advOption)
        private bool SetBonusFeat(RequirementKey key, IAdvancementOption advOption)
        {
            // must be a simple feat
            var _levelKey = key as LevelRequirementKey;
            if (advOption is AdvancementParameter<FeatBase> _featOption)
            {
                var _newFeat = _featOption.ParameterValue;
                return DoBindBonusFeat(_newFeat, _levelKey.Level);
            }
            else
            {
                if (advOption is AdvancementParameter<Type> _typeOption)
                {
                    // must have been a parameterized feat, get the option, and its target
                    var _powerLevel = Creature.AdvancementLog[this, _levelKey.Level].PowerDie.Level;
                    var _newFeat = (FeatBase)Activator.CreateInstance(_typeOption.ParameterValue, this, _powerLevel);
                    return DoBindBonusFeat(_newFeat, _levelKey.Level);
                }

                // !!! sometimes a GenericTypeAdvanementOption worms its way into here when it has no options and is clicked
                return false;
            }
        }
        #endregion

        #region private bool CheckBonusFeat(RequirementKey key)
        private bool CheckBonusFeat(RequirementKey key)
        {
            var _levelKey = key as LevelRequirementKey;
            if (IsBonusFeatLevel(_levelKey.Level))
            {
                var _feat = BonusFeat(_levelKey.Level);
                if (_feat != null)
                {
                    return _feat.MeetsRequirementsAtPowerLevel;
                }
            }
            return false;
        }
        #endregion

        #region protected override void OnDecreaseLevel()
        /// <summary>Removes any fighter bonus feat for the level</summary>
        protected override void OnDecreaseLevel()
        {
            if (IsBonusFeatLevel(CurrentLevel + 1))
            {
                var _feat = BonusFeat(CurrentLevel + 1);
                if (_feat != null)
                    _feat.UnbindFromCreature();
            }
            base.OnDecreaseLevel();
        }
        #endregion

        #region IWeaponProficiency Members

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
        {
            return ((profType == WeaponProficiencyType.Simple) || (profType == WeaponProficiencyType.Martial))
                && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);
        }

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
        {
            return IsProficientWithWeapon(typeof(WpnType), powerLevel);
        }

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
        {
            return IsProficientWith(weapon.ProficiencyType, powerLevel);
        }

        public bool IsProficientWithWeapon(Type type, int powerLevel)
        {
            // everything but exotic weapons (generally)
            return (!typeof(IExoticWeapon).IsAssignableFrom(type)
                && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level));
        }

        string IWeaponProficiency.Description { get { return @"All simple and martial weapons"; } }

        #endregion

        #region IArmorProficiency Members
        public bool IsProficientWith(ArmorProficiencyType profType, int powerLevel)
        {
            // proficient with all armor
            return (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);
        }

        public bool IsProficientWith(ArmorBase armor, int powerLevel)
        {
            // proficient with all armor
            return (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);
        }

        string IArmorProficiency.Description { get { return @"All Armor"; } }
        #endregion

        #region IShieldProficiency Members
        public bool IsProficientWithShield(bool tower, int powerLevel)
        {
            return (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);
        }

        public bool IsProficientWith(ShieldBase shield, int powerLevel)
        {
            return IsProficientWithShield(shield.Tower, powerLevel);
        }

        string IShieldProficiency.Description { get { return @"All shields"; } }
        #endregion
    }
}
