using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.UI.SpellPrep
{
    public class SpellSlotModel<SlotType> : INotifyPropertyChanged
        where SlotType : SpellSlot, new()
    {
        #region state
        protected SpellSlotLevelModel<SlotType> _Level;
        private readonly RelayCommand _Recharge;
        private readonly RelayCommand<double> _Discharge;
        private SlotType _Slot;
        private double _Time;
        protected List<object> _Menus;
        #endregion

        public SpellSlotModel(SpellSlotLevelModel<SlotType> level, SlotType slot, double time)
        {
            _Level = level;
            _Slot = slot;
            _Time = time;
            _Recharge = new RelayCommand(() =>
            {
                _Slot.RechargeSlot(_Time);
                DoPropertyChanged(nameof(IsCharged));
                _Level.RefreshSlots();
            });
            _Discharge = new RelayCommand<double>((offset) =>
            {
                _Slot.UseSlot(_Time - (Hour.UnitFactor * offset));
                DoPropertyChanged(nameof(IsCharged));
                _Level.RefreshSlots();
            });
            _Menus = new List<object>
            {
                new MenuViewModel
                {
                    Header = @"Recharge",
                    Command = Recharge
                }
            };
        }

        protected virtual void AddMenuItems(List<object> menus)
        {
            AddDischarge(menus);
        }

        protected void AddDischarge(List<object> menus)
        {
            menus.Add(new MenuViewModel
            {
                Header = @"Discharge Now",
                Command = Discharge,
                Parameter = 0d
            });
            menus.Add(new MenuViewModel
            {
                Header = @"Discharge 1 hour ago",
                Command = Discharge,
                Parameter = 1d
            });
            menus.Add(CreateDischarge(2d));
            menus.Add(CreateDischarge(3d));
            menus.Add(CreateDischarge(4d));
            menus.Add(CreateDischarge(5d));
            menus.Add(CreateDischarge(6d));
            menus.Add(CreateDischarge(7d));
            menus.Add(CreateDischarge(8d));
        }

        private MenuViewModel CreateDischarge(double offset)
        {
            return new MenuViewModel
            {
                Header = string.Format(@"Discharge {0} hours ago", offset),
                Command = Discharge,
                Parameter = offset
            };
        }

        public SlotType SpellSlot
        {
            get => _Slot;
            set => _Slot = value;
        }

        public bool IsCharged => _Slot.IsCharged;

        public RelayCommand Recharge => _Recharge;
        public RelayCommand<double> Discharge => _Discharge;
        public List<object> Menus => _Menus;

        #region INotifyPropertyChanged Members
        protected void DoPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
