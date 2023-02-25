using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Universal;
using Uzi.Visualize;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class SoundAwarenessSet : ISerializable
    {
        public SoundAwarenessSet(ISensorHost sensors)
        {
            _Sensors = sensors;
            _Awarenesses = new ConcurrentDictionary<Guid, SoundAwareness>();
        }

        protected SoundAwarenessSet(SerializationInfo info, StreamingContext context)
        {
            _Sensors = (ISensorHost)info.GetValue(nameof(_Sensors), typeof(ISensorHost));

            foreach (var _p in info)
            {
                if (_p.Name.Equals(@"Awarenesses"))
                {
                    var _awareness = (Dictionary<Guid, SoundAwareness>)info.GetValue(@"Awarenesses", typeof(Dictionary<Guid, SoundAwareness>));
                    _Awarenesses = new ConcurrentDictionary<Guid, SoundAwareness>(_awareness);
                    break;
                }
            }
            _Awarenesses ??= new();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(_Sensors), _Sensors);
            info.AddValue(@"Awarenesses", _Awarenesses.ToDictionary(_kvp => _kvp.Key, _kvp => _kvp.Value));
        }

        #region state
        private ConcurrentDictionary<Guid, SoundAwareness> _Awarenesses;
        private ISensorHost _Sensors;
        #endregion

        public ISensorHost Sensors => _Sensors;

        public SoundAwareness GetAwareness(Guid id)
            => _Awarenesses.TryGetValue(id, out var _return)
            ? _return
            : null;

        public bool CanTake10OnListen
            => (!(Sensors.Senses.Creature?.ProcessManager as IkosaProcessManager)?.LocalTurnTracker.IsInitiative ?? false)
            && (Sensors.Senses.Creature?.IsTake10InEffect(typeof(ListenSkill)) ?? false);

        public IEnumerable<SoundAwareness> GetAll()
            => _Awarenesses.Values.Select(_a => _a);

        #region public void ResolveSounds(IDictionary<Guid, List<(SoundRef sound, IGeometricRegion region, double magnitude)>> soundGroups, Func<Guid, bool> soundFilter)
        public void ResolveSounds(IDictionary<Guid, List<(SoundRef sound, IGeometricRegion region, double magnitude)>> soundRefs,
            Func<Guid, bool> soundFilter)
        {
            var _locator = Sensors.GetLocated()?.Locator;
            if (_locator != null)
            {
                var _ptImpact = _locator.GeometricRegion.GetPoint3D();
                var _time = _locator.Map.CurrentTime;

                // must be able to hear!
                var _senses = Sensors.Senses;
                var _critter = Sensors.Senses.Creature;
                var _canTake10 = CanTake10OnListen;
                if (_senses.AllSenses.Any(_sense => _sense.UsesHearing && _sense.IsActive))
                {
                    // clear old impacts from awarenesses not in new (legacy awareness without sound impact)
                    // also, sound filter must allow processing this sound
                    foreach (var _aware in _Awarenesses.Where(_a => !soundRefs.ContainsKey(_a.Key) && soundFilter(_a.Key)).ToList())
                    {
                        // hold onto IDs and checks, but clear out impacts
                        _aware.Value.ClearImpacts(_time, _ptImpact);
                        // TODO: ... time-out/distraction/other sounds can remove
                    }

                    foreach (var _sRef in soundRefs)
                    {
                        // get previous roll information (do not notify principal of roll-value)
                        var _unpack = _sRef.Value.FirstOrDefault().sound;
                        var _audible = _unpack?.Audible;
                        var _srcRange = _unpack?.SourceRange ?? 1;
                        var _message = _audible?.Name ?? @"Sound";
                        var _prev = _Awarenesses.TryGetValue(_sRef.Key, out var _existing);

                        // always taking 10 to listen (reactively) if not in initiative...
                        var _rollValue = _existing?.RollValue ?? (_canTake10 ? 10 : DieRoller.RollDie(_critter.ID, 20, @"Listen", _message));
                        var _lastAware = _existing?.HasNoticed;

                        // fallback check info
                        DeltaCalcInfo _leastDiff = null;
                        DeltaCalcInfo _leastCheck = null;
                        var _exceed = 0;

                        var _impacts = new List<SoundImpact>();
                        var _contingent = new List<SoundImpact>();

                        // look at all regions producing this sound
                        foreach (var (_sound, _region, _magnitude) in _sRef.Value)
                        {
                            // transit for each region
                            var _data = _sound.GetTransit(_region, _locator);
                            if (_data != null)
                            {
                                // sound didn't get negated
                                if (_Sensors is IInteract _interactor)
                                {
                                    // associate checks (pre-load contingent so we don't have to check again)
                                    _data.CheckRoll = _rollValue;
                                    _data.LastAware = _lastAware;

                                    var _interact = new Interaction(_critter, _locator.Map.GetICore<ICore>(_sound.Audible.SourceID), _interactor, _data);
                                    _interactor.HandleInteraction(_interact);
                                    var _heard = _interact.Feedback.OfType<SoundFeedback>().FirstOrDefault();
                                    if (_heard != null)
                                    {
                                        // heard feedback indicates we heard it
                                        _impacts.Add(new SoundImpact(_region, _data.RangeRemaining, _heard.DifficultyInfo, _heard.CheckInfo));
                                        _exceed = Math.Max(_exceed, _heard.CheckInfo.Result - _heard.DifficultyInfo.Result);
                                    }
                                    else if (!_impacts.Any())
                                    {
                                        // did not hear, figure out what should have been heard (at least until we hear something)
                                        var _checkInfo = (new ScoreDeltable(_data.CheckRoll, _Sensors.Skills.Skill<ListenSkill>(), @"Listen Skill"))
                                            ?.Score.GetDeltaCalcInfo(_interact, @"Listen Skill");
                                        _leastCheck = (_leastCheck ?? _checkInfo)?.Result >= _checkInfo.Result
                                            ? _checkInfo
                                            : _leastCheck;
                                        var _soundInfo = _data.GetListenDifficulty();
                                        _leastDiff = (_leastDiff ?? _soundInfo)?.Result >= _soundInfo.Result
                                            ? _soundInfo
                                            : _leastDiff;
                                    }
                                }
                            }
                        }

                        if (!_prev)
                        {
                            // need a new awareness
                            _existing = new SoundAwareness(_audible, _rollValue);
                            _Awarenesses.AddOrUpdate(_sRef.Key, _existing, (id, found) => _existing);

                            // notify, use biggest impact (assuming one was made)
                            if (_impacts.Any())
                            {
                                var _notify = Deltable.GetCheckNotify(_critter.ID, string.Empty, Guid.Empty, string.Empty);
                                var _max = _impacts.OrderByDescending(_i => _i.RelativeMagnitude).FirstOrDefault();
                                _notify.CheckInfo = _max?.Check ?? _leastCheck;
                                _notify.OpposedInfo = _max?.Difficulty ?? _leastDiff;
                            }
                            else if ((_leastCheck != null) && (_leastDiff != null))
                            {
                                // otherwise, using least stuff
                                var _notify = Deltable.GetCheckNotify(_critter.ID, string.Empty, Guid.Empty, string.Empty);
                                _notify.CheckInfo = _leastCheck;
                                _notify.OpposedInfo = _leastDiff;
                            }
                        }
                        else if (!_existing.HasNoticed && _impacts.Any())
                        {
                            // hadn't noticed, but now could...
                            var _notify = Deltable.GetCheckNotify(_critter.ID, string.Empty, Guid.Empty, string.Empty);
                            var _max = _impacts.OrderByDescending(_i => _i.RelativeMagnitude).FirstOrDefault();
                            _notify.CheckInfo = _max?.Check ?? _leastCheck;
                            _notify.OpposedInfo = _max?.Difficulty ?? _leastDiff;
                        }

                        // set awareness impacts
                        _existing.SetImpacts(_impacts, Sensors, _time, _ptImpact, _audible, _srcRange, _exceed);
                    }
                }
                else
                {
                    // clear
                    foreach (var _aware in _Awarenesses.Values)
                    {
                        _aware.ClearImpacts(_time, _ptImpact);
                    }
                }
            }
        }
        #endregion

        #region public void Cleanup()
        /// <summary>Clears notices that can be cleared.  Removes sounds that can be removed</summary>
        public void Cleanup()
        {
            var _locator = Sensors.GetLocated()?.Locator;
            if (_locator != null)
            {
                var _time = _locator.Map.CurrentTime;
                foreach (var _aware in _Awarenesses.Where(_a => _a.Value.CanClearNotice(_time)))
                {
                    // clear lost notices
                    _aware.Value.ClearNotice();
                }

                foreach (var _aware in _Awarenesses.Where(_a => _a.Value.IsRemoveable(_time)).ToList())
                {
                    // remove removeable
                    _Awarenesses.TryRemove(_aware.Key, out var _sound);
                    _sound.Audible.LostSoundInfo(Sensors);
                }
            }
        }
        #endregion
    }
}
