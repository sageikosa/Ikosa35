using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class PrepareSpellSlotAim : AimingMode
    {
        public PrepareSpellSlotAim(string key, string displayName, IPreparedCasterClass casterClass, bool abandonRecharge)
            : base(key, displayName, FixedRange.One, FixedRange.One)
        {
            _CasterClass = casterClass;
            _Recharge = abandonRecharge;
        }

        #region data
        private IPreparedCasterClass _CasterClass;
        private bool _Recharge;
        #endregion

        public IPreparedCasterClass CasterClass => _CasterClass;
        public bool AbandonRecharge => _Recharge;

        #region public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, AimTargetInfo[] infos, IInteractProvider provider)
        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, AimTargetInfo[] infos, IInteractProvider provider)
        {
            var _time = actor?.GetCurrentTime() ?? 0;
            var _allTargets = (from _a in SelectedTargets<PrepareSpellSlotsTargetInfo>(actor, action, infos)
                               select _a).ToList();
            var _slotSets = _allTargets.FirstOrDefault()?.SlotSets;
            if (_slotSets != null)
            {
                // build and validate target
                var _target = new PrepareSpellSlotsTarget(_allTargets.FirstOrDefault().Key, null);
                foreach (var _inSet in _slotSets.Where(_ss => _ss.SetIndex < CasterClass.SlotSetCount && _ss.SetIndex >= 0))
                {
                    // get valid sets
                    var _realSet = CasterClass.GetSpellSlotSets(_inSet.SetIndex);
                    var _gatherSet = new SpellSlotSetInfo
                    {
                        ID = _realSet.ID,
                        SetIndex = _inSet.SetIndex,
                        SlotLevels = new List<SpellSlotLevelInfo>()
                    };
                    foreach (var _inLevel in _inSet.SlotLevels
                        .Where(_sl => _sl.SlotLevel <= _realSet.MaximumSlotLevel && _sl.SlotLevel >= _realSet.MinimumSlotLevel))
                    {
                        // gather valid levels
                        var _inboundSlots = _inLevel.SpellSlots.OfType<PreparedSpellSlotInfo>()
                            .Where(_is => _is.IsCharged || (_is.CanRecharge && AbandonRecharge))
                            .ToList();
                        var _indexer = _realSet.SlotsForSpellLevel(_inLevel.SlotLevel).ToList();
                        var _candidateSlots = _indexer.Select(_i => _i).ToList();
                        var _maxSlots = _candidateSlots.Count;
                        var _gatherLevel = new SpellSlotLevelInfo
                        {
                            SlotLevel = _inLevel.SlotLevel,
                            SpellSlots = new List<SpellSlotInfo>()
                        };

                        // slot counter
                        var _sx = 0;
                        void _slotTally(PreparedSpellSlotInfo info, PreparedSpellSlot slot)
                        {
                            _inboundSlots.Remove(info);
                            _candidateSlots.Remove(slot);
                            _sx++;
                        }

                        #region unchanged
                        // slots with spells in them
                        foreach (var _inSlot in (from _is in _inboundSlots
                                                 where !string.IsNullOrWhiteSpace(_is.SpellSource?.SpellDef?.Key)
                                                 select _is).ToList())
                        {
                            if (_sx < _maxSlots)
                            {
                                // find unchanged and remove from consideration
                                var _keeper = (from _ks in _candidateSlots
                                               where _ks.PreparedSpell?.SpellDef?.DoSpellDefsMatch(_inSlot.SpellSource.SpellDef) ?? false
                                               select _ks).FirstOrDefault();
                                if (_keeper != null)
                                {
                                    // explicitly unchanged slot
                                    _slotTally(_inSlot, _keeper);
                                }
                            }
                        }
                        #endregion

                        #region prepare into charged slot
                        // slots with spells in them
                        foreach (var _inSlot in (from _is in _inboundSlots
                                                 where !string.IsNullOrEmpty(_is.SpellSource?.SpellDef?.Key)
                                                 select _is).ToList())
                        {
                            if (_sx < _maxSlots)
                            {
                                // new spell into a charged but unfilled slot...
                                var _ready = _candidateSlots.FirstOrDefault(_cs => _cs.IsCharged && _cs.PreparedSpell == null);
                                if (_ready != null)
                                {
                                    // actual spell source will be used when processed
                                    _gatherLevel.SpellSlots.Add(new PreparedSpellSlotInfo
                                    {
                                        SlotIndex = _indexer.IndexOf(_ready),
                                        SlotLevel = _inLevel.SlotLevel,
                                        IsCharged = true,
                                        CanRecharge = false,
                                        SpellSource = _inSlot.SpellSource
                                    });
                                    _slotTally(_inSlot, _ready);
                                }
                            }
                        }
                        #endregion

                        // able to recharge with current action
                        if (AbandonRecharge)
                        {
                            #region prepare into uncharged slot (while recharging)
                            // slots with spells in them
                            foreach (var _inSlot in (from _is in _inboundSlots
                                                     where !string.IsNullOrEmpty(_is.SpellSource?.SpellDef?.Key)
                                                     select _is).ToList())
                            {
                                if (_sx < _maxSlots)
                                {
                                    // recharge with new spell into uncharged slot
                                    var _pending = _candidateSlots.FirstOrDefault(_cs => !_cs.IsCharged && _cs.CanRecharge(_time));
                                    if (_pending != null)
                                    {
                                        // actual spell source will be used when processed
                                        _gatherLevel.SpellSlots.Add(new PreparedSpellSlotInfo
                                        {
                                            SlotIndex = _indexer.IndexOf(_pending),
                                            SlotLevel = _inLevel.SlotLevel,
                                            IsCharged = true,
                                            CanRecharge = false,
                                            SpellSource = _inSlot.SpellSource
                                        });
                                        _slotTally(_inSlot, _pending);
                                    }
                                }
                            }
                            #endregion

                            #region abandon spell in slot and prepare new spell in slot
                            // slots with spells in them
                            foreach (var _inSlot in (from _is in _inboundSlots
                                                     where !string.IsNullOrEmpty(_is.SpellSource?.SpellDef?.Key)
                                                     select _is).ToList())
                            {
                                if (_sx < _maxSlots)
                                {
                                    // recharge with new spell into uncharged slot
                                    var _replacer = _candidateSlots.FirstOrDefault(_cs => _cs.PreparedSpell != null);
                                    if (_replacer != null)
                                    {
                                        // actual spell source will be used when processed
                                        _gatherLevel.SpellSlots.Add(new PreparedSpellSlotInfo
                                        {
                                            SlotIndex = _indexer.IndexOf(_replacer),
                                            SlotLevel = _inLevel.SlotLevel,
                                            IsCharged = true,
                                            CanRecharge = false,
                                            SpellSource = _inSlot.SpellSource
                                        });
                                        _slotTally(_inSlot, _replacer);
                                    }
                                }
                            }
                            #endregion

                            #region charging an uncharged slot
                            // slots defined, but with no spells in them
                            foreach (var _inSlot in (from _is in _inboundSlots
                                                     where _is.SpellSource == null || _is.SpellSource.SpellDef == null
                                                     select _is).ToList())
                            {
                                if (_sx < _maxSlots)
                                {
                                    // recharge slot only
                                    var _charger = _candidateSlots.FirstOrDefault(_s => !_s.IsCharged && _s.CanRecharge(_time));
                                    if (_charger != null)
                                    {
                                        // explicitly indicate it should be charged but empty
                                        _gatherLevel.SpellSlots.Add(new PreparedSpellSlotInfo
                                        {
                                            SlotIndex = _indexer.IndexOf(_charger),
                                            SlotLevel = _inLevel.SlotLevel,
                                            IsCharged = true,
                                            CanRecharge = false,
                                            SpellSource = null
                                        });
                                        _slotTally(_inSlot, _charger);
                                    }
                                }
                            }
                            #endregion

                            #region abandon spell in slot and leave charged but empty
                            // slots defined, but with no spells in them
                            foreach (var _inSlot in (from _is in _inboundSlots
                                                     where _is.SpellSource == null || _is.SpellSource.SpellDef == null
                                                     select _is).ToList())
                            {
                                if (_sx < _maxSlots)
                                {
                                    // abandon spell
                                    var _abandonner = _candidateSlots.FirstOrDefault(_cs => _cs.PreparedSpell != null);
                                    if (_abandonner != null)
                                    {
                                        // explicitly indicate it should be charged but empty
                                        _gatherLevel.SpellSlots.Add(new PreparedSpellSlotInfo
                                        {
                                            SlotIndex = _indexer.IndexOf(_abandonner),
                                            SlotLevel = _inLevel.SlotLevel,
                                            IsCharged = true,
                                            CanRecharge = false,
                                            SpellSource = null
                                        });
                                        _slotTally(_inSlot, _abandonner);
                                    }
                                }
                            }
                            #endregion
                        }

                        // if gather level collected any spellSlots, add to SlotLevels in target set
                        if (_gatherLevel.SpellSlots.Any())
                        {
                            _gatherSet.SlotLevels.Add(_gatherLevel);
                        }
                    }

                    // if gather target set collected any levels, add to slot sets in _target
                    if (_gatherSet.SlotLevels.Any())
                        _target.SlotSets.Add(_gatherSet);
                }

                // target, if any collected
                if (_target.SlotSets.Any())
                    yield return _target;
            }
            yield break;
        }
        #endregion

        #region public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _time = actor?.GetCurrentTime() ?? 0;
            var _info = ToInfo<PrepareSpellSlotsAimInfo>(action, actor);
            _info.AbandonRecharge = AbandonRecharge;
            _info.SlotSets = (from _set in CasterClass.AllSpellSlotSets
                              select _set.SlotSet.ToSpellSlotSetInfo(CasterClass, _set.SetIndex, _set.SetName, _time)).ToList();
            if (actor is Creature _critter)
            {
                _info.AvailableMetaMagics = _critter.Feats.OfType<MetamagicFeatBase>()
                    .Select(_mmf => new MetaMagicInfo
                    {
                        Message = _mmf.MetaMagicBenefit,
                        MetaTag = _mmf.MetaMagicTag,
                        SlotAdjustment = _mmf.SlotAdjustment,
                        PresenterID = _mmf.PresenterID
                    }).ToList();
            }
            return _info;
        }
        #endregion
    }
}
