using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Movement;
using Uzi.Visualize;
using Uzi.Core;
using System.Diagnostics;

namespace Uzi.Ikosa.Tactical
{
    public class MovementVolume
    {
        public MovementVolume(ICoreObject coreObject, MovementBase movement, AnchorFace[] crossings, AnchorFace baseFace,
            LocalMap map, PlanarPresence planar, Dictionary<Guid, ICore> exclusions)
        {
            _CoreObject = coreObject;
            _Movement = movement;
            _Crossings = crossings;
            _BaseFace = baseFace;
            _Map = map;
            _Exclusions = exclusions;
            _Planar = planar;
        }

        #region  data
        private ICoreObject _CoreObject;
        private MovementBase _Movement;
        private AnchorFace[] _Crossings;
        private AnchorFace _BaseFace;
        private LocalMap _Map;
        //private List<MovementSlice> _Slices = new List<MovementSlice>();
        private PlanarPresence _Planar;
        private Dictionary<Guid, ICore> _Exclusions;
        #endregion

        public MovementBase Movement => _Movement;
        public AnchorFace[] Crossings => _Crossings;
        public LocalMap Map => _Map;

        #region private CellList SqueezeCells(Cubic test, Axis planeStepAxis, Axis sliceStepAxis, Axis stripStepAxis, double length, double height, double narrowest)
        private CellList SqueezeCells(Cubic test, Axis planeStepAxis, Axis sliceStepAxis, Axis stripStepAxis,
            double length, double height, double narrowest, bool noOffset)
        {
            // TODO: dynamic strip extension should happen here?
            #region setup loop and other parameters
            var _planeStart = (planeStepAxis == Axis.Z) ? test.Z : (planeStepAxis == Axis.Y) ? test.Y : test.X;
            var _planeStop = (planeStepAxis == Axis.Z) ? test.UpperZ : (planeStepAxis == Axis.Y) ? test.UpperY : test.UpperX;
            var _sliceStart = (sliceStepAxis == Axis.Z) ? test.Z : (sliceStepAxis == Axis.Y) ? test.Y : test.X;
            var _sliceStop = (sliceStepAxis == Axis.Z) ? test.UpperZ : (sliceStepAxis == Axis.Y) ? test.UpperY : test.UpperX;
            var _cellStart = (stripStepAxis == Axis.Z) ? test.Z : (stripStepAxis == Axis.Y) ? test.Y : test.X;
            var _cellStop = (stripStepAxis == Axis.Z) ? test.UpperZ : (stripStepAxis == Axis.Y) ? test.UpperY : test.UpperX;
            var _sliceHigh = (sliceStepAxis == Axis.Z) ? AnchorFace.ZHigh : (sliceStepAxis == Axis.Y) ? AnchorFace.YHigh : AnchorFace.XHigh;
            var _sliceLow = _sliceHigh.ReverseFace();
            var _twist = ((stripStepAxis == Axis.Z) && (planeStepAxis == Axis.X))
                || ((stripStepAxis == Axis.Y) && (planeStepAxis == Axis.Z))
                || ((stripStepAxis == Axis.X) && (planeStepAxis == Axis.Y));
            #endregion
            var _rankingSliceOffset = 0;

            // all plane crossings
            var _slices = new List<MovementSlice>();
            var _alwaysOpenToLow = _planeStart == _planeStop;
            for (var _planeIndex = _planeStart; _planeIndex <= _planeStop; _planeIndex++)
            {
                var _done = false;

                // looking for a gap
                var _current = new MovementSlice
                {
                    Coordinate = _planeIndex * 5 + 2.5,
                    SliceOffset = _rankingSliceOffset
                };
                while (!_done)
                {
                    var _started = false;
                    var _ended = false;

                    // each plane has columns in squeezing direction
                    for (var _stripIndex = _sliceStart; _stripIndex <= _sliceStop; _stripIndex++)
                    {
                        var _strip = new MovementStrip(_CoreObject, Movement, Map, stripStepAxis, _BaseFace, height,
                            _cellStart + _current.SliceOffset,
                            _cellStop + _current.SliceOffset,
                            (_twist ? _planeIndex : _stripIndex),
                            (_twist ? _stripIndex : _planeIndex),
                            _Exclusions
                            );
                        var _crossOpenHigh = ((_slices.Count == 0)
                            || (_slices.Last().StartIndex > _stripIndex)
                            || (_slices.Last().EndIndex < _stripIndex));
                        if (!_started)
                        {
                            #region start?
                            // looking for start
                            var _start = _strip.UsableSpace(_sliceHigh, planeStepAxis, _crossOpenHigh, 
                                !_crossOpenHigh || _alwaysOpenToLow, _Planar);
                            if (_start > 0)
                            {
                                _started = true;
                                _current.StartIndex = _stripIndex;
                                _current.Start = ((_stripIndex + 1) * 5) - _start;
                                _current.Strips.Add(_strip);
                            }
                            else if (!_strip.BlocksFace(_sliceHigh))
                            {
                                // include the strip, but don't count extent
                                _current.Strips.Add(_strip);
                            }
                            #endregion
                        }
                        else
                        {
                            #region end?
                            // looking for end
                            var _end = _strip.UsableSpace(_sliceLow, planeStepAxis, _crossOpenHigh, 
                                !_crossOpenHigh || _alwaysOpenToLow, _Planar);
                            if (_end < 5)
                            {
                                _ended = true;
                                if (_end > 0)
                                {
                                    _current.Strips.Add(_strip);
                                    _current.EndIndex = _stripIndex;
                                }
                                else
                                {
                                    _current.EndIndex = _stripIndex - 1;
                                    if (!_strip.BlocksFace(_sliceLow))
                                    {
                                        _current.Strips.Add(_strip);
                                    }
                                }

                                _current.End = (_stripIndex * 5) + _end;

                                if (_current.Extent < narrowest)
                                {
                                    // not enough room to pass through (restart)
                                    _current = new MovementSlice
                                    {
                                        Coordinate = _planeIndex * 5 + 2.5,
                                        SliceOffset = _current.SliceOffset
                                    };
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                _current.Strips.Add(_strip);
                            }
                            #endregion
                        }
                    }

                    #region end!
                    // ensure a usable end
                    if (_started && !_ended)
                    {
                        _current.EndIndex = _sliceStop + 1;
                        _current.End = _current.EndIndex * 5;
                    }
                    #endregion

                    // check current plane fitness
                    if (_started && (_current.Extent >= narrowest)
                        && ((_slices.Count == 0) // JDO:2015-05-25 fix since _slices.Add() only happens within
                        || _slices.All(_s =>
                            _current.DistanceFromEndToSliceStart(_s) >= narrowest
                            && _current.DistanceFromStartToSliceEnd(_s) >= narrowest)))
                    {
                        // good slice
                        _slices.Add(_current);
                        if (Math.Abs(_current.SliceOffset) > Math.Abs(_rankingSliceOffset))
                        {
                            _rankingSliceOffset = _current.SliceOffset;
                        }

                        _done = true;
                    }
                    else
                    {
                        // must attempt an offset to squeeze

                        // allowed to make an offset?
                        if (noOffset)
                        {
                            return null;
                        }

                        if (_rankingSliceOffset == 0)
                        {
                            #region initial attempts to offset slices
                            // haven't seen any slice offsets
                            if (_current.SliceOffset == 0)
                            {
                                // haven't tried any either, so try positive
                                _current = new MovementSlice
                                {
                                    Coordinate = _planeIndex * 5 + 2.5,
                                    SliceOffset = 1
                                };
                            }
                            else if (_current.SliceOffset == 1)
                            {
                                // then...try negative (if first pass failed)
                                _current = new MovementSlice
                                {
                                    Coordinate = _planeIndex * 5 + 2.5,
                                    SliceOffset = -1
                                };
                            }
                            else
                            {
                                // couldn't step the slice and get anything either
                                return null;
                            }
                            #endregion
                        }
                        else if (_rankingSliceOffset > 0)
                        {
                            #region already stepping "up"
                            if (_current.SliceOffset == _rankingSliceOffset)
                            {
                                _current = new MovementSlice
                                {
                                    Coordinate = _planeIndex * 5 + 2.5,
                                    SliceOffset = _current.SliceOffset + 1
                                };
                            }
                            else
                            {
                                // couldn't step the slice and get anything either
                                return null;
                            }
                            #endregion
                        }
                        else
                        {
                            #region already stepping "down"
                            if (_current.SliceOffset == _rankingSliceOffset)
                            {
                                _current = new MovementSlice
                                {
                                    Coordinate = _planeIndex * 5 + 2.5,
                                    SliceOffset = _current.SliceOffset - 1
                                };
                            }
                            else
                            {
                                // couldn't step the slice and get anything either
                                return null;
                            }
                            #endregion
                        }
                    }
                } // while (!_done)
            }

            #region assign dimensional limits to axial parameters
            var _zNarrow = 0d;
            var _yNarrow = 0d;
            var _xNarrow = 0d;
            switch (planeStepAxis)
            {
                case Axis.Z:
                    _zNarrow = length;
                    break;
                case Axis.Y:
                    _yNarrow = length;
                    break;
                default:
                    _xNarrow = length;
                    break;
            }
            switch (sliceStepAxis)
            {
                case Axis.Z:
                    _zNarrow = _slices.Min(_s => _s.Extent);
                    break;
                case Axis.Y:
                    _yNarrow = _slices.Min(_s => _s.Extent);
                    break;
                default:
                    _xNarrow = _slices.Min(_s => _s.Extent);
                    break;
            }
            switch (stripStepAxis)
            {
                case Axis.Z:
                    _zNarrow = height;
                    break;
                case Axis.Y:
                    _yNarrow = height;
                    break;
                default:
                    _xNarrow = height;
                    break;
            }
            #endregion

            // got through all planes without failing
            return new CellList(from _sl in _slices
                                from _st in _sl.Strips
                                from _l in _st.Locations
                                select _l, _zNarrow, _yNarrow, _xNarrow);
        }
        #endregion

        #region public CellList SqueezeCells(Cubic test)
        public CellList SqueezeCells(Cubic test, double zFit, double yFit, double xFit, double squeezeFactor, params Axis[] excludedOffsetAxis)
        {
            // TODO: parameter for surface hugging climbing?

            // use excludedOffsetAxis to determine whether the offset should not be applied
            bool _noOffset(Axis axis) => (excludedOffsetAxis != null) && excludedOffsetAxis.Contains(axis);

            CellList _minDisplacementList(Axis planeStep, Axis cut1, Axis cut2, double fit1, double fit2, double fit3)
            {
                // slice one way, then the other
                var _list = SqueezeCells(test, planeStep, cut1, cut2, fit1, fit2, fit3 * squeezeFactor, _noOffset(cut2));
                if (_list != null)
                {
                    if ((_list.GetPoint3D() - test.GetPoint3D()).LengthSquared == 0)
                    {
                        // no displacement...
                        return _list;
                    }
                }
                var _list2 = SqueezeCells(test, planeStep, cut2, cut1, fit1, fit3, fit2 * squeezeFactor, _noOffset(cut1));
                if (_list2 != null)
                {
                    var _length2 = (_list2.GetPoint3D() - test.GetPoint3D()).LengthSquared;
                    if (_length2 == 0)
                    {
                        // second list no displacement
                        return _list2;
                    }
                    if (_list != null)
                    {
                        // must compare
                        if ((_list.GetPoint3D() - test.GetPoint3D()).LengthSquared < _length2)
                        {
                            return _list;
                        }

                        return _list2;
                    }
                    else
                    {
                        // only had a second list
                        return _list2;
                    }
                }
                else if (_list != null)
                {
                    // only had a first list
                    return _list;
                }
                return null;
            }

            if (_Crossings.Any(_face => _face == AnchorFace.ZHigh || _face == AnchorFace.ZLow))
            {
                return _minDisplacementList(Axis.Z, Axis.Y, Axis.X, zFit, xFit, yFit);
            }

            if (_Crossings.Any(_face => _face == AnchorFace.YHigh || _face == AnchorFace.YLow))
            {
                return _minDisplacementList(Axis.Y, Axis.Z, Axis.X, yFit, xFit, zFit);
            }

            if (_Crossings.Any(_face => _face == AnchorFace.XHigh || _face == AnchorFace.XLow))
            {
                return _minDisplacementList(Axis.X, Axis.Y, Axis.Z, xFit, zFit, yFit);
            }
            return null;
        }
        #endregion
    }
}