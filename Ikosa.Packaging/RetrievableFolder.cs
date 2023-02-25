using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Ikosa.Packaging
{
    /// <summary>
    /// Parts folder container
    /// </summary>
    public class RetrievableFolder : IRetrievablePart, INotifyPropertyChanged
    {
        #region state
        private readonly IRetrievablePart _Parent;
        private string _Name;
        private readonly IEnumerable<IRetrievablePart> _Parts;
        private readonly string _PartType;
        #endregion

        #region ctor(...)
        public RetrievableFolder(IRetrievablePart parent, string name, IEnumerable<IRetrievablePart> parts, string partType)
        {
            _Parent = parent;
            _Name = name;
            _Parts = parts;
            _PartType = partType;
        }

        public RetrievableFolder(IRetrievablePart parent, string name, string partType, params IRetrievablePart[] parts)
            : this(parent, name, parts, partType)
        {
        }
        #endregion

        public IRetrievablePart Parent => _Parent;
        public string PartName => _Name;

        public IEnumerable<IRetrievablePart> Parts
            => _Parts.OrderBy(_rel => _rel.PartName);

        public string PartType => _PartType;
        public string TypeName => GetType().FullName;

        /// <summary>Call to inform listeners that the relationships may have changed</summary>
        public void ContentsChanged()
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(@"Relationships"));

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
