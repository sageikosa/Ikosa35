using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LocatorIndexLayer
    {
        public LocatorIndexLayer()
        {
            _Strips = new Dictionary<int, LocatorIndexStrip>(4);
        }

        private Dictionary<int, LocatorIndexStrip> _Strips;

        private IEnumerable<KeyValuePair<int, LocatorIndexStrip>> GetStrips(Locator locator, bool addIfMissing = true)
        {
            var _rgn = locator.GeometricRegion;
            var _lo = _rgn.LowerY;
            var _hi = _rgn.UpperY;
            for (var _idx = _lo; _idx <= _hi; _idx++)
            {
                if (!_Strips.ContainsKey(_idx))
                {
                    if (addIfMissing)
                    {
                        var _new = new LocatorIndexStrip();
                        _Strips.Add(_idx, _new);
                        yield return new KeyValuePair<int, LocatorIndexStrip>(_idx, _new);
                    }
                }
                else
                {
                    yield return new KeyValuePair<int, LocatorIndexStrip>(_idx, _Strips[_idx]);
                }
            }
            yield break;
        }

        public void Add(Locator locator)
        {
            foreach (var _strip in GetStrips(locator))
            {
                _strip.Value.Add(locator);
            }
        }

        public void Remove(Locator locator)
        {
            List<int> _empty = null;
            foreach (var _strip in GetStrips(locator, false))
            {
                _strip.Value.Remove(locator);
                if (_strip.Value.IsEmpty)
                {
                    _empty ??= [];
                    _empty.Add(_strip.Key);
                }
            }
            if (_empty?.Any() ?? false)
            {
                foreach (var _e in _empty)
                {
                    _Strips.Remove(_e);
                }
            }
        }

        public IEnumerable<Locator> GetLocators(ICellLocation location, PlanarPresence locPlanes)
        {
            var _idx = location.Y;
            if (_Strips.TryGetValue(_idx, out var _strip))
            {
                return _strip.GetLocators(location, locPlanes);
            }
            return new Locator[] { };
        }

        public bool IsEmpty => _Strips.Count == 0;


        public IEnumerable<Locator> GetLocatorsInRegion(IGeometricRegion region, PlanarPresence locPlanes)
        {
            for (var _idx = region.LowerY; _idx <= region.UpperY; _idx++)
            {
                if (_Strips.TryGetValue(_idx, out var _strip))
                {
                    foreach (var _loc in _strip.GetLocatorsInRegion(region, locPlanes))
                    {
                        yield return _loc;
                    }
                }
            }
            yield break;
        }

        public IEnumerable<Locator> AllLocators(int layer)
            => _Strips.SelectMany(_kvp => _kvp.Value.AllLocators(layer, _kvp.Key)).Distinct();
    }
}
