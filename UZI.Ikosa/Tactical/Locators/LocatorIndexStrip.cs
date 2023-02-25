using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LocatorIndexStrip
    {
        public LocatorIndexStrip()
        {
            _Cells = new Dictionary<int, List<Locator>>(4);
        }

        private Dictionary<int, List<Locator>> _Cells;

        private IEnumerable<KeyValuePair<int, List<Locator>>> GetLists(Locator locator, bool addIfMissing = true)
        {
            var _rgn = locator.GeometricRegion;
            var _lo = _rgn.LowerX;
            var _hi = _rgn.UpperX;
            for (var _idx = _lo; _idx <= _hi; _idx++)
            {
                if (!_Cells.ContainsKey(_idx))
                {
                    if (addIfMissing)
                    {
                        var _new = new List<Locator>();
                        _Cells.Add(_idx, _new);
                        yield return new KeyValuePair<int, List<Locator>>(_idx, _new);
                    }
                }
                else
                {
                    yield return new KeyValuePair<int, List<Locator>>(_idx, _Cells[_idx]);
                }
            }
            yield break;
        }

        public void Add(Locator locator)
        {
            foreach (var _nugget in GetLists(locator))
            {
                if (!_nugget.Value.Contains(locator))
                    _nugget.Value.Add(locator);
            }
        }


        public void Remove(Locator locator)
        {
            List<int> _empty = null;
            foreach (var _cell in GetLists(locator, false))
            {
                _cell.Value.Remove(locator);
                if (_cell.Value.Count == 0)
                {
                    if (_empty == null)
                        _empty = new List<int>();
                    _empty.Add(_cell.Key);
                }
            }
            if (_empty?.Any() ?? false)
                foreach (var _e in _empty)
                    _Cells.Remove(_e);
        }

        public IEnumerable<Locator> GetLocators(ICellLocation location, PlanarPresence locPlanes)
        {
            var _idx = location.X;
            if (_Cells.TryGetValue(_idx, out var _cell))
            {
                foreach (var _loc in _cell)
                {
                    if (_loc.GeometricRegion.ContainsCell(location))
                    {
                        if (locPlanes.HasOverlappingPresence(_loc.PlanarPresence))
                        {
                            yield return _loc;
                        }
                    }
                }
            }
            yield break;
        }

        public bool IsEmpty => _Cells.Count == 0;

        public IEnumerable<Locator> GetLocatorsInRegion(IGeometricRegion region, PlanarPresence locPlanes)
        {
            for (var _idx = region.LowerX; _idx <= region.UpperX; _idx++)
            {
                if (_Cells.TryGetValue(_idx, out var _locs))
                {
                    foreach (var _loc in _locs)
                    {
                        if (locPlanes.HasOverlappingPresence(_loc.PlanarPresence))
                        {
                            yield return _loc;
                        }
                    }
                }
            }
            yield break;
        }

        public IEnumerable<Locator> AllLocators(int layer, int strip)
        {
            foreach (var _kvp in _Cells)
            {
                foreach (var _loc in _kvp.Value)
                {
                    Debug.WriteLine($"{layer}\t{strip}\t{_kvp.Key}\t{_loc.ICore.ID}\t{_loc.Name}");
                    yield return _loc;
                }
            }
            yield break;
        }
    }
}
