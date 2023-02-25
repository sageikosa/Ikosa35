using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Uzi.Core.Contracts;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public abstract class ModuleNode : ModuleElement, INotifyPropertyChanged, IBasePart, IModuleNode, ICorePartNameManager
    {
        #region non-serialized: package-relative state
        [NonSerialized, JsonIgnore]
        private ICorePartNameManager _NameManager = null;

        [NonSerialized, JsonIgnore]
        private PackagePart _NodePart;
        #endregion

        protected ModuleNode(Description description)
            : base(description)
        {
        }

        protected abstract void OnSetPackagePart();

        protected void SetPackagePart(PackagePart part)
        {
            _NodePart = part;
            OnSetPackagePart();
        }

        public string Name => Description.Message.ToSafeString();
        public abstract IEnumerable<ICorePart> Relationships { get; }
        public abstract string TypeName { get; }

        public abstract string GroupName { get; }

        #region INotifyPropertyChanged Members

        protected void DoPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public ICorePartNameManager NameManager { get; set; }
        public ICorePartNameManager NameManager
        {
            get => _NameManager;
            set => _NameManager ??= value;
        }
        #endregion

        #region public string BindableName { get; set; }
        public string BindableName
        {
            get => Name;
            set
            {
                if (value == null)
                    return;
                if (!(_NameManager?.CanUseName(value, GetType()) ?? false))
                {
                    return;
                }
                Description.Message = value.ToSafeString();
                DoPropertyChanged(nameof(Name));
                DoPropertyChanged(nameof(BindableName));
                DoPropertyChanged(nameof(Description));
            }
        }
        #endregion

        public PackagePart Part => _NodePart;

        public abstract void Close();

        protected abstract void OnRefreshPart();

        public virtual void RefreshPart(PackagePart part)
        {
            _NodePart = part;
            if (_NodePart != null)
            {
                OnRefreshPart();
            }
        }

        protected abstract string NodeExtension { get; }
        protected abstract string ContentType { get; }
        protected abstract string RelationshipType { get; }

        protected void DoSaveFolder(Package parent, string folderFile)
        {
            // create new part
            var _folder = UriHelper.ConcatRelative(new Uri(@"/", UriKind.Relative), $@"{Name}.{NodeExtension}");
            var _content = UriHelper.ConcatRelative(_folder, folderFile);
            _NodePart = parent.CreatePart(_content, ContentType, CompressionOption.Normal);
            parent.CreateRelationship(_content, TargetMode.Internal, RelationshipType);

            DoSave(_folder);
        }

        protected void DoSaveFolder(PackagePart parent, Uri baseUri, string folderFile)
        {
            // create new part
            var _folder = UriHelper.ConcatRelative(baseUri, $@"{Name}.{NodeExtension}");
            var _content = UriHelper.ConcatRelative(_folder, folderFile);
            _NodePart = parent.Package.CreatePart(_content, ContentType, CompressionOption.Normal);
            parent.CreateRelationship(_content, TargetMode.Internal, RelationshipType);

            DoSave(_folder);
        }

        protected void DoSaveFile(Package parent)
        {
            var _base = UriHelper.ConcatRelative(new Uri(@"/", UriKind.Relative), Name);
            _NodePart = parent.CreatePart(_base, ContentType, CompressionOption.Normal);
            parent.CreateRelationship(_base, TargetMode.Internal, RelationshipType);
            DoSave();
        }

        protected void DoSaveFile(PackagePart parent, Uri baseUri)
        {
            var _base = UriHelper.ConcatRelative(baseUri, Name);
            _NodePart = parent.Package.CreatePart(_base, ContentType, CompressionOption.Normal);
            parent.CreateRelationship(_base, TargetMode.Internal, RelationshipType);
            DoSave();
        }

        private void DoSave()
        {
            // part
            using var _stream = _NodePart.GetStream(FileMode.Create, FileAccess.ReadWrite);
            IFormatter _fmt = new BinaryFormatter();
            _fmt.Serialize(_stream, this);
        }

        private void DoSave(Uri baseUri)
        {
            // part and additional contents
            DoSave();
            OnDoSave(baseUri);
        }

        protected abstract void OnDoSave(Uri baseUri);

        public abstract void Save(Package parent);
        public abstract void Save(PackagePart parent, Uri baseUri);

        public bool CanUseName(string name, Type partType)
            => false;

        public void Rename(string oldName, string newName, Type partType)
        {
        }
    }
}
