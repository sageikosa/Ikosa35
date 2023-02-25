using System;
using Uzi.Core;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Uzi.Ikosa
{
    [Serializable]
    public abstract class Sizer : IMonitorChange<DeltaValue>, IControlChange<Size>
    {
        protected Sizer(Size size)
        {
            // size
            _Size = size;
            _NaturalSize = _Size;
            _SizeOffset = new ConstDeltable(0);
            _SizeOffset.AddChangeMonitor(this);
            _ValueCtrl = new ChangeController<Size>(this, size);
        }

        #region private data
        protected Size _Size;
        protected Size _NaturalSize;
        private ConstDeltable _SizeOffset;
        private ChangeController<Size> _ValueCtrl;
        #endregion

        /// <summary>Natural size (not under any sizing effects)</summary>
        public virtual Size NaturalSize
        {
            get { return _NaturalSize; }
            set
            {
                if (value != null)
                {
                    _NaturalSize = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(@"NaturalSize"));
                    SyncSize();
                }
            }
        }

        /// <summary>Allow temporary adjustments to size order</summary>
        public ConstDeltable SizeOffset { get { return _SizeOffset; } }

        /// <summary>
        /// Ensure expressed size accounts for current NaturalSize and any offset
        /// </summary>
        private void SyncSize()
        {
            if (_SizeOffset.EffectiveValue == 0)
                Size = _NaturalSize;
            else
                Size = _NaturalSize.OffsetSize(_SizeOffset.EffectiveValue);
        }

        /// <summary>Effective current expressed size (same as natural size of SizeOffset=0)</summary>
        public Size Size
        {
            get { return _Size; }
            internal set
            {
                // only if there is a real change
                if (_Size.Order != value.Order)
                {
                    _ValueCtrl.DoPreValueChanged(value);
                    _Size = value;
                    OnSizeChange();
                    _ValueCtrl.DoValueChanged(value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(@"Size"));
                }
            }
        }

        protected virtual void OnSizeChange()
        {
        }

        #region IControlChange<Size> Members
        public void AddChangeMonitor(IMonitorChange<Size> subscriber) { _ValueCtrl.AddChangeMonitor(subscriber); }
        public void RemoveChangeMonitor(IMonitorChange<Size> subscriber) { _ValueCtrl.RemoveChangeMonitor(subscriber); }
        #endregion

        #region IMonitorChange<DeltaValue> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            SyncSize();
        }
        #endregion

        protected void DoPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

    }
}
