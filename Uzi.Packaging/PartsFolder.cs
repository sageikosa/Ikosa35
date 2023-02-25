using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Uzi.Packaging
{
    /// <summary>
    /// Parts folder container
    /// </summary>
    public class PartsFolder : ICorePart, INotifyPropertyChanged
    {
        #region constructor
        public PartsFolder(ICorePart parent, string name, IEnumerable<ICorePart> parts, Type partType)
        {
            _Parent = parent;
            _Name = name;
            _Parts = parts;
            _PartType = partType;
        }

        public PartsFolder(ICorePart parent, string name, Type partType, params ICorePart[] parts)
            : this(parent, name, parts, partType)
        {
        }
        #endregion

        #region data
        private readonly ICorePart _Parent;
        private readonly string _Name;
        private readonly IEnumerable<ICorePart> _Parts;
        private readonly Type _PartType;
        private bool _NameSort = true;
        private bool _HideTree = false;
        #endregion

        public ICorePart Parent => _Parent;
        public string Name => _Parts.Any() ? $@"{_Name} ({_Parts.Count()})" : _Name;

        public bool NameSort { get => _NameSort; set => _NameSort = value; }
        public bool HideTree { get => _HideTree; set => _HideTree = value; }

        public IEnumerable<ICorePart> FolderContents
            => !NameSort
            ? _Parts.Select(_p => _p)
            : _Parts.OrderBy(_rel => _rel.Name);

        public IEnumerable<ICorePart> Relationships
            => !HideTree
            ? FolderContents
            : new ICorePart[] { };

        public Type PartType => _PartType;
        public string TypeName => GetType().FullName;

        /// <summary>Call to inform listeners that the relationships may have changed</summary>
        public void ContentsChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Relationships)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FolderContents)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
