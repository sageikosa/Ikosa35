using System;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class WarInfluence : Influence, IWeaponProficiency
    {
        public WarInfluence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(devotion, influenceClass)
        {
            _WpnFocus = null;
        }

        public override string Name { get { return @"War Influence"; } }
        public override string Description { get { return @"Weapon proficiency and weapon focus with devotional weapon"; } }
        public override object Clone() { return new WarInfluence(Devotion, InfluenceClass); }

        private FeatBase _WpnFocus;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            var _critter = Creature;
            if (_critter != null)
            {
                // proficiency
                _critter.Proficiencies.Add(this);

                // weapon focus
                Type _wfType = typeof(WeaponFocusFeat<>).MakeGenericType(new Type[] { Devotion.WeaponType });
                _WpnFocus = Activator.CreateInstance(_wfType, this, PowerLevel) as FeatBase;
                _WpnFocus.IgnorePreRequisite = true;
                _WpnFocus.BindTo(_critter);
            }
        }

        protected override void OnDeactivate(object source)
        {
            var _critter = Creature;
            if (_critter != null)
            {
                if (_WpnFocus != null)
                {
                    // weapon focus
                    _WpnFocus.UnbindFromCreature();
                    _WpnFocus = null;
                }

                // proficiency
                _critter.Proficiencies.Remove(this);
            }
            base.OnDeactivate(source);
        }

        #region IWeaponProficiency Members

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
        {
            // no blanket proficiency granted
            return false;
        }

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
        {
            return this.IsProficientWithWeapon(typeof(WpnType), powerLevel);
        }

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
        {
            return this.IsProficientWithWeapon(weapon.GetType(), powerLevel);
        }

        public bool IsProficientWithWeapon(Type type, int powerLevel)
        {
            var _wpnType = Devotion.WeaponType;
            if (_wpnType != null)
            {
                return (powerLevel >= PowerLevel) && _wpnType.IsAssignableFrom(type);
            }
            return false;
        }

        #endregion
    }
}
