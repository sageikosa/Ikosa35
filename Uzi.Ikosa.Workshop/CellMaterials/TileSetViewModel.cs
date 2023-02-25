using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize.Packaging;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Workshop
{
    public class TileSetViewModel : INotifyPropertyChanged
    {
        public string Name => TileSet?.Name;

        public TileSet TileSet { get; set; }

        public IEnumerable<BrushCollectionPart> ResolvableBrushCollections
        {
            get { return TileSet.Map.Resources.ResolvableBrushCollections.Select(_i => _i.BrushCollectionPart); }
        }

        public BrushCollectionPart BrushCollectionPart
        {
            get
            {
                var _item = TileSet.Map.Resources.ResolvableBrushCollections
                    .FirstOrDefault(_bcp => _bcp.BrushCollectionPart.Name.Equals(TileSet.BrushCollectionKey, StringComparison.OrdinalIgnoreCase));
                if (_item != null)
                    return _item.BrushCollectionPart;
                return null;
            }
            set
            {
                if (value != null)
                    TileSet.BrushCollectionKey = value.Name;
                else
                    TileSet.BrushCollectionKey = string.Empty;
                DoPropertyChanged(@"BrushCollectionPart");
            }
        }

        private void DoPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #region INotifyPropertyChanged Members
        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
