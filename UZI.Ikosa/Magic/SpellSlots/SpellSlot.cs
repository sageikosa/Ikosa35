using System;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class SpellSlot
    {
        #region data
        private int _SlotLevel;
        private double _LastRechargeTime = 0;
        private double? _LastUsedTime = null;
        #endregion

        /// <summary>Level of the slot</summary>
        public int SlotLevel { get => _SlotLevel; set => _SlotLevel = value; }

        /// <summary>Indication that the slot is charged for use</summary>
        public bool IsCharged => _LastUsedTime == null;

        /// <summary>A slot cannot be recharged twice in the same day, or if used in past 8 hours</summary>
        public bool CanRecharge(double currentTime)
            => (Math.Floor(LastRechargeTime / Day.UnitFactor) < Math.Floor(currentTime / Day.UnitFactor))
            && (((currentTime - (LastUsedTime ?? LastRechargeTime)) / Hour.UnitFactor) >= 8d);

        /// <summary>Last time a slot was recharged</summary>
        public double LastRechargeTime => _LastRechargeTime;

        /// <summary>Slots cannot be recharged or abandonned if used in past 8 hours</summary>
        public double? LastUsedTime => _LastUsedTime;

        /// <summary>
        /// Clears the last use time, and sets slot recharge time; 
        /// indicating the slot is ready for use and has been recharged on the current day
        /// </summary>
        public virtual void RechargeSlot(double currentTime)
        {
            _LastRechargeTime = currentTime;
            _LastUsedTime = null;
        }

        /// <summary>Marks last use time, effectively decharging the spell slot</summary>
        public virtual void UseSlot(double? currentTime)
        {
            _LastUsedTime = currentTime;
        }

        protected SSInfo ToInfo<SSInfo>(int slotIndex, double currentTime)
            where SSInfo : SpellSlotInfo, new()
            => new SSInfo
            {
                SlotIndex = slotIndex,
                SlotLevel = SlotLevel,
                IsCharged = LastUsedTime == null,
                CanRecharge = CanRecharge(currentTime),
                Message = string.Empty
            };

        public virtual SpellSlotInfo ToSpellSlotInfo(int slotIndex, double currentTime)
            => ToInfo<SpellSlotInfo>(slotIndex, currentTime);
    }
}
