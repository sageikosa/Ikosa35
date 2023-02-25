using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;

namespace Ikosa.Packaging
{
    public abstract class StorablePart : IStorablePart, INotifyPropertyChanged
    {
        protected StorablePart(IRetrievablePartNameManager manager, string name)
        {
            _Name = name.ToSafeString();
            _NameManager = manager;
        }

        #region state
        private string _Name;
        private IRetrievablePartNameManager _NameManager;
        #endregion

        #region public IRetrievablePartNameManager NameManager { get; set; }
        /// <summary>Reference to the name manager to ensure renaming doesn't cause a name collision</summary>
        public IRetrievablePartNameManager PartNameManager
        {
            get => _NameManager;
            set
            {
                if (value != null)
                    _NameManager = value;
            }
        }
        #endregion

        #region public string BindableName { get; set; }
        /// <summary>Name suitable for editing in a WPF UI</summary>
        public string MutableName
        {
            get => _Name;
            set
            {
                if (value == null)
                {
                    return;
                }
                if (!(PartNameManager?.CanUseName(value) ?? false))
                {
                    return;
                }
                var _oldVal = _Name;
                _Name = value.ToSafeString();
                PartNameManager?.Rename(_oldVal, _Name);
                DoPropertyChanged(nameof(PartName));
                DoPropertyChanged(nameof(MutableName));
            }
        }
        #endregion

        public string PartName => _Name;

        #region INotifyPropertyChanged Members

        protected void DoPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        // IRetrievablePart Members
        public abstract IEnumerable<IRetrievablePart> Parts { get; }
        public abstract string PartType { get; }

        public abstract void StorePart(ZipArchive archive, string parentPath);

        public abstract void ClosePart();

        public abstract void ReloadPart(ZipArchive archive, string parentPath);
    }
}
