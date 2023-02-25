using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Packaging;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Uzi.Packaging
{
    public abstract class BasePart : IBasePart, INotifyPropertyChanged
    {
        #region protected construction
        protected BasePart(ICorePartNameManager manager, PackagePart part, string name)
        {
            _Name = name.ToSafeString();
            _Part = part;
            _NameManager = manager;
        }

        protected BasePart(ICorePartNameManager manager, string name)
        {
            _Name = name.ToSafeString();
            _Part = null;
            _NameManager = manager;
        }
        #endregion

        #region state
        private string _Name;
        protected PackagePart _Part;
        private ICorePartNameManager _NameManager;
        #endregion

        /// <summary>Reference to the name manager to ensure renaming doesn't cause a name collision</summary>
        public ICorePartNameManager NameManager
        {
            get => _NameManager;
            set => _NameManager ??= value;
        }

        protected virtual void OnBindableNameChange(string oldVal)
        {
        }

        #region public string BindableName { get; set; }
        /// <summary>Name suitable for editing in a WPF UI</summary>
        public string BindableName
        {
            get { return _Name; }
            set
            {
                if (value == null)
                    return;
                if (!(NameManager?.CanUseName(value, GetType()) ?? false))
                    return;
                var _oldVal = _Name;
                _Name = value.ToSafeString();
                OnBindableNameChange(_oldVal);
                NameManager?.Rename(_oldVal, _Name, GetType());
                DoPropertyChanged(nameof(Name));
                DoPropertyChanged(nameof(BindableName));
            }
        }
        #endregion

        public string Name => _Name;

        public PackagePart Part => _Part;

        #region INotifyPropertyChanged Members

        protected void DoPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ICorePart Members

        public abstract IEnumerable<ICorePart> Relationships { get; }
        public abstract string TypeName { get; }

        #endregion

        /// <summary>If the part isn't backed by an PackagePart in this Package, one will be created in the parent Package</summary>
        public abstract void Save(Package parent);

        /// <summary>If the part isn't backed by an PackagePart in this Package, one will be created in the parent PackagePart</summary>
        public abstract void Save(PackagePart parent, Uri baseUri);

        public abstract void Close();

        protected abstract void OnRefreshPart();

        public void RefreshPart(PackagePart part)
        {
            _Part = part;
            OnRefreshPart();
        }
    }
}
