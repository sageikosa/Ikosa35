using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Time;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class HideousLaughter : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Hideous Laughter";
        public override string Description => @"Target falls prone and cannot take actions while laughing";
        public override MagicStyle MagicStyle => new Enchantment(Enchantment.SubEnchantment.Compulsion);
        public override IEnumerable<Descriptor> Descriptors => new MindAffecting().ToEnumerable();
        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, true, false, false);

        public override IEnumerable<SpellComponent> DivineComponents
            => YieldComponents(true, true, false, false, true);

        // ISpellMode
        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new AwarenessAim(@"Creature", @"Creature", FixedRange.One, FixedRange.One, new NearRange(), new CreatureTargetType())
            .ToEnumerable();

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.GetFirstTarget<AwarenessTarget>(@"Humanoid"), 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        // ISaveCapable
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            var _mode = new SaveMode(SaveType.Will, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
            _mode.QualifiedDeltas.Add(new HideousLaughterSaveBonus((actor as Creature)?.CreatureType));
            return _mode;
        }

        // IDurableCapable
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();
        public bool IsDismissable(int subMode) => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _critter = target as Creature;
            var _effect = new HideousLaughterEffect(source);
            _critter.AddAdjunct(_effect);
            return _effect;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
            => (source.ActiveAdjunctObject as HideousLaughterEffect)?.Eject();
    }

    [Serializable]
    public class HideousLaughterSaveBonus : IQualifyDelta
    {
        /// <summary>+4 if creature type different</summary>
        public HideousLaughterSaveBonus(CreatureType creatureType)
        {
            _CType = creatureType;
            _Term = new TerminateController(this);
        }

        #region state
        private CreatureType _CType;
        private TerminateController _Term;
        #endregion

        public int TerminateSubscriberCount
            => _Term.TerminateSubscriberCount;

        public void AddTerminateDependent(IDependOnTerminate subscriber)
            => _Term.AddTerminateDependent(subscriber);

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            => _Term.RemoveTerminateDependent(subscriber);

        public void DoTerminate()
            => _Term.DoTerminate();

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if ((qualify.Target is Creature _critter)
                && (_critter.CreatureType.GetType() != _CType.GetType()))
            {
                yield return new Delta(4, this, @"Creature Type");
            }
            yield break;
        }
    }

    [Serializable]
    public class HideousLaughterEffect : Adjunct, IActionFilter, IAudible
    {
        public HideousLaughterEffect(object source)
            : base(source)
        {
            _ID = Guid.NewGuid();
        }

        #region state
        private Guid _ID;
        #endregion

        public override object Clone()
            => new HideousLaughterEffect(Source);

        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // filter
                _critter.Actions.Filters.Add(this, this);

                // drop prone
                _critter.AddAdjunct(new ProneEffect(this));

                // laughter
                var _sound = new SoundGroup(this, new SoundRef(this, 0, 120, _critter.GetSerialState()));
                var _participant = new SoundParticipant(this, _sound);
                _critter.AddAdjunct(_participant);
            }
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // remove filter and sound
                _critter.Actions.Filters.Remove(this);
                _critter.Adjuncts.OfType<SoundParticipant>().FirstOrDefault(_sp => _sp.Source == this)?.Eject();
            }
            base.OnDeactivate(source);
        }

        // IActionFilter
        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
           => true;

        // IAudible
        public Guid SoundGroupID => _ID;
        public Guid SourceID => Anchor?.ID ?? Guid.Empty;
        public string Name => $@"Laughing";

        public SoundInfo GetSoundInfo(ISensorHost sensors, SoundAwareness awareness)
        {
            // how well the awareness was perceived
            var _exceed = awareness.CheckExceed;
            var _presence = Math.Max(awareness.SourceRange, 1);
            var _strength = (awareness.Magnitude ?? 0) / _presence;

            var _info = new SoundInfo
            {
                Strength = _strength,
                Description = $@"{SoundInfo.GetStrengthDescription(_presence, _strength, _exceed)}laughing"
            };

            return _info;
        }

        public void LostSoundInfo(ISensorHost sensors)
        {
        }
    }
}
