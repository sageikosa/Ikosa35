using System;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    [Serializable]
    public class ProficiencySet : ICreatureBound, IWeaponProficiencyTreatment, IArmorProficiency, IShieldProficiency
    {
        #region Construction
        public ProficiencySet(Creature creature)
        {
            _Creature = creature;
            _WeaponProficiencies = [];
            _ArmorProficiencies = [];
            _ShieldProficiencies = [];
            _WpnDelta = new WeaponProficiencyDelta(this);
            _ArmorDelta = new ArmorProficiencyDelta(this);
            _ShldDelta = new ShieldProficiencyDelta(this);
        }
        #endregion

        #region private data
        private Collection<IWeaponProficiency> _WeaponProficiencies;
        private Collection<IArmorProficiency> _ArmorProficiencies;
        private Collection<IShieldProficiency> _ShieldProficiencies;
        private WeaponProficiencyDelta _WpnDelta;
        private ArmorProficiencyDelta _ArmorDelta;
        private ShieldProficiencyDelta _ShldDelta;
        #endregion

        public WeaponProficiencyDelta WeaponProficiencyDelta => _WpnDelta;
        public ArmorProficiencyDelta ArmorProficiencyDelta => _ArmorDelta;
        public ShieldProficiencyDelta ShieldProficiencyDelta => _ShldDelta;

        #region ICreatureBound Members
        private Creature _Creature;
        public Creature Creature
        {
            get { return _Creature; }
        }
        #endregion

        #region Add Proficiencies
        public void Add(IShieldProficiency proficiency)
        {
            if (!_ShieldProficiencies.Contains(proficiency))
            {
                _ShieldProficiencies.Add(proficiency);
            }
        }

        public void Add(IArmorProficiency proficiency)
        {
            if (!_ArmorProficiencies.Contains(proficiency))
            {
                _ArmorProficiencies.Add(proficiency);
            }
        }

        public void Add(IWeaponProficiency proficiency)
        {
            if (!_WeaponProficiencies.Contains(proficiency))
            {
                _WeaponProficiencies.Add(proficiency);
            }
        }
        #endregion

        #region Remove Proficiencies
        public void Remove(IShieldProficiency proficiency)
        {
            if (_ShieldProficiencies.Contains(proficiency))
            {
                _ShieldProficiencies.Remove(proficiency);
            }
        }

        public void Remove(IArmorProficiency proficiency)
        {
            if (_ArmorProficiencies.Contains(proficiency))
            {
                _ArmorProficiencies.Remove(proficiency);
            }
        }

        public void Remove(IWeaponProficiency proficiency)
        {
            if (_WeaponProficiencies.Contains(proficiency))
            {
                _WeaponProficiencies.Remove(proficiency);
            }
        }
        #endregion

        #region IShieldProficiency Members
        public bool IsProficientWithShield(bool tower, int powerLevel)
        {
            foreach (IShieldProficiency _proficiency in _ShieldProficiencies)
            {
                if (_proficiency.IsProficientWithShield(tower, powerLevel))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsProficientWith(ShieldBase shield, int powerLevel)
        {
            return IsProficientWithShield(shield.Tower, powerLevel);
        }

        string IShieldProficiency.Description => @"N/A";
        #endregion

        #region IArmorProficiency Members
        /// <summary>
        /// Used for Feat Prerequisite, CharSheet info and assistance in single armor instance proficiency
        /// </summary>
        public bool IsProficientWith(ArmorProficiencyType profType, int powerLevel)
        {
            foreach (IArmorProficiency _proficiency in _ArmorProficiencies)
            {
                if (_proficiency.IsProficientWith(profType, powerLevel))
                {
                    return true;
                }
            }
            return profType == ArmorProficiencyType.None;
        }

        public bool IsProficientWith(ArmorBase armor, int powerLevel)
        {
            // when checking specific armor, either the exposed proficiency type or the specific armor will work
            foreach (IArmorProficiency _proficiency in _ArmorProficiencies)
            {
                if (_proficiency.IsProficientWith(armor.ProficiencyType, powerLevel))
                {
                    return true;
                }

                if (_proficiency.IsProficientWith(armor, powerLevel))
                {
                    return true;
                }
            }
            return armor.ProficiencyType == ArmorProficiencyType.None;
        }

        string IArmorProficiency.Description => @"N/A";
        #endregion

        #region IWeaponProficiency Members
        /// <summary>
        /// Checks whether the creature is proficient with the broad category of weapon (simple/martial).  
        /// Used in Feat prerequisites, charSheet info and assitance in specific weapon instances.
        /// </summary>
        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
        {
            foreach (var _proficiency in _WeaponProficiencies)
            {
                if (_proficiency.IsProficientWith(profType, powerLevel))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether the creature is (unconditionally) proficient with the specific weapon type.  
        /// A dwarf with martial weapon proficiency should indicate that he is proficient with the dwarven waraxe.
        /// </summary>
        public bool IsProficientWithWeapon(Type type, int powerLevel)
        {
            // if not exotic, try to check against the unconditional weapon proficiency type
            if (!typeof(IExoticWeapon).IsAssignableFrom(type))
            {
                if (typeof(IMartialWeapon).IsAssignableFrom(type))
                {
                    // since it is a martial weapon (normally), see if we are proficient with martial weapons
                    if (IsProficientWith(WeaponProficiencyType.Martial, powerLevel))
                    {
                        return true;
                    }
                }
                else
                {
                    // since it is neither exotic nor martial, see if we are proficient with simple weapons
                    if (IsProficientWith(WeaponProficiencyType.Simple, powerLevel))
                    {
                        return true;
                    }
                }
            }

            // since the broad categories did not hit, try for specific types 
            // Dwarven Waraxe wielded by dwarf should hit here if the dwarf has martial weapon proficiency
            //   (ie, the dwarf should implement WpnType=DwarvenWaraxe to recheck martial proficiencies even 
            //        though the dwarven waraxe is exotic otherwise )
            foreach (var _proficiency in _WeaponProficiencies)
            {
                if (_proficiency.IsProficientWithWeapon(type, powerLevel))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether the creature is (unconditionally) proficient with the specific weapon type.  
        /// A dwarf with martial weapon proficiency should indicate that he is proficient with the dwarven waraxe.
        /// </summary>
        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
        {
            return IsProficientWithWeapon(typeof(WpnType), powerLevel);
        }

        /// <summary>
        /// Checks whether the creature is proficient with the specific weapon instance as currently wielded.
        /// Without exotic proficiency, the result should vary for bastard sword and dwarven waraxe 
        /// based on how the weapon is wielded (if at all).  Results may also vary if the weapon has a 
        /// proficiency enhancement that activates when wielded.
        /// </summary>
        public bool IsProficientWith(IWeapon weapon, int powerLevel)
        {
            // if types mismatch, there is a problem
            if (weapon.GetType().Equals(weapon.GetType()))
            {
                // first check the weapon type unconditionally
                if (IsProficientWithWeapon(weapon.GetType(), powerLevel))
                {
                    return true;
                }

                foreach (var _proficiency in _WeaponProficiencies)
                {
                    // some exotic weapons may change their proficiency-type to martial if wielded with 2-hands
                    if (_proficiency.IsProficientWith(weapon.ProficiencyType, powerLevel))
                    {
                        return true;
                    }

                    // then check if the specific weapon has a proficiency item (such as an enhancement)
                    if (_proficiency.IsProficientWith(weapon, powerLevel))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        string IWeaponProficiency.Description { get { return "N/A"; } }
        #endregion

        #region public Collection<string> ProficiencyDescriptions { get; }
        /// <summary>Returns a new collection of proficiency descriptions</summary>
        public Collection<string> ProficiencyDescriptions
            => new Collection<string>(
                _WeaponProficiencies.Select(_wp => _wp.Description).Distinct()
                .Concat(_ArmorProficiencies.Select(_ap => _ap.Description).Distinct())
                .Concat(_ShieldProficiencies.Select(_sp => _sp.Description).Distinct()).ToList());
        #endregion

        #region IWeaponProficiencyTreatment Members

        public WeaponProficiencyType WeaponTreatment(Type weaponType, int powerLevel)
        {
            var _prof = WeaponProficiencyHelper.StandardType(weaponType);
            foreach (var _treater in _WeaponProficiencies.OfType<IWeaponProficiencyTreatment>())
            {
                var _treat = _treater.WeaponTreatment(weaponType, powerLevel);
                if (_treat < _prof)
                {
                    _prof = _treat;
                }
            }
            return _prof;
        }

        public WeaponProficiencyType WeaponTreatment(IWeapon weapon, int powerLevel)
        {
            var _prof = WeaponProficiencyHelper.StandardType(weapon.GetType());
            foreach (var _treater in _WeaponProficiencies.OfType<IWeaponProficiencyTreatment>())
            {
                var _treat = _treater.WeaponTreatment(weapon, powerLevel);
                if (_treat < _prof)
                {
                    _prof = _treat;
                }
            }
            return _prof;
        }

        #endregion
    }
}
