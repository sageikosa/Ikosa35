using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;

namespace Uzi.Ikosa.Creatures.BodyType
{
    /// <summary>Manages natural weapons: reslots automatic weapons (such as claws) when not holding other items.  Provides proficiencies.</summary>
    [Serializable]
    public class NaturalWeaponSet : ICreatureBind, IEnumerable<NaturalWeapon>, IMonitorChange<ISlottedItem>,
        IWeaponProficiency
    {
        /// <summary>Manages natural weapons: reslots automatic weapons (such as claws) when not holding other items.  Provides proficiencies.</summary>
        public NaturalWeaponSet()
        {
            _Critter = null;
            _Weapons = new List<NaturalWeapon>();
        }

        #region private data
        private Creature _Critter;
        private List<NaturalWeapon> _Weapons;
        #endregion

        #region ICreatureBound Members

        public Creature Creature
        {
            get { return _Critter; }
        }

        #endregion

        #region IEnumerable<NaturalWeapon> Members

        public IEnumerator<NaturalWeapon> GetEnumerator()
        {
            foreach (var _nw in _Weapons)
                yield return _nw;
            yield break;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var _nw in _Weapons)
                yield return _nw;
            yield break;
        }

        #endregion

        #region public void Add(NaturalWeapon weapon)
        public void Add(NaturalWeapon weapon)
        {
            if (!_Weapons.Contains(weapon))
            {
                weapon.ItemSizer.ExpectedCreatureSize = Creature.Body.Sizer.NaturalSize;
                _Weapons.Add(weapon);
                BindWeapon(weapon);
            }
        }
        #endregion

        #region public void Remove(NaturalWeapon weapon)
        /// <summary>
        /// removes weapon from set, slots and proficiencies
        /// </summary>
        /// <param name="weapon"></param>
        public void Remove(NaturalWeapon weapon)
        {
            if (_Weapons.Contains(weapon))
            {
                _Weapons.Remove(weapon);
                UnbindWeapon(weapon);
            }
            if (!_Weapons.Any() && (Creature != null))
                Creature.Proficiencies.Remove(this);
        }
        #endregion

        public void RemoveAll()
        {
            foreach (var _wpn in _Weapons.ToList())
                Remove(_wpn);
        }

        #region private void BindWeapon(NaturalWeapon weapon)
        private void BindWeapon(NaturalWeapon weapon)
        {
            if (_Critter != null)
            {
                weapon.Possessor = Creature;
                if (weapon.AlwaysOn)
                {
                    // find a matching weapon slot
                    ItemSlot _slot = null;
                    if (weapon.SlotSubType != string.Empty)
                    {
                        _slot = Creature.Body.ItemSlots[weapon.SlotType, weapon.SlotSubType];
                    }
                    else
                    {
                        _slot = Creature.Body.ItemSlots[weapon.SlotType];
                    }

                    if (_slot != null)
                    {
                        // put the weapon in the slot, keep track of it, and watch the slot for items being unwielded so the weapon can be re-added
                        weapon.SetItemSlot(_slot);
                        _slot.AddChangeMonitor(this);
                    }
                }
                Creature.Proficiencies.Add(this);
            }
        }
        #endregion

        #region private void UnbindWeapon(NaturalWeapon weapon)
        private void UnbindWeapon(NaturalWeapon weapon)
        {
            // unslot
            if (weapon.MainSlot != null)
            {
                weapon.MainSlot.RemoveChangeMonitor(this);
                weapon.ClearSlots();
            }

            // then disown
            weapon.Possessor = null;
        }
        #endregion

        public int Count { get { return _Weapons.Count; } }

        #region ICreatureBind Members

        public bool BindTo(Creature creature)
        {
            _Critter = creature;
            foreach (var _wpn in _Weapons)
            {
                // bind weapons
                BindWeapon(_wpn);
            }
            if (_Weapons.Any())
            {
                // ensure proficiency
                creature.Proficiencies.Add(this);
            }
            return true;
        }

        public void UnbindFromCreature()
        {
            foreach (var _wpn in _Weapons)
            {
                // unbind weapons
                UnbindWeapon(_wpn);
            }
            Creature.Proficiencies.Remove(this);
            _Critter = null;
        }

        #endregion

        #region IMonitorChange<ISlottedItem> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<ISlottedItem> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args)
        {
            // if the item slots being inspected can hold natural weapons, re-slot them immediately upon being cleared
            if (args.NewValue == null)
            {
                var _slot = sender as ItemSlot;
                if (_slot != null)
                {
                    // look for unslotted natural weapon that goes with this slot
                    foreach (var _natural in _Weapons.Where(_nat => _nat.MainSlot == null))
                    {
                        if (_natural.SlotType.Equals(_slot.SlotType) && _natural.SlotSubType.Equals(_slot.SubType))
                        {
                            // put it back into the slot
                            _natural.SetItemSlot(_slot);
                            return;
                        }
                    }

                    // if we get here, then we couldn't find a weapon, so this monitoring if invalid (ergo, cleanup)
                    _slot.RemoveChangeMonitor(this);
                }
            }
        }
        #endregion

        #region IWeaponProficiency Members

        bool IWeaponProficiency.IsProficientWith(WeaponProficiencyType profType, int powerLevel)
        {
            return (profType == WeaponProficiencyType.Natural);
        }

        bool IWeaponProficiency.IsProficientWith<WpnType>(int powerLevel)
        {
            return ((IWeaponProficiency)this).IsProficientWithWeapon(typeof(WpnType), powerLevel);
        }

        bool IWeaponProficiency.IsProficientWith(IWeapon weapon, int powerLevel)
        {
            return ((IWeaponProficiency)this).IsProficientWithWeapon(weapon.GetType(), powerLevel);
        }

        bool IWeaponProficiency.IsProficientWithWeapon(Type type, int powerLevel)
        {
            return Creature.Body.NaturalWeapons.Any(_w => _w.GetType() == type);
        }

        string IWeaponProficiency.Description
        {
            get { return @"Natural weapons"; }
        }

        #endregion
    }
}
