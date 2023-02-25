using System;
using System.Collections.Generic;
using Uzi.Ikosa.Tactical;
using System.Linq;
using System.Diagnostics;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Actions;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class AwarenessSet : ICreatureBound, IEnumerable<KeyValuePair<Guid, AwarenessLevel>>, IAwarenessLevels
    {
        #region Construction
        public AwarenessSet(Creature creature)
        {
            _Creature = creature;
            _Awarenesses = new Dictionary<Guid, AwarenessLevel>();
        }
        #endregion

        #region data
        private Dictionary<Guid, AwarenessLevel> _Awarenesses;
        private Dictionary<Guid, Locator> _Roots;
        private HashSet<Guid> _DarkDraws = new HashSet<Guid>();
        private Creature _Creature;
        #endregion

        /// <summary>Clear awarenesses and found</summary>
        public void Clear()
        {
            _Awarenesses.Clear();
            _DarkDraws?.Clear();
            _Roots?.Clear();
        }

        #region public void RecalculateAwareness(ISensorHost sensors)
        public void RecalculateAwareness(ISensorHost sensors)
        {
            // refresh
            var _map = Locator.FindFirstLocator(Creature).Map;
            var _all = _map.GetAwarenessResults(Creature, sensors).ToList();

            // get darkdraws
            _DarkDraws = new HashSet<Guid>(_all
                .Where(_a => _a.AwarenessLevel == AwarenessLevel.DarkDraw)
                .Select(_a => _a.ID)
                .Distinct());

            // get awarenesses
            _Awarenesses = _all
                .Where(_a => _a.AwarenessLevel > AwarenessLevel.None)
                .ToDictionary(_a => _a.ID, _a => _a.AwarenessLevel);

            // and locators
            _Roots = _all
                .Where(_a => (_a.AwarenessLevel > AwarenessLevel.None) && (_a.Locator.ICore?.ID == _a.ID))
                .ToDictionary(_a => _a.ID, _a => _a.Locator);

            // determine whether escalation to turn tick is appropriate
            // NOTE: PromptTurnTrackerStep is launched in LocalTurnTracker.CompleteStep for LocalTimeStep
            if (sensors is Creature _critter)
            {
                NeedsTurnTick.TryBindToCreature(_critter);
            }
        }
        #endregion

        #region public AwarenessLevel GetAwarenessLevel(Guid guid)
        public AwarenessLevel GetAwarenessLevel(Guid guid)
        {
            // completely aware
            var _aware = this[guid];
            if (_aware == AwarenessLevel.Aware)
                return _aware;

            // action aware
            if (Creature.Adjuncts.OfType<IActionAwareProvider>().Any(_aap => _aap.IsActionAware(guid) ?? false))
                return AwarenessLevel.Aware;

            // or whatever we first got
            return _aware;
        }
        #endregion

        #region public bool IsTotalConcealmentMiss(Guid guid)
        /// <summary>Would miss by total concealment?</summary>
        public bool IsTotalConcealmentMiss(Guid guid)
        {
            // completely aware
            var _aware = this[guid];
            if (_aware == AwarenessLevel.Aware)
                return false;

            // action aware
            if (Creature.Adjuncts.OfType<IActionAwareProvider>().Any(_aap => _aap.IsActionAware(guid) ?? true))
                return false;

            // or whatever we first got
            return true;
        }
        #endregion

        /// <summary>Aware of the provider enough to use actions it provides</summary>
        public bool IsActionAware(Guid guid)
            => (this[guid] == AwarenessLevel.Aware)
            || Creature.Adjuncts.OfType<IActionAwareProvider>().Any(_aap => _aap.IsActionAware(guid) ?? false);

        public bool ShouldDraw(Guid guid)
            => (_Awarenesses.TryGetValue(guid, out var _aware) && (_aware == AwarenessLevel.Aware))
            || _DarkDraws.Contains(guid);

        public AwarenessLevel this[Guid guid]
            => (_Awarenesses.TryGetValue(guid, out var _aware))
            ? _aware
            : AwarenessLevel.None;

        #region public IEnumerable<AwarenessLocatorCore> GetAwareLocatorCores()
        public IEnumerable<AwarenessLocatorCore> GetAwareLocatorCores()
        {
            var _loc = Creature?.GetLocated()?.Locator;
            if (_loc != null)
            {
                foreach (var _alc in (from _a in _Awarenesses
                                      where (_a.Value >= AwarenessLevel.Aware) && _Roots.ContainsKey(_a.Key)
                                      let _l = _Roots[_a.Key]
                                      from _c in _l.ICoreAs<ICoreObject>()
                                      select new AwarenessLocatorCore
                                      {
                                          ICoreObject = _c,
                                          Locator = _l
                                      }))
                {
                    yield return _alc;
                }
            }
            yield break;
        }
        #endregion

        #region IEnumerable<KeyValuePair<Guid, AwarenessLevel>> Members
        public IEnumerator<KeyValuePair<Guid, AwarenessLevel>> GetEnumerator()
        {
            foreach (KeyValuePair<Guid, AwarenessLevel> _aware in _Awarenesses)
                yield return _aware;
            yield break;
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (KeyValuePair<Guid, AwarenessLevel> _aware in _Awarenesses)
                yield return _aware;
            yield break;
        }
        #endregion

        // ICreatureBound Members
        public Creature Creature => _Creature;

        public IEnumerable<Creature> FriendlyAwarenesses
            => (from _kvp in _Awarenesses
                where _kvp.Value >= AwarenessLevel.Aware
                let _id = _kvp.Key
                where Creature.IsFriendly(_id)
                let _critter = IkosaStatics.CreatureProvider?.GetCreature(_id)
                where _critter != null
                select _critter);

        public IEnumerable<Creature> UnFriendlyAwarenesses
            => (from _kvp in _Awarenesses
                where _kvp.Value >= AwarenessLevel.Aware
                let _id = _kvp.Key
                where Creature.IsUnfriendly(_id)
                let _critter = IkosaStatics.CreatureProvider?.GetCreature(_id)
                where _critter != null
                select _critter);

        public static void RecalculateAllSensors(LocalMap map, List<Guid> sensorIDs, bool roomRecalc)
        {
            if (sensorIDs.Any())
            {
                var _idx = map.ContextSet.GetCoreIndex();
                Parallel.ForEach(sensorIDs, _id =>
                {
                    if (_idx.TryGetValue(_id, out ICore _core))
                    {
                        if (_core is ISensorHost _sensors
                            && _sensors.IsSensorHostActive)
                        {
                            if (roomRecalc)
                            {
                                _sensors.RoomAwarenesses?.RecalculateAwareness(_sensors);
                            }
                            _sensors.Awarenesses?.RecalculateAwareness(_sensors);
                        }
                        else
                        {
                            Debug.WriteLine($@"_idx[{_id}] not ISensorHost!");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($@"_idx[{_id}] not found!");
                    }
                });
            }
        }
    }
}
