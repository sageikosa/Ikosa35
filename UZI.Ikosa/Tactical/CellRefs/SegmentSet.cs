using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class SegmentSet
    {
        #region ctor()
        public SegmentSet(LocalMap map, Point3D startPoint, Point3D endPoint,
            IGeometricRegion sourceRegion, IGeometricRegion targetRegion,
            ITacticalInquiry[] exclusions, SegmentSetProcess setType, PlanarPresence locPlanes)
            : base()
        {
            _SegRefs = new List<SegmentRef>(20);
            _Map = map;
            _Start = startPoint;
            _End = endPoint;
            _Vector = (Vector3D)(EndPoint - StartPoint);
            _SourceRgn = sourceRegion;
            _TargetRgn = targetRegion;
            _Exclusions = exclusions;
            _Blocked = -1;
            _Unblocked = -1;
            _SetType = setType;
            _LocPlanes = locPlanes;

            _Concealers = [];
            _TotalConcealers = [];

            _DetectBlocked = -1;
        }
        #endregion

        #region state
        private List<SegmentRef> _SegRefs;
        private Point3D _Start;
        private Point3D _End;
        private Vector3D _Vector;
        private LocalMap _Map;
        private int _Unblocked;
        private int _Blocked;
        private ITacticalInquiry _BlockingObject;
        private IGeometricRegion _SourceRgn;
        private IGeometricRegion _TargetRgn;
        private IEnumerable<ITacticalInquiry> _Exclusions;
        private SegmentSetProcess _SetType;
        private PlanarPresence _LocPlanes;

        private List<ITacticalInquiry> _Concealers = null;
        private List<ITacticalInquiry> _TotalConcealers = null;

        private int _DetectBlocked;
        private ITacticalInquiry _NonDetectObject = null;
        #endregion

        public Point3D StartPoint => _Start;
        public Point3D EndPoint => _End;
        public Vector3D Vector => _Vector;
        public IGeometricRegion SourceRegion => _SourceRgn;
        public IGeometricRegion TargetRegion => _TargetRgn;
        public PlanarPresence PlanarPresence => _LocPlanes;

        #region public CoverLevel SuppliesCover()
        /// <summary>determine if the line (or objects along the line) supplies cover for the object specified</summary>
        public CoverLevel SuppliesCover()
        {
            if (!IsLineOfEffect)
            {
                return CoverLevel.Hard;
            }

            var _maxLevel = CoverLevel.None;

            // look along path...
            foreach (var _cRef in All())
            {
                var _tInfo = TacticalCell(in _cRef);
                // ... for locatable objects ...
                foreach (var _tactical in _Map.MapContext.AllInCell<ITacticalInquiry>(_cRef, _LocPlanes))
                {
                    // ... supplying cover
                    if ((_tactical != null) && !_Exclusions.Contains(_tactical))
                    {
                        switch (_tactical.SuppliesCover(in _tInfo))
                        {
                            case CoverLevel.Improved:
                                return CoverLevel.Improved;
                            case CoverLevel.Hard:
                                if (_maxLevel < CoverLevel.Hard)
                                {
                                    _maxLevel = CoverLevel.Hard;
                                }

                                break;
                            case CoverLevel.Soft:
                                if (_maxLevel < CoverLevel.Soft)
                                {
                                    _maxLevel = CoverLevel.Soft;
                                }

                                break;
                        }
                    }
                }
            }

            return _maxLevel;
        }
        #endregion

        #region public CoverConcealmentResult SuppliesConcealment()
        /// <summary>determine if objects along the line supply total or partial concealment</summary>
        public CoverConcealmentResult SuppliesConcealment()
        {
            if (!IsLineOfEffect)
            {
                return CoverConcealmentResult.None;
            }

            if (_TotalConcealers.Except(_Exclusions).Any())
            {
                return CoverConcealmentResult.Total;
            }
            if (_Concealers.Except(_Exclusions).Any())
            {
                return CoverConcealmentResult.Partial;
            }
            return CoverConcealmentResult.None;
        }
        #endregion

        /// <summary>True if there are no cells blocking detection</summary>
        public bool IsLineOfDetect => _DetectBlocked < 0;

        /// <summary>First Cell blocking detection along the path (or null if none)</summary>
        public SegmentRef DetectBlockingCell
            => _DetectBlocked > 0
            ? _SegRefs[_DetectBlocked]
            : default;

        /// <summary>Object that blocked line of effect (or null if blocked by environment)</summary>
        public ITacticalInquiry DetectBlockingObject => _NonDetectObject;

        /// <summary>Map for which this LineSet contains LinearCellRefs</summary>
        public LocalMap Map => _Map;

        /// <summary>Last Cell unblocked along the path (or null if none)</summary>
        public SegmentRef UnblockedCell
            => _Unblocked > 0
            ? _SegRefs[_Unblocked]
            : default;

        /// <summary>First Cell blocked along the path (or null if none)</summary>
        public SegmentRef BlockedCell
            => _Blocked > 0
            ? _SegRefs[_Blocked]
            : default;

        /// <summary>Object that blocked line of effect (or null if blocked by environment)</summary>
        public ITacticalInquiry BlockedObject => _BlockingObject;

        public IEnumerable<SegmentRef> All()
            => _SegRefs.Select(_sr => _sr);

        public IEnumerable<SegmentRef> AllIntermediate()
            => _SegRefs.Skip(1).Take(_SegRefs.Count - 2);

        protected TacticalInfo TacticalCell(in SegmentRef segRef)
            => new TacticalInfo
            (
                in segRef,
                StartPoint,
                EndPoint,
                SourceRegion,
                TargetRegion
            );

        #region public void Add(CRef item)
        /// <summary>Override to do things after adding them</summary>
        public void Add(in SegmentRef item)
        {
            //if (!_SegRefs.Contains(item))
            _SegRefs.Add(item);
            OnAdd(item, _SegRefs.Count - 1);
        }
        #endregion

        public void Block()
        {
            _Blocked = _SegRefs.Count - 1;
        }

        #region protected void OnAdd(LinearCellRef cellRef)
        protected void OnAdd(in SegmentRef cellRef, int index)
        {
            var _tacticalQuery = _Map.MapContext.AllInCell<ITacticalInquiry>(cellRef, _LocPlanes)
                .Except(_Exclusions);
            List<ITacticalInquiry> _tacticals = null;

            #region Line of Effect
            if (_Blocked < 0)
            {
                // crossing the cell, cell could block, and looking for material blockage
                if (!cellRef.IsPoint
                    && cellRef.BlocksPath
                    && _LocPlanes.HasMaterialPresence())
                {
                    // NOTE: no single cell with zero distance can block on its own merits
                    _Blocked = index;
                }
                else
                {
                    // look for objects and geom-effects that block the path
                    _tacticals = _tacticalQuery.ToList();
                    if (_tacticals.Count > 0)
                    {
                        var _tInfo = TacticalCell(in cellRef);
                        foreach (var _blocker in _tacticals)
                        {
                            if (_blocker.BlocksLineOfEffect(in _tInfo))
                            {
                                _Blocked = index;
                                _BlockingObject = _blocker;
                                break;
                            }
                        }
                    }
                    if (_Blocked < 0)
                    {
                        // TODO: geom-effects?
                        _Unblocked = index;
                    }
                }
            }
            #endregion

            switch (_SetType)
            {
                case SegmentSetProcess.Observation:
                    #region Observation Line
                    // if the line ceases to be an effect line, concealment is irrelevant
                    if (!IsLineOfEffect)
                    {
                        return;
                    }

                    // look for objects and geom-effects that block the path
                    _tacticals ??= _tacticalQuery.ToList();
                    if (_tacticals.Count > 0)
                    {
                        var _tInfo = TacticalCell(in cellRef);
                        foreach (var _obj in _tacticals)
                        {
                            if (_obj.SuppliesConcealment(in _tInfo)
                                && !_Concealers.Contains(_obj))
                            {
                                _Concealers.Add(_obj);
                            }
                        }
                        foreach (var _obj in _tacticals)
                        {
                            if (_obj.SuppliesTotalConcealment(in _tInfo)
                                && !_TotalConcealers.Contains(_obj))
                            {
                                _TotalConcealers.Add(_obj);
                            }
                        }
                    }
                    #endregion
                    break;

                case SegmentSetProcess.Detect:
                    #region Line of Detect (Divination)
                    if (_DetectBlocked < 0)
                    {
                        if (cellRef.BlocksDetect)
                        {
                            _DetectBlocked = index;
                        }
                        else
                        {
                            // look for objects and geom-effects that block the path
                            _tacticals ??= _tacticalQuery.ToList();
                            if (_tacticals.Count > 0)
                            {
                                var _tInfo = TacticalCell(in cellRef);
                                foreach (var _t in _tacticals)
                                {
                                    if (_t.BlocksLineOfDetect(in _tInfo))
                                    {
                                        _DetectBlocked = index;
                                        _NonDetectObject = _t;
                                        break;
                                    }
                                }
                            }
                            if (_DetectBlocked < 0)
                            {
                                // TODO: geom-effects?
                            }
                        }
                    }
                    #endregion
                    break;

                case SegmentSetProcess.Effect:
                default:
                    break;
            }
        }
        #endregion

        public int Count
            => _SegRefs.Count;

        /// <summary>If there are no blocked cells in the line, it is a line of effect</summary>
        public bool IsLineOfEffect
            => _Blocked < 0;

        public Point3D TerminationPoint
            => _SegRefs[_SegRefs.Count - 1].ExitPoint;

        #region public bool CarryInteraction(Interaction workSet)
        /// <summary>
        /// True if the interaction would not be destroyed by alterers.  
        /// NOTE: this does not consider IsLineOfEffect, so is safe for sound.
        /// </summary>
        /// <param name="workSet">interaction to carry</param>
        public bool CarryInteraction(Interaction workSet, List<InteractCapture> zones = null)
        {
            var _zones = zones ?? Map.MapContext.GetInteractionTransitZones(workSet).ToList();
            if (_zones.Any())
            {
                // move interaction through each cell
                foreach (var _cRef in All())
                {
                    // look for alterers
                    if ((from _z in _zones
                         where _z.Geometry.Region.ContainsCell(_cRef)
                         && _z.WillDestroyInteraction(workSet, _cRef)
                         select _z).Any())
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion
    }
}
