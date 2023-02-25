using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LocatorIndex
    {
        public LocatorIndex(LocalMap map)
        {
            _Map = map;
            _Layers = new Dictionary<int, LocatorIndexLayer>(8);
        }

        private LocalMap _Map;
        private Dictionary<int, LocatorIndexLayer> _Layers;

        private IEnumerable<KeyValuePair<int, LocatorIndexLayer>> GetLayers(Locator locator, bool addIfMissing = true)
        {
            var _rgn = locator.GeometricRegion;
            var _lo = _rgn.LowerZ;
            var _hi = _rgn.UpperZ;
            for (var _idx = _lo; _idx <= _hi; _idx++)
            {
                if (!_Layers.ContainsKey(_idx))
                {
                    if (addIfMissing)
                    {
                        var _new = new LocatorIndexLayer();
                        _Layers.Add(_idx, _new);
                        yield return new KeyValuePair<int, LocatorIndexLayer>(_idx, _new);
                    }
                }
                else
                {
                    yield return new KeyValuePair<int, LocatorIndexLayer>(_idx, _Layers[_idx]);
                }
            }
            yield break;
        }

        public void ReIndex(Locator locator)
        {
            Remove(locator);
            Add(locator);
        }

        public void Add(Locator locator)
        {
            foreach (var _layer in GetLayers(locator))
            {
                _layer.Value.Add(locator);
            }
        }

        public void Remove(Locator locator)
        {
            List<int> _empty = null;
            foreach (var _layer in GetLayers(locator, false))
            {
                _layer.Value.Remove(locator);
                if (_layer.Value.IsEmpty)
                {
                    if (_empty == null)
                        _empty = new List<int>();
                    _empty.Add(_layer.Key);
                }
            }
            if (_empty?.Any() ?? false)
                foreach (var _e in _empty)
                    _Layers.Remove(_e);
        }

        public IEnumerable<Locator> GetLocators(ICellLocation location, PlanarPresence locPlanes)
        {
            if (_Layers.TryGetValue(location.Z, out var _layer))
            {
                return _layer.GetLocators(location, locPlanes);
            }
            return new List<Locator>();
        }

        private IEnumerable<Locator> GetAllLocatorsInRegion(IGeometricRegion region, PlanarPresence locPlanes)
        {
            for (var _idx = region.LowerZ; _idx <= region.UpperZ; _idx++)
            {
                if (_Layers.TryGetValue(_idx, out var _layer))
                {
                    foreach (var _loc in _layer.GetLocatorsInRegion(region, locPlanes))
                        yield return _loc;
                }
            }
            yield break;
        }

        public IEnumerable<Locator> GetLocatorsInRegion(IGeometricRegion region, PlanarPresence locPlanes)
            => GetAllLocatorsInRegion(region, locPlanes).Distinct();

        public IEnumerable<Locator> AllLocators()
            => _Layers.SelectMany(_kvp => _kvp.Value.AllLocators(_kvp.Key)).Distinct();
    }
}
