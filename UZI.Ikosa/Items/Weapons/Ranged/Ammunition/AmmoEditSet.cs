using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    public class AmmoEditSet : INotifyPropertyChanged
    {
        public AmmoEditSet(IAmmunitionBundle bundle, IAmmunitionBase ammoBase, int count)
        {
            _Bundle = bundle;
            _Ammo = ammoBase;
            _Count = count;
        }

        #region data
        private IAmmunitionBundle _Bundle;
        private IAmmunitionBase _Ammo;
        private int _Count;
        #endregion

        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        public IAmmunitionBundle Bundle => _Bundle;
        public IAmmunitionBase Ammunition => _Ammo;
        public int Count
        {
            get => _Count;
            set
            {
                if (_Count > 0)
                {
                    _Count = value;
                    Bundle.SetCount(Ammunition, _Count);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Weight)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Price)));
                }
            }
        }

        public double Weight => (Ammunition?.Weight ?? 0d) * Count;

        public decimal Price => (Ammunition?.Price.BasePrice ?? 0m) * Count;
    }
}
