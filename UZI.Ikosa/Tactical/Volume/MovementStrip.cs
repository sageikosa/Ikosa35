using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    public class MovementStrip
    {
        #region construction
        public MovementStrip(ICoreObject coreObj, MovementBase movement, LocalMap map, Axis runAxis, AnchorFace baseFace,
            double height, int low, int high, int primary, int secondary,
            Dictionary<Guid, ICore> exclusions)
        {
            _CoreObj = coreObj;
            _Movement = movement;
            _Map = map;
            _Spaces = new List<(CellStructure Structure, CellLocation Location)>();
            _RunAxis = runAxis;
            _BaseFace = baseFace;
            _Height = height;
            _Exclusions = exclusions;

            for (var _s = low; _s <= high; _s++)
            {
                CellLocation _loc = null;
                switch (runAxis)
                {
                    case Axis.Z:
                        _loc = new CellLocation(_s, secondary, primary);
                        break;

                    case Axis.Y:
                        _loc = new CellLocation(primary, _s, secondary);
                        break;

                    default: // X
                        _loc = new CellLocation(secondary, primary, _s);
                        break;
                }
                _Spaces.Add((_Map[_loc], _loc));
            }
        }
        #endregion

        #region data
        private ICoreObject _CoreObj;
        private MovementBase _Movement;
        private LocalMap _Map;
        private List<(CellStructure Structure, CellLocation Location)> _Spaces;
        private Axis _RunAxis;
        private AnchorFace _BaseFace;
        private double _Height;
        private Dictionary<Guid, ICore> _Exclusions;
        #endregion

        public IEnumerable<ICellLocation> Locations
            => _Spaces.Select(_s => _s.Location);

        private bool IsFirstSpace(CellStructure space)
            => _Spaces.Any() && (space == _Spaces.First().Structure);

        private bool IsLastSpace(CellStructure space)
            => _Spaces.Any() && (space == _Spaces.Last().Structure);

        public bool BlocksFace(AnchorFace face)
            => _Spaces.All(_s => _s.Structure.HedralGripping(_Movement, face).GripCount() >= 32);

        #region public double UsableSpace(AnchorFace openFace, bool crossOpenHigh)
        /// <summary>Find the usable space available adjoining the desired face</summary>
        /// <param name="openFace">face needed to be open towards</param>
        /// <param name="planeCrossingAxis">test plane axis crossing</param>
        /// <param name="crossOpenHigh">true if the cross axis is allowed to open towards the high-plane</param>
        public double UsableSpace(AnchorFace openFace, Axis planeCrossingAxis, bool crossOpenHigh, bool crossOpenLow, PlanarPresence planar)
        {
            // NOTE: generally, crossOpenHigh should be set if the previous plane's opening didn't extend below this strip
            // NOTE: ...this indicates that this plane is allowed to open on an "earlier" strip index
            var _amount = new List<double>();
            double _usableHeight = 0;
            foreach (var _s in _Spaces)
            {
                if (_s.Structure.ValidSpace(_Movement))
                {
                    // valid space
                    _amount.Add(5d);
                    _usableHeight += 5d;
                }
                else
                {
                    // get openings
                    var _openings = _s.Structure.OpensTowards(_Movement, _BaseFace).ToList();
                    if (_openings.Any())
                        _usableHeight += AddOpenings(openFace, planeCrossingAxis, crossOpenHigh, crossOpenLow, _amount, _s, _openings);
                }
            }

            // if some amount of passage is possible, check object blockings
            if (_amount.Any() && _amount.Min() > 0)
            {
                // Tactically important objects
                foreach (var _s in _Spaces)
                {
                    foreach (var _ima in _Map.MapContext.AllInCell<IMoveAlterer>(_s.Location, planar)
                        .Where(_ima => !_Exclusions.ContainsKey(_ima.ID)))
                    {
                        if (_ima.HindersTransit(_Movement, _s.Location))
                        {
                            var _openings = _ima.OpensTowards(_Movement, _s.Location, _BaseFace, _CoreObj).ToList();
                            if (_openings.Any())
                                // ??? AddOpenings(openFace, planeCrossingAxis, crossOpenHigh, crossOpenLow, _amount, _s, _openings);
                                _usableHeight += AddOpenings(openFace, planeCrossingAxis, crossOpenHigh, crossOpenLow, _amount, _s, _openings);
                        }
                    }
                }
            }

            // return minimum opening allowance
            if (_usableHeight < _Height)
                return 0d;
            return _amount.Any() ? _amount.Min() : 0;
        }
        #endregion

        #region private double AddOpenings(AnchorFace openFace, Axis planeCrossingAxis, bool crossOpenHigh, bool crossOpenLow, List<double> _amount, (CellStructure Structure, CellLocation Location) _s, List<MovementOpening> _openings)
        private double AddOpenings(AnchorFace openFace, Axis planeCrossingAxis, bool crossOpenHigh, bool crossOpenLow,
            List<double> amount, (CellStructure Structure, CellLocation Location) strucLoc, List<MovementOpening> openings)
        {
            var _height = UsableHeight(strucLoc.Structure, openings);

            #region remove cross-open-high (if allowed)
            if (crossOpenHigh)
            {
                // remove cross-axis opening high
                var _coh = openings
                    .Where(_o => (_o.Face.GetAxis() == planeCrossingAxis) && !_o.Face.IsLowFace() && _o.Amount >= 2.5d).ToList();
                foreach (var _c in _coh)
                {
                    openings.Remove(_c);
                }
            }
            #endregion

            #region remove cross-open-low (if allowed)
            if (crossOpenLow)
            {
                // prune crossOpenLow if not allowing high
                var _col = openings
                    .Where(_o => (_o.Face.GetAxis() == planeCrossingAxis) && _o.Face.IsLowFace() && _o.Amount >= 2.5d).ToList();
                foreach (var _c in _col)
                {
                    openings.Remove(_c);
                }
            }
            #endregion

            // emptied the openings
            if (!openings.Any())
            {
                // nothing left that explicitly opens in only one direction
                amount.Add(5d);
            }
            else
            {

                if (openings.All(_o => _o.Face == openFace))
                {
                    // only openings in the direction we want should be left
                    amount.Add(openings.Min(_o => _o.Amount));
                }
                else
                {
                    // invalid (either none left after last prune, or what's left isn't what we are looking for)
                    amount.Add(0d);
                }
            }
            return _height;
        }
        #endregion

        #region private double UsableHeight(CellStructure space, List<MovementOpening> openings)
        private double UsableHeight(CellStructure space, List<MovementOpening> openings)
        {
            if (IsFirstSpace(space))
            {
                // remove opening information "up" from "bottom"
                var _axisHigh = openings
                    .Where(_o => (_o.Face.GetAxis() == _RunAxis) && !_o.Face.IsLowFace()).ToList();
                foreach (var _ah in _axisHigh)
                {
                    // remove and return amount allowed in cell
                    openings.Remove(_ah);
                }
                if (_axisHigh.Count > 0)
                {
                    return _axisHigh.Average(_ah => _ah.Amount);
                }
            }
            if (IsLastSpace(space))
            {
                // remove opening information "down" from "top"
                var _axisLow = openings
                    .Where(_o => (_o.Face.GetAxis() == _RunAxis) && _o.Face.IsLowFace()).ToList();
                foreach (var _al in _axisLow)
                {
                    // remove and return amount allowed in cell
                    openings.Remove(_al);
                }
                if (_axisLow.Count > 0)
                {
                    return _axisLow.Average(_ah => _ah.Amount);
                }
            }
            // nothing to remove, so will allow all
            return 5d;
        }
        #endregion
    }
}