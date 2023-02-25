using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Uzi.Packaging;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public abstract class CellMaterial : ICorePart, INotifyPropertyChanged
    {
        #region construction
        public CellMaterial(string name, LocalMap parent)
        {
            _Name = name;
            _AvailableTilings = new Dictionary<string, TileSet>();
            _DetectBlockThick = 3;
            _LocalMap = parent;
        }

        public CellMaterial(string name, double detectBlock, LocalMap parent)
        {
            _Name = name;
            _AvailableTilings = new Dictionary<string, TileSet>();
            _DetectBlockThick = detectBlock;
            _LocalMap = parent;
        }
        #endregion

        #region private data
        private Dictionary<string, TileSet> _AvailableTilings;
        private double _DetectBlockThick;
        private string _Name;
        private LocalMap _LocalMap;
        private int? _Balance;
        private int? _BaseGrip;
        private int? _DangleGrip;
        private int? _LedgeGrip;
        #endregion

        public string Name { get { return _Name; } }
        public LocalMap LocalMap { get { return _LocalMap; } internal set { _LocalMap = value; } }
        public IEnumerable<TileSet> AvailableTilings { get { return _AvailableTilings.Select(_kvp => _kvp.Value); } }

        public int? Balance { get { return _Balance; } set { _Balance = value; } }
        public int? BaseGrip { get { return _BaseGrip; } set { _BaseGrip = value; } }
        public int? DangleGrip { get { return _DangleGrip; } set { _DangleGrip = value; } }
        public int? LedgeGrip { get { return _LedgeGrip; } set { _LedgeGrip = value; } }

        public TileSet this[string key] { get { return _AvailableTilings[key]; } }

        #region public void AddTiling(TileSet tileSet)
        public void AddTiling(TileSet tileSet)
        {
            if (!_AvailableTilings.ContainsKey(tileSet.Name))
            {
                _AvailableTilings.Add(tileSet.Name, tileSet);
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(@"AvailableTilings"));
            }
        }
        #endregion

        #region public void RemoveTiling(string key)
        public void RemoveTiling(string key)
        {
            if (_AvailableTilings.ContainsKey(key))
            {
                TileSet _tSet = _AvailableTilings[key];
                _AvailableTilings.Remove(key);
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(@"AvailableTilings"));
            }
        }
        #endregion

        /// <summary>Amount of material in a cell (expressed as feet) that must be traversed for detection spells to be blocked</summary>
        /// <remarks>Set to more than 8.66 (longest diagonal) to prevent blocking at any thickness in a cell</remarks>
        public double DetectBlockingThickness { get { return _DetectBlockThick; } set { _DetectBlockThick = value; } }

        public abstract bool BlocksEffect { get; }

        public TileSet FirstTiling { get { return _AvailableTilings.Select(_kvp => _kvp.Value).FirstOrDefault(); } }

        // types: rock, dirt, earth, sand, wood, ice...as Ikosa Materials
        // TODO: characteristics...        

        #region ICorePart Members

        public IEnumerable<ICorePart> Relationships
        {
            get { yield break; }
        }

        public string TypeName { get { return this.GetType().FullName; } }

        #endregion

        #region INotifyPropertyChanged Members

        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
