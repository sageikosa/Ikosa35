using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class SpellCastSound : IAudible
    {
        public SpellCastSound(CoreActivity activity)
        {
            _ID = Guid.NewGuid();
            _Activity = activity;
        }

        #region state
        private Guid _ID;
        private CoreActivity _Activity;
        #endregion

        // IAudible
        public Guid SoundGroupID => _ID;
        public Guid SourceID => _Activity.Actor.ID;
        public string Name => $@"Spell-Cast Sound";

        public SoundInfo GetSoundInfo(ISensorHost sensors, SoundAwareness awareness)
        {
            // how well the awareness was perceived
            var _exceed = awareness.CheckExceed;
            var _presence = Math.Max(awareness.SourceRange, 1);
            var _strength = (awareness.Magnitude ?? 0) / _presence;

            // creature trained in spellcraft?
            if (sensors.Skills.Skill<SpellcraftSkill>() is SpellcraftSkill _craft
                && _craft.IsTrained)
            {
                // ensure its a spell casting (or suitable subclass)
                if (_Activity.Action is CastSpell _cast)
                {
                    var _level = _cast.SpellSource.PowerLevel;
                    var _self = _Activity.Actor == sensors.Senses.Creature;
                    if (_self || _craft.AutoCheck(_level + 15, _cast))
                    {
                        Debug.WriteLine($@"{_craft.Creature.Name} spellcraft check succeeds");

                        // aura
                        var _aura = _level < 4 ? AuraStrength.Faint :
                            _level < 7 ? AuraStrength.Moderate :
                            _level < 10 ? AuraStrength.Strong :
                            AuraStrength.Overwhelming;

                        // spell info, but no indication of slot level
                        var _spellInfo = _cast.SpellSource.ToSpellSourceInfo();

                        // clear things we shouldn't know about (power class, caster level and slot level)
                        _spellInfo.SlotLevel = _self ? _spellInfo.SlotLevel : null;
                        _spellInfo.CasterLevel = _self ? _spellInfo.CasterLevel : 0;
                        _spellInfo.PowerClass = _self ? _spellInfo.PowerClass : null;

                        // geometric region from strongest impact
                        var _impact = awareness.SoundImpacts.OrderByDescending(_i => _i.RelativeMagnitude).FirstOrDefault();
                        if (_impact != null)
                        {
                            sensors.ExtraInfoMarkers.Add(new ExtraInfoMarker(
                                new ExtraInfoSource
                                {
                                    ID = _craft.ID,
                                    Message = _craft.SkillName
                                },
                                new Informable(_spellInfo), _impact.Region, false, _aura, null));
                        }
                    }
                }
            }

            // TODO: language dependency?
            var _info = new SoundInfo
            {
                Strength = _strength,
                Description = $@"{SoundInfo.GetStrengthDescription(_presence, _strength, _exceed)}spell-casting",
            };

            return _info;
        }

        public void LostSoundInfo(ISensorHost sensors)
        {
            if (_Activity.Action is CastSpell _cast)
            {
                var _critter = sensors as Creature;
                foreach (var _extra in (from _eim in sensors.ExtraInfoMarkers.All
                                        from _s in _eim.Informations.Inform(_critter).OfType<SpellSourceInfo>()
                                        where _s.ID == _cast.SpellSource.ID
                                        select _eim).ToList())
                {
                    sensors.ExtraInfoMarkers.Remove(_extra, false);
                }
            }
        }
    }
}
