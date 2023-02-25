using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class BestowCurse : SpellDef, ISpellMode, IDurableCapable, ISaveCapable, ICounterDispelCapable
    {
        public override string DisplayName => @"Bestow Curse";
        public override string Description => @"Curse subject for penalties or constraints";
        public override MagicStyle MagicStyle => new Necromancy();

        public override IEnumerable<Descriptor> Descriptors
            => new Curse().ToEnumerable();

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, false, false);

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        #region ISpellMode Members
        public static IEnumerable<OptionAimOption> EffectOptions
        {
            get
            {
                yield return new OptionAimOption() { Key = MnemonicCode.Str, Name = @"-6 STR", Description = @"-6 Penalty to STR (won't lower score below 1)" };
                yield return new OptionAimOption() { Key = MnemonicCode.Dex, Name = @"-6 DEX", Description = @"-6 Penalty to DEX (won't lower score below 1)" };
                yield return new OptionAimOption() { Key = MnemonicCode.Con, Name = @"-6 STR", Description = @"-6 Penalty to STR (won't lower score below 1)" };
                yield return new OptionAimOption() { Key = MnemonicCode.Int, Name = @"-6 INT", Description = @"-6 Penalty to INT (won't lower score below 1)" };
                yield return new OptionAimOption() { Key = MnemonicCode.Wis, Name = @"-6 WIS", Description = @"-6 Penalty to WIS (won't lower score below 1)" };
                yield return new OptionAimOption() { Key = MnemonicCode.Cha, Name = @"-6 CHA", Description = @"-6 Penalty to CHA (won't lower score below 1)" };
                yield return new OptionAimOption() { Key = @"-4", Name = @"-4 ATK, Saves and Checks", Description = @"-4 Penalty to ATK, Saves and Checks" };
                yield return new OptionAimOption() { Key = @"NoAct", Name = @"50% chance to lose action", Description = @"50% chance to act normally or lose action per turn" };
                yield break;
            }
        }

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal, 20, this, new FixedRange(1), new FixedRange(1), new MeleeRange(), new CreatureTargetType());
            yield return new OptionAim(@"Effect", @"Effect", true, FixedRange.One, FixedRange.One, EffectOptions);
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.CopyActivityTargetsToSpellEffects(apply);
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        // ISaveCapable Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
            => new SaveMode(SaveType.Will, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));

        // ICounterDispelCapable
        public IEnumerable<Type> CounterableSpells
            => typeof(RemoveCurse).ToEnumerable();

        public IEnumerable<Type> DescriptorTypes
            => Enumerable.Empty<Type>();

        // IDurableCapable Members
        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if ((source is MagicPowerEffect _power)
                && (target is Creature _critter))
            {
                var _effect = _power.AllTargets.OfType<OptionTarget>()
                    .FirstOrDefault(_t => _t.Key.Equals(@"Effect"))?.Option.Key;
                switch (_effect)
                {
                    case MnemonicCode.Str:
                    case MnemonicCode.Dex:
                    case MnemonicCode.Con:
                    case MnemonicCode.Int:
                    case MnemonicCode.Wis:
                    case MnemonicCode.Cha:
                        // ability curse
                        var _ability = new AbilityCurse(typeof(BestowCurse), _effect, 6, @"Cursed");
                        _critter.AddAdjunct(_ability);
                        return _ability;

                    case @"-4":
                        var _penalty = new Delta(-4, typeof(BestowCurse), @"Cursed");
                        // ATK
                        _critter.MeleeDeltable.Deltas.Add(_penalty);
                        _critter.RangedDeltable.Deltas.Add(_penalty);
                        _critter.OpposedDeltable.Deltas.Add(_penalty);
                        // Saves
                        _critter.FortitudeSave.Deltas.Add(_penalty);
                        _critter.ReflexSave.Deltas.Add(_penalty);
                        _critter.WillSave.Deltas.Add(_penalty);
                        // abilities
                        _critter.ExtraAbilityCheck.Deltas.Add(_penalty);
                        _critter.ExtraSkillCheck.Deltas.Add(_penalty);
                        return _penalty;

                    case @"NoAct":
                        var _unreliable = new UnreliableActionCurse(typeof(BestowCurse));
                        break;
                }
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source?.ActiveAdjunctObject as Adjunct)?.Eject();
            (source?.ActiveAdjunctObject as Delta)?.DoTerminate();
        }

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => @"Save.Will";

        public bool IsDismissable(int subMode)
            => false;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Permanent);

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();
    }

    [Serializable]
    public class AbilityCurse : Adjunct, IMonitorChange<DeltaValue>
    {
        public AbilityCurse(object source, string mnemonicCode, int maxCurse, string name)
            : base(source)
        {
            _Curse = new Delta(0, source, name);
            _Mnemonic = mnemonicCode;
            _Max = maxCurse;
        }

        #region state
        private Delta _Curse;
        private string _Mnemonic;
        private int _Max;
        #endregion

        public string Mnemonic => _Mnemonic;
        public int MaxCurse => _Max;

        public override object Clone()
            => new AbilityCurse(Source, Mnemonic, MaxCurse, _Curse.Name);

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
            {
                var _ability = _critter.Abilities[Mnemonic];
                if (!_ability.IsNonAbility)
                {
                    // add curse
                    _ability.Deltas.Add(_Curse);

                    // dial it to max that would take score to 1
                    var _base = _ability.EffectiveValue;
                    var _dVal = (_base > MaxCurse) ? MaxCurse : _base - 1;

                    // set curse delta
                    _Curse.Value = 0 - _dVal;

                    // and look for changes since we didn't max out
                    if (_dVal < MaxCurse)
                    {
                        _ability.AddChangeMonitor(this);
                    }
                }
            }
        }

        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                var _ability = _critter.Abilities[Mnemonic];
                _ability.RemoveChangeMonitor(this);

                // add curse
                _Curse.DoTerminate();
                _Curse.Value = 0;
            }
            base.OnDeactivate(source);
        }

        // IMonitorChange<DeltaValue>
        public void PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            if (args.OldValue.Value < args.NewValue.Value)
            {
                // effective value went up
                if (Anchor is Creature _critter)
                {
                    var _ability = _critter.Abilities[Mnemonic];
                    if (!_ability.IsNonAbility)
                    {
                        // dial it to max that would take score to 1
                        var _base = _ability.EffectiveValue;
                        var _dVal = (_base > MaxCurse) ? MaxCurse : _base - 1;

                        if (_dVal >= MaxCurse)
                        {
                            // stop looking for changes once we max out
                            _ability.RemoveChangeMonitor(this);
                        }

                        if (_dVal > (0 - _Curse.Value))
                        {
                            // set curse delta if increased
                            _Curse.Value = 0 - _dVal;
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class UnreliableActionCurse : Adjunct, ITrackTime, IActionFilter
    {
        public UnreliableActionCurse(object source)
            : base(source)
        {
        }

        #region state
        private double _Next;
        private bool _Suppressing;
        #endregion

        public override object Clone()
            => new UnreliableActionCurse(Source);

        protected override void OnActivate(object source)
        {
            _Suppressing = true;
            if (Anchor is Creature _critter)
            {
                _critter.Actions.Filters.Add(this, this);
                _Next = (_critter.CurrentTime ?? 0) + Round.UnitFactor;
            }
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as Creature)?.Actions.Filters.Remove(this);
            base.OnDeactivate(source);
        }

        // ITrackTime
        public double Resolution => Round.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (timeVal >= _Next && direction == TimeValTransition.Entering)
            {
                if (Anchor is Creature _critter)
                {
                    _Suppressing = DieRoller.RollDie(_critter.ID, 100, @"Curse", @"50% to be unable to act this round") <= 50;
                    _Next = (_critter.CurrentTime ?? 0) + Round.UnitFactor;
                }
            }
        }

        // IActionFilter
        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
            => _Suppressing;
    }
}
