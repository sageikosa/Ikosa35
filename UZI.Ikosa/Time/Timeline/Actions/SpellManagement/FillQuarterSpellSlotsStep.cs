using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class FillQuarterSpellSlotsStep : CoreStep
    {
        public FillQuarterSpellSlotsStep(CoreActivity activity, IPreparedCasterClass caster)
            : base(activity)
        {
            _Caster = caster;
        }

        #region data
        private IPreparedCasterClass _Caster;
        #endregion

        public IPreparedCasterClass Caster => _Caster;

        public CoreActivity Activity
            => Process as CoreActivity;

        public override bool IsDispensingPrerequisites => false;

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            var _critter = Activity.Actor as Creature;
            var _time = _critter?.GetCurrentTime() ?? 0;
            var _prepTarget = Activity.GetFirstTarget<PrepareSpellSlotsTarget>(@"PrepareSpells.Slots");
            if ((_prepTarget != null) && (_critter.ID == Caster.OwnerID))
            {
                // max slots to determine when to cut and schedule next activity
                var _maxFill = (int)Math.Ceiling(Caster.AllSpellSlots.Count() / 4m);
                var _sx = 0;
                var _availSpells = Caster.AllSpellSlotSets.ToDictionary(
                    _s => _s.SetIndex,
                    _s => Caster.PreparableSpells(_s.SetIndex).ToDictionary(_sp => _sp.SpellDef.Key));
                var _metas = _critter.Feats.OfType<MetamagicFeatBase>().ToDictionary(_f => _f.MetaMagicTag);

                // loop through multiple times until all are handled or _maxFill reached
                while (_prepTarget.SlotSets.Any() && (_sx < _maxFill))
                {
                    // step through all targeted slot-sets
                    foreach (var _tSet in _prepTarget.SlotSets.ToList())
                    {
                        var _set = Caster.GetSpellSlotSets(_tSet.SetIndex);

                        // step through all targeted levels
                        foreach (var _tLevel in _tSet.SlotLevels.ToList())
                        {
                            var _level = _set[_tLevel.SlotLevel];
                            var _maxLevelFill = (int)Math.Ceiling(_level.SlotCount / 4m);
                            var _lx = 0;

                            // step through all targeted slots
                            foreach (var _tSlot in _tLevel.SpellSlots.OfType<PreparedSpellSlotInfo>().Where(_pss => _pss.SpellSource != null).ToList())
                            {
                                // get targeted spell from available spell list
                                if (_availSpells[_tSet.SetIndex].TryGetValue(_tSlot.SpellSource.SpellDef.Key, out ClassSpell _spell))
                                {
                                    // targetted spell must fit in the level
                                    if (_spell.Level <= _tLevel.SlotLevel)
                                    {
                                        // spell source for un-adjusted spell def
                                        ISpellDef _def = _spell.SpellDef;
                                        var _source = new SpellSource(Caster, _spell.Level, _tLevel.SlotLevel, false, _def);

                                        // consider metas
                                        var _adjMetas = (from _m in _tSlot.SpellSource.SpellDef.MetaMagics
                                                         let _fm = _metas.ContainsKey(_m.MetaTag) ? _metas[_m.MetaTag] : null
                                                         where _fm != null
                                                         select _fm).ToList();
                                        if (_adjMetas.Any())
                                        {
                                            // make sure meta adjustments didn't exceed slot level
                                            var _adjLevel = _adjMetas.Sum(_m => _m.SlotAdjustment) + _spell.Level;
                                            if (_adjLevel <= _tLevel.SlotLevel)
                                            {
                                                // apply all meta-magics
                                                foreach (var _adj in _adjMetas)
                                                {
                                                    _def = _adj.ApplyMetamagic(_def, false);
                                                }

                                                // heighten gets special treatment, it affects the spell source
                                                var _heighten = Math.Max(0,
                                                    _adjMetas.OfType<HeightenSpellFeat>().Any() ? (_tLevel.SlotLevel - _adjLevel) : 0);
                                                _source = new SpellSource(Caster, _spell.Level + _heighten, _tLevel.SlotLevel, false, _def);
                                            }
                                        }

                                        // prepared spell
                                        var _slot = _level[_tSlot.SlotIndex];
                                        _slot.PreparedSpell = _source;
                                        _sx++;
                                        _lx++;
                                    }
                                }

                                // draw down target 
                                _tLevel.SpellSlots.Remove(_tSlot);

                                // break if total fill exceeded, or level fill exceeded
                                if ((_sx >= _maxFill) || (_lx >= _maxLevelFill))
                                {
                                    break;
                                }
                            }

                            // clear target level if empty
                            if (!_tLevel.SpellSlots.OfType<PreparedSpellSlotInfo>().Any())
                            {
                                _tSet.SlotLevels.Remove(_tLevel);
                            }

                            // break if total fill exceeded
                            if (_sx >= _maxFill)
                            {
                                break;
                            }
                        }

                        // clear target level if empty
                        if (!_tSet.SlotLevels.Any())
                        {
                            _prepTarget.SlotSets.Remove(_tSet);
                        }

                        // break if total fill exceeded
                        if (_sx >= _maxFill)
                        {
                            break;
                        }
                    }
                }

                // got done all that could be done, but still had some left
                if (_prepTarget.SlotSets.Any())
                {
                    // start a follow-on action
                    if (_critter.GetLocalActionBudget() is LocalActionBudget _budget)
                    {
                        _budget.NextActivity = new CoreActivity(_critter, new PrepareMoreSpells(_critter, Caster), Activity.Targets);
                    }
                }
            }
            return true;
        }
        #endregion
    }
}
