using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Scare : SpellDef, ISpellMode, IDurableCapable, ISaveCapable, ICounterDispelCapable
    {
        public override string DisplayName => @"Scare";
        public override string Description => @"Living creatures (6 PD or less) frightened or shaken";
        public override MagicStyle MagicStyle => new Necromancy();

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Fear();
                yield return new MindAffecting();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new MaterialComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<ISpellMode> SpellModes { get; }
        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                yield return this;
                yield break;
            }
        }
        #endregion

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            var _max = new LinearRange(0, 1, 3);
            yield return new AwarenessAim(@"Creature", @"Living Creature",
                FixedRange.One, _max, new NearRange(), new CreatureTargetType());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            var _targets = (from _aware in deliver.TargetingProcess.Targets.OfType<AwarenessTarget>()
                            where _aware.Key.Equals(@"Creature", StringComparison.OrdinalIgnoreCase)
                                && ((_aware.Target as Creature)?.AdvancementLog.NumberPowerDice <= 6)
                            select _aware).Cast<AimTarget>().ToList();
            if (_targets.Any())
            {
                SpellDef.DeliverDurableInCluster(deliver, _targets, true, 30, 0, 1);
            }
            else
            {
                deliver.Notify(@"No targets within spell capacity.", @"Failed", false);
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // Convert Prerequisites to Targets for SpellDef...
            var _savePre = apply.AllPrerequisites<SavePrerequisite>(@"Save.Will").FirstOrDefault();
            var _effects = ((MagicPowerEffectTransit<SpellSource>)apply.DeliveryInteraction.InteractData)
                .MagicPowerEffects;

            // if save fails, use frightened, else use shaken
            if (!(_savePre?.Success ?? true))
            {
                // using frightened (remove shaken)
                var _shaken = _effects.FirstOrDefault(_e => _e.SubMode == 1);
                _effects.Remove(_shaken);
            }
            else
            {
                // using shaken (remove frightened)
                var _frightened = _effects.FirstOrDefault(_e => _e.SubMode == 0);
                _effects.Remove(_frightened);
            }

            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode Members
        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0; // frightened
                yield return 1; // shaken
                yield break;
            }
        }

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                Adjunct _fear = null;
                switch (_spellEffect.SubMode)
                {
                    case 0:
                        _fear = new FrightenedEffect(source);
                        break;
                    case 1:
                        _fear = new ShakenEffect(source);
                        break;
                }
                if (target is Creature _critter)
                {
                    _critter.AddAdjunct(_fear);
                }
                return _fear;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as Adjunct)?.Eject();
        }

        public bool IsDismissable(int subMode) { return false; }
        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
        {
            switch (subMode)
            {
                case 0:
                    // NOTE: duration for frightened...
                    return new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));

                case 1:
                    // NOTE: duration for shaken...
                    return new DurationRule(DurationType.Span, new SpanRulePart(1, new Round()));
            }
            // NOTE: duration for shaken...
            return new DurationRule(DurationType.Span, new SpanRulePart(1, new Round()));
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Partial, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion

        #region ICounterDispelMode Members
        public IEnumerable<Type> CounterableSpells
        {
            get
            {
                yield return typeof(RemoveFear);
                yield break;
            }
        }

        public IEnumerable<Type> DescriptorTypes { get { yield break; } }
        #endregion
    }
}
