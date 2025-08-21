using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Descriptions;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class SpellCompletion : SpellActivation, IDecipherable
    {
        #region Construction
        public SpellCompletion(SpellSource source, bool autoActivate)
            : base(source, new PowerCapacity(source), new SpellActivationCost(), true, null)
        {
            _Casters =
            [
                source.ID
            ];
            _AutoAct = autoActivate;
        }
        #endregion

        #region data
        private Collection<Guid> _Casters;
        private bool _AutoAct;
        #endregion

        /// <summary>Indicates the spell will activate when deciphered (such as a cursed scroll)</summary>
        public bool AutomaticActivation => _AutoAct;

        public Collection<Guid> Casters => _Casters;

        public Description GetDescription(Guid guid)
            => (HasDeciphered(guid))
            ? new Description(@"Spell Decipher",
                new string[]
                {
                    $@"Spell: {SpellSource.SpellDef.DisplayName}",
                    $@"Description: {SpellSource.SpellDef.Description}",
                    $@"Type: {SpellSource.SpellDef.MagicStyle.StyleName}/{SpellSource.CasterClass.MagicType}",
                    $@"Levels: Power={PowerLevel}, Caster={SpellSource.CasterClass.ClassPowerLevel.EffectiveValue}"
                })
            : new Description(@"Writing", @"Undeciphered text");

        #region public CoreStep Decipher(CoreActivity activity)
        public CoreStep Decipher(CoreActivity activity)
        {
            if (!HasDeciphered(activity.Actor.ID))
            {
                _Casters.Add(activity.Actor.ID);

                // TODO: consider auto activation...
                return activity.GetActivityResultNotifyStep(GetDescription(activity.Actor.ID));
            }
            return null;
        }
        #endregion

        /// <summary>Indicates that the actor associated with the guid has deciphered the magical writing</summary>
        public bool HasDeciphered(Guid guid)
            => _Casters.Contains(guid);

        public bool HasPossessorDeciphered
        {
            get
            {
                var _item = (Anchor as ICoreItem);
                if (_item?.Possessor != null)
                {
                    return HasDeciphered(_item.Possessor.ID);
                }
                return false;
            }
        }

        #region public override IEnumerable<CoreAction> GetActions(CoreActor actor)
        /// <summary>Yields actions (if spell completion adjunct is active)</summary>
        /// <param name="actor"></param>
        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsActive)
            {
                if (budget.Actor is Creature _critter)
                {
                    // if creature has ranks in use magic device, some of the limits may be surmountable with checks during the action
                    var _hasUseMagicDevice = _critter.Skills[typeof(UseMagicItemSkill)].BaseValue > 0;

                    if (HasDeciphered(_critter.ID))
                    {
                        if (_hasUseMagicDevice || UseDirectly(_critter))
                        {
                            var _budget = budget as LocalActionBudget;

                            if ((SpellSource.SpellDef.ActionTime.ActionTimeType == TimeType.Twitch)
                                && _budget.IsTwitchAvailable)
                            {
                                var _mx = 0;
                                foreach (var _mode in SpellSource.SpellDef.SpellModes)
                                {
                                    yield return new CompleteSpell(this, _mode, SpellSource.SpellDef.ActionTime, $@"{_mx:0#}");
                                    _mx++;
                                }
                            }
                            else if (_budget.CanPerformRegular)
                            {
                                var _mx = 0;
                                foreach (var _mode in SpellSource.SpellDef.SpellModes)
                                {
                                    yield return new CompleteSpell(this, _mode, new ActionTime(TimeType.Regular), $@"{_mx:0#}");
                                    _mx++;
                                }
                            }
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        public override Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Spell Completion", ID);

        #region public virtual bool UseDirectly(CoreActor actor)
        /// <summary>Indicates the creature can complete the magic without a use magic device check</summary>
        public virtual bool UseDirectly(CoreActor actor)
        {
            if (actor is Creature _critter)
            {
                var _spellType = SpellSource.SpellDef.SeedSpellDef.GetType();

                return (from _c in _critter.Classes
                        where typeof(ICasterClass).IsAssignableFrom(_c.GetType())
                        let _castClass = _c as ICasterClass
                        where (_castClass.MagicType == SpellSource.CasterClass.MagicType)              // arcane/divine match
                        from _classSpell in _castClass.UsableSpells
                        where _spellType.IsAssignableFrom(_classSpell.SpellDef.SeedSpellDef.GetType())      // has spell
                        && (_castClass.SpellDifficultyAbility.MaxSpellLevel() >= _classSpell.Level)                 // and can cast it
                        select _classSpell).FirstOrDefault() != null;
            }
            return false;
        }
        #endregion

        #region public static IEnumerable<SpellCompletion> GetSpellCompletions(ICoreObject coreObject)
        /// <summary>List all spell completions anchored to the particular object</summary>
        public static IEnumerable<SpellCompletion> GetSpellCompletions(ICoreObject coreObject)
        {
            if (coreObject is CoreObject _core)
            {
                foreach (var _completion in _core.Adjuncts.OfType<SpellCompletion>())
                {
                    yield return _completion;
                }
            }

            yield break;
        }
        #endregion

        public override object Clone()
            => new SpellCompletion(SpellSource, AutomaticActivation);

        public override decimal LevelUnitPrice
            => 25m;
    }
}
