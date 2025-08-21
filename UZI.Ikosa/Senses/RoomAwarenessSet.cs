using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class RoomAwarenessSet : ICreatureBound
    {
        public RoomAwarenessSet(Creature critter)
        {
            _Creature = critter;
            _Render = new ConcurrentDictionary<Guid, Guid>();
            _Sweep = new ConcurrentDictionary<Guid, Guid>();
        }

        #region private data
        [field:NonSerialized]
        private ConcurrentDictionary<Guid, Guid> _Render;
        [field:NonSerialized]
        private ConcurrentDictionary<Guid, Guid> _Sweep;
        private Creature _Creature;
        #endregion

        #region private bool CheckSenses(SegmentSet obsSet, List<SensoryBase> senses, CoreObject target)
        private bool CheckSenses(SegmentSet obsSet, List<SensoryBase> senses)
        {
            return (from _sense in senses
                    let _sTrans = new SenseTransit(_sense)
                    let _senseSet = new Interaction(_Creature, _sense, null, _sTrans)
                    where ((_sense.IgnoresConcealment
                    ? CoverConcealmentResult.None
                    : obsSet.SuppliesConcealment()) < CoverConcealmentResult.Total)
                    && (!_sense.UsesSenseTransit || obsSet.CarryInteraction(_senseSet))
                    select _sense).Any();
        }
        #endregion

        #region private bool AnyLineOfEffect(...)
        /// <summary>
        /// default behavior is to get all observation lines (line of effect) between regions,
        /// preventing the target object from providing cover to itself, then checking each sense
        /// </summary>
        private bool AnyLineOfEffect(IGeometricContext sensorContext, List<Point3D> pts, CellList outerCells, List<SensoryBase> senses,
            PlanarPresence sensePresence)
        {
            // get all lines of effect
            return sensorContext.LinesToCells(pts,
                new SegmentSetFactory(sensorContext.MapContext.Map, sensorContext.GeometricRegion, outerCells,
                    ITacticalInquiryHelper.EmptyArray, SegmentSetProcess.Observation),
                Creature.Skills.Skill<SpotSkill>().MaxMultiDistance, sensePresence)
                .Where(_l => _l.IsLineOfEffect)
                .Any(_ols => CheckSenses(_ols, senses));
        }
        #endregion

        #region private bool IsAwareOfCells(IGeometricContext sensorContext, IList<SensoryBase> bestEffectSenses, List<Point3D> pts, CellList outerCells)
        private bool IsAwareOfCells(IGeometricContext sensorContext, IList<SensoryBase> bestEffectSenses,
            PlanarPresence sensePresence, List<Point3D> pts, CellList outerCells)
        {
            var _distance = outerCells.AllCellLocations().Min(_c => sensorContext.GeometricRegion.NearDistanceToCell(_c));

            // have at least one sense that uses line of effect in range 
            var _effectBest = bestEffectSenses.Where(_b => _b.Range >= _distance).ToList();
            if (_effectBest.Any())
            {
                if (AnyLineOfEffect(sensorContext, pts, outerCells, _effectBest, sensePresence))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region private List<LocalCellGroup> GetNextGroups(IGeometricContext sensorContext, ISensorHost sensors, List<SensoryBase> effectBest, List<LocalCellGroup> newGroups)
        private List<LocalCellGroup> GetNextGroups(IGeometricContext sensorContext, List<SensoryBase> effectBest,
            List<LocalCellGroup> edgeGroups, ConcurrentDictionary<Guid, Guid> render, ConcurrentDictionary<Guid, Guid> sweep,
            PlanarPresence sensePresence)
        {
            var _next = new List<LocalCellGroup>();
            var _sweep = new List<LocalCellGroup>();
            foreach (var _grp in edgeGroups)
            {
                // wave is all outside groups
                var _wave = (from _l in _grp.Links.All
                             let _out = _l.OutsideGroup(_grp)
                             where !render.ContainsKey(_out.ID)
                             group new
                             {
                                 Out = _l.OutsideInteractionCell(_grp),
                                 Pts = _l.InteractionCell(_grp).FaceCorners(_l.GetAnchorFaceInGroup(_grp)).ToList()
                             }
                             by _out).ToList();

                // renderable if aware of outside cell
                var _render = _wave.AsParallel()
                    .Where(_g => IsAwareOfCells(sensorContext, effectBest, sensePresence,
                    _g.SelectMany(_oi => _oi.Pts).Union(_g.Select(_oi => _oi.Out.GetPoint3D())).Distinct().ToList(),
                    new CellList(_g.Select(_oi => _oi.Out).OfType<ICellLocation>(), 0, 0, 0)))
                    .Select(_g => _g.Key).ToList();

                // track renderable groups
                MergeIn(render, _render);

                // anything we've seen is in the next set to test
                _next.AddRange(_render);

                // everything tested must be added (regardless if whether it goes to the next level)
                _sweep.AddRange(_wave.Select(_w => _w.Key));
            }

            // everything we tested is in set to send info
            MergeIn(sweep, _sweep);
            MergeIn(sweep, edgeGroups);
            return _next;
        }
        #endregion

        private void MergeIn(ConcurrentDictionary<Guid, Guid> dest, IEnumerable<LocalCellGroup> source)
        {
            foreach (var _s in source)
            {
                dest.TryAdd(_s.ID, _s.ID);
            }
        }

        private List<LocalCellGroup> IncludeBackgrounds(ConcurrentDictionary<Guid, Guid> render, List<LocalCellGroup> groups, LocalMap map)
            => groups.Union(from _g in groups
                            where _g.IsPartOfBackground
                            from _s in map.GetSharedGroups(_g)
                            where !render.ContainsKey(_g.ID)
                            select _s).ToList();

        /// <summary>Used when leaving setting</summary>
        public void ClearAwarenesses(ISensorHost sensors, LocalMap map)
        {
            var _original = GetSweptRooms();
            var _locator = sensors?.GetLocated()?.Locator;

            foreach (var _key in _original.Keys)
            {
                map.GetLocalCellGroup(_key)?.RemoveSensorsNotifier(sensors);
            }
        }

        #region public void RecalculateAwareness(ISensorHost sensors)
        /// <summary>Recalculate awareness and maintain local cell group notifiers</summary>
        public void RecalculateAwareness(ISensorHost sensors)
        {
            // capture original swept rooms
            var _original = GetSweptRooms();

            // gather new
            var _render = new ConcurrentDictionary<Guid, Guid>();
            var _sweep = new ConcurrentDictionary<Guid, Guid>();

            // working set
            var _locator = sensors?.GetLocated().Locator;
            var _planer = _locator.PlanarPresence;
            var _map = _locator?.Map;
            var _effectBest = sensors.Senses.BestTerrainSenses
                .Where(_s => ((_s.PlanarPresence & _planer) != PlanarPresence.None)
                && _s.UsesLineOfEffect).ToList();

            // start with any room currently in, and any shared groups
            var _next = IncludeBackgrounds(_render, _locator.GetLocalCellGroups().ToList(), _map);
            if (_next.Any())
            {
                MergeIn(_render, _next);
                MergeIn(_sweep, _next);
            }

            // loop while gathering
            var _sensePresence = _effectBest.Any() ? _effectBest.Max(_s => _s.PlanarPresence) : PlanarPresence.None;
            while (_next.Any())
            {
                // gather next
                _next = IncludeBackgrounds(_render, GetNextGroups(_locator, _effectBest, _next, _render, _sweep, _sensePresence), _map);
            }

            // commit
            lock (this)
            {
                _Render = _render;
                _Sweep = _sweep;
            }

            // merge original and new
            var _new = GetSweptRooms();
            foreach (var _add in _new.Keys)
            {
                // try-add notifiers
                _map.GetLocalCellGroup(_add)?.AddSensorsNotifier(sensors);
            }
            foreach (var _drop in _original.Keys.Where(_i => !_new.ContainsKey(_i)).ToList())
            {
                // drop notifiers
                _map.GetLocalCellGroup(_drop)?.RemoveSensorsNotifier(sensors);
            }
        }
        #endregion

        // ICreatureBound Members
        public Creature Creature
            => _Creature;

        public bool IsSweptRoom(Guid id)
            => _Sweep.ContainsKey(id);

        public IDictionary<Guid, Guid> GetSweptRooms()
        {
            var _sweep = _Sweep ?? new ConcurrentDictionary<Guid, Guid>();
            return _sweep.ToDictionary(_s => _s.Key, _s => _s.Value);
        }

        public IDictionary<Guid, Guid> GetRenderedRooms()
        {
            var _render = _Render ?? new ConcurrentDictionary<Guid, Guid>();
            return _render.ToDictionary(_r => _r.Key, _s => _s.Value);
        }
    }
}
