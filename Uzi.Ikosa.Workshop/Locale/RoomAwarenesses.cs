using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop.Locale
{
    public class RoomAwarenesses
    {
        public RoomAwarenesses()
        {
            _Render = new ConcurrentDictionary<Guid, Guid>();
            _Sweep = new ConcurrentDictionary<Guid, Guid>();
        }

        #region private data
        private ConcurrentDictionary<Guid, Guid> _Render;
        private ConcurrentDictionary<Guid, Guid> _Sweep;
        #endregion

        #region private bool CheckSenses(SegmentSet obsSet, List<SensoryBase> senses, CoreObject target)
        private bool CheckSenses(SegmentSet obsSet, List<SensoryBase> senses)
        {
            return (from _sense in senses
                    let _sTrans = new SenseTransit(_sense)
                    let _senseSet = new Interaction(_sense.Creature, _sense, null, _sTrans)
                    where ((_sense.IgnoresConcealment
                    ? CoverConcealmentResult.None
                    : obsSet.SuppliesConcealment()) < CoverConcealmentResult.Total)
                    && (!_sense.UsesSenseTransit || obsSet.CarryInteraction(_senseSet))
                    select _sense).Any();
        }
        #endregion

        #region private bool ProcessAllLinesOfEffect(...)
        /// <summary>
        /// default behavior is to get all observation lines (line of effect) between regions,
        /// preventing the target object from providing cover to itself, then checking each sense
        /// </summary>
        private bool ProcessAllLinesOfEffect(IGeometricContext observerContext, List<Point3D> pts,
            CellList cellList, List<SensoryBase> senses)
        {
            // get all lines of effect
            var _factory = new SegmentSetFactory(observerContext.MapContext.Map, observerContext.GeometricRegion,
                cellList, ITacticalInquiryHelper.EmptyArray, SegmentSetProcess.Observation);
            return observerContext.LinesToCells(pts, _factory, 120, PlanarPresence.Both)
                .Where(_l => _l.IsLineOfEffect)
                .Any(_ols => CheckSenses(_ols, senses));
        }
        #endregion

        #region private bool IsAwareOfCells(IGeometricContext geomContext, IList<SensoryBase> bestEffectSenses, List<Point3D> pts, CellList cellList)
        private bool IsAwareOfCells(IGeometricContext geomContext, IList<SensoryBase> bestEffectSenses, List<Point3D> pts, CellList cellList)
        {
            var _distance = cellList.AllCellLocations().Min(_c => geomContext.GeometricRegion.NearDistanceToCell(_c));

            // have at least one sense that uses line of effect in range 
            var _effectBest = bestEffectSenses.Where(_b => _b.Range >= _distance).ToList();
            if (_effectBest.Any())
            {
                if (ProcessAllLinesOfEffect(geomContext, pts, cellList, _effectBest))
                    return true;
            }
            return false;
        }
        #endregion

        #region private List<LocalCellGroup> GetNextGroups(IGeometricContext geomContext, ISensorHost sensors, List<SensoryBase> effectBest, List<LocalCellGroup> newGroups)
        private List<LocalCellGroup> GetNextGroups(IGeometricContext geomContext, List<SensoryBase> effectBest, List<LocalCellGroup> newGroups)
        {
            // mark processing
            MergeIn(_Render, newGroups);

            var _next = new List<LocalCellGroup>();
            foreach (var _grp in newGroups)
            {
                // get all links to track
                var _links = _grp.Links.All;
                var _grouped = from _l in _links
                               let _out = _l.OutsideGroup(_grp)
                               where !_Sweep.ContainsKey(_out.ID)
                               group new
                               {
                                   Out = _l.OutsideInteractionCell(_grp),
                                   In = _l.InteractionCell(_grp),
                                   Pts = _l.InteractionCell(_grp).FaceCorners(_l.GetAnchorFaceInGroup(_grp)).ToList()
                               }
                               by _out;

                // render all groups
                var _rend = _grouped.AsParallel()
                    .Where(_g => IsAwareOfCells(geomContext, effectBest,
                    _g.SelectMany(_oi => _oi.Pts).Union(_g.Select(_oi => _oi.Out.GetPoint3D())).Distinct().ToList(),
                    new CellList(_g.Select(_oi => _oi.Out).OfType<ICellLocation>(), 0, 0, 0)))
                    .ToList();
                MergeIn(_Render, _rend.Select(_g => _g.Key));

                // figure out next level out
                _next.AddRange(_rend.AsParallel().SelectMany(_g =>
                {
                    if (IsAwareOfCells(geomContext, effectBest,
                        _g.SelectMany(_oi => _oi.Pts).Union(_g.Select(_oi => _oi.Out.GetPoint3D())).Distinct().ToList(),
                        new CellList(_g.Select(_oi => _oi.In).OfType<ICellLocation>(), 0, 0, 0)))
                        return _g.Key.ToEnumerable();
                    return new LocalCellGroup[] { };

                }).Where(_g => _g != null));
            }
            MergeIn(_Sweep, newGroups);
            return _next;
        }
        #endregion

        private void MergeIn(ConcurrentDictionary<Guid, Guid> dest, IEnumerable<LocalCellGroup> source)
        {
            foreach (var _s in source)
                dest.TryAdd(_s.ID, _s.ID);
        }

        private List<LocalCellGroup> IncludeBackgrounds(List<LocalCellGroup> groups)
            => groups.Union(from _g in groups
                            where _g.IsPartOfBackground
                            from _s in _g.Map.GetSharedGroups(_g)
                            where !_Sweep.ContainsKey(_g.ID)
                            select _s).ToList();

        public void RecalculateAwareness(IGeometricContext geomContext, List<SensoryBase> sensors)
        {
            // start with any room currently in, and any shared groups
            var _next = IncludeBackgrounds(geomContext.GetLocalCellGroups().ToList());
            var _sx = 1;
            while (_next.Any() && (_sx > 0))
            {
                // render and mark next level "out" groups
                MergeIn(_Render, _next);
                MergeIn(_Sweep, _next);

                // and next level out automatically
                _next = IncludeBackgrounds(_next
                    .SelectMany(_g => _g.Links.LinkedGroups.Where(_lg => !_Sweep.ContainsKey(_lg.ID)))
                    .Distinct().ToList());

                // decrement auto-count
                _sx--;
            }

            while (_next.Any())
            {
                _next = IncludeBackgrounds(GetNextGroups(geomContext, sensors, _next));
            }

            // merge
            //Debug.WriteLine($@"Rooms: {_Render.Count} | {_Sweep.Count}");
        }

        public IEnumerable<Guid> GetRooms()
            => _Render.Select(_r => _r.Key);

        public bool this[Guid guid]
            => _Render.ContainsKey(guid);
    }

    public class CellLocContext : IGeometricContext
    {
        public CellLocContext(MapContext context, CellLocation cellLoc, LocalCellGroup cellGroup)
        {
            _Context = context;
            _CellLoc = cellLoc;
            _Group = cellGroup;
        }

        private readonly MapContext _Context;
        private readonly CellLocation _CellLoc;
        private readonly LocalCellGroup _Group;

        public MapContext MapContext => _Context;
        public IGeometricRegion GeometricRegion => _CellLoc;
        public PlanarPresence PlanarPresence => PlanarPresence.Material;

        public IEnumerable<LocalCellGroup> GetLocalCellGroups()
            => _Group.ToEnumerable();
    }
}
