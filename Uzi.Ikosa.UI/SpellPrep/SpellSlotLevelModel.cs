using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.UI.SpellPrep
{
    public abstract class SpellSlotLevelModel<SlotType> : INotifyPropertyChanged
        where SlotType : SpellSlot, new()
    {
        protected SpellSlotLevelModel(SpellSlotLevel<SlotType> level)
        {
            _Level = level;
        }

        #region private data
        protected List<SpellSlotModel<SlotType>> _Slots;
        private SpellSlotLevel<SlotType> _Level;
        #endregion

        /// <summary>Ikosa Framework object</summary>
        public SpellSlotLevel<SlotType> Level => _Level;

        public IEnumerable<SpellSlotModel<SlotType>> SpellSlots => _Slots.Select(_s => _s).ToList();

        public void RefreshSlots()
            => DoPropertyChanged(nameof(SpellSlots));

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
