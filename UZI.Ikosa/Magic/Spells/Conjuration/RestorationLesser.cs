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
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class RestorationLesser : SpellDef, ISpellMode, ISaveCapable
    {
        public override MagicStyle MagicStyle => new Conjuration(Conjuration.SubConjure.Healing);
        public override string DisplayName => @"Lesser Restoration";
        public override string Description => @"Dispel ability penalty, or cure 1d4 ability damage. Remove fatigue. Change exhaustion to fatigue.";

        public override ActionTime ActionTime => new ActionTime(3 * Round.UnitFactor);

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
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

        #region ISpellMode
        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        #region private IEnumerable<OptionAimOption> Abilities()
        private IEnumerable<OptionAimOption> Abilities()
        {
            yield return new OptionAimOption
            {
                Description = @"Ability with most damage",
                Key = @"*",
                Name = @"Worst",
                IsCurrent = true
            };
            foreach (var (_mnemonic, _name) in MnemonicCode.AllAbilities)
            {
                yield return new OptionAimOption
                {
                    Description = _name,
                    Name = _name,
                    Key = _mnemonic
                };
            }
            yield break;
        }
        #endregion

        #region private IEnumerable<OptionAimOption> DispelOrCure()
        private IEnumerable<OptionAimOption> DispelOrCure()
        {
            yield return new OptionAimOption
            {
                Description = @"Prefer Dispel",
                Key = @"Dispel",
                Name = @"Dispel",
                IsCurrent = true
            };
            yield return new OptionAimOption
            {
                Description = @"Prefer Cure",
                Key = @"Cure",
                Name = @"Cure",
                IsCurrent = false
            };
            yield break;
        }
        #endregion

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal,
                ImprovedCriticalTouchFeat.CriticalThreatStart(actor as Creature),
                this, new FixedRange(1), new FixedRange(1), new MeleeRange(), new CreatureTargetType());
            yield return new OptionAim(@"Ability", @"Ability Preference", true, FixedRange.One, FixedRange.One, Abilities());
            yield return new OptionAim(@"Mode", @"Application Preference", true, FixedRange.One, FixedRange.One, DispelOrCure());
            yield break;
        }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverSpell(deliver, 0, deliver.TargetingProcess.Targets[0]);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            var _target = apply.TargetingProcess.Targets.OfType<AttackTarget>().FirstOrDefault();
            if (_target?.Target is Creature _critter)
            {
                // remove fatigue
                foreach (var _fatigued in _critter.Adjuncts.OfType<Fatigued>().ToList())
                {
                    if (_fatigued.Source is MagicPowerEffect _magic)
                    {
                        // any fatigue maintained by a magic power effect
                        _magic.Eject();
                    }
                    else if ((_fatigued.Source is Type _type) && (_type == typeof(Fatigued)))
                    {
                        // self-sourced fatigue
                        _fatigued.Eject();
                    }
                }

                // convert exhausted to fatigued...
                foreach (var _exhausted in _critter.Adjuncts.OfType<Exhausted>()
                    .Where(_e => _e.Source.Equals(typeof(Fatigued))).ToList())
                {
                    if (_exhausted.Eject())
                    {
                        // only if exhausted was removed, otherwise, no point
                        _critter.AddAdjunct(new Fatigued(typeof(Fatigued)));
                    }
                }

                // find magicPowerEffects on creature
                var _mpe = _critter.Adjuncts.OfType<MagicPowerEffect>().ToList();

                // get ability to affect
                AbilityBase _ability = null;
                var _abilityTarget = apply.TargetingProcess.GetFirstTarget<OptionTarget>(@"Ability");
                switch (_abilityTarget?.Key)
                {
                    case MnemonicCode.Str:
                    case MnemonicCode.Dex:
                    case MnemonicCode.Con:
                    case MnemonicCode.Int:
                    case MnemonicCode.Wis:
                    case MnemonicCode.Cha:
                        _ability = _critter.Abilities[_abilityTarget.Key];
                        if ((_ability.Damage == 0)
                            && !_ability.Deltas.Any(_d => (_d.Value < 0) && (_d.TerminateSubscriberCount == 1) && _mpe.Any(_m => _m.ActiveAdjunctObject == _d)))
                        {
                            // ability NOT damaged 
                            // AND ability has no negative delta only affecting itself managed by a magic power effect exclusively
                            _ability = null;
                        }
                        break;
                }

                var _modePref = apply.TargetingProcess.GetFirstTarget<OptionTarget>(@"Mode")?.Key ?? @"Dispel";
                if (_ability == null)
                {
                    // find ability with worst exclusive magic-power-effect sourced negative deltas
                    _ability = (from _a in _critter.Abilities.AllAbilities
                                let _penalties = _a.Deltas.Where(_d => (_d.Value < 0) && (_d.TerminateSubscriberCount == 1) && _mpe.Any(_m => _m.ActiveAdjunctObject == _d))
                                where _penalties.Any()
                                let _min = _penalties.Min(_d => _d.Value)
                                orderby _min
                                select _a).FirstOrDefault();
                    if ((_ability == null) || (_modePref == @"Cure"))
                    {
                        // find ability with most damage
                        _ability = (from _a in _critter.Abilities.AllAbilities
                                    where _a.Damage.Value < 0
                                    orderby _a.Damage.Value
                                    select _a).FirstOrDefault() ?? _ability;
                    }
                }

                if (_ability != null)
                {
                    bool _dispel()
                    {
                        // dispel magics directly controlling penalties for single ability scores
                        var _magics = (from _d in _ability.Deltas
                                       where (_d.Value < 0) && (_d.TerminateSubscriberCount == 1)
                                       let _magic = _mpe.FirstOrDefault(_m => _m.ActiveAdjunctObject == _d)
                                       where _magic != null
                                       select _magic).ToList();
                        if (_magics.Any())
                        {
                            foreach (var _m in _magics)
                            {
                                _m.Eject();
                            }
                            return true;
                        }
                        return false;
                    }

                    if ((_modePref == @"Cure") || !_dispel())
                    {
                        // prefer "Cure", or had nothing to dispel
                        if (_ability.Damage.Value < 0)
                        {
                            new LesserRestorationCure(apply, _ability);
                        }
                        else
                        {
                            // didn't have anything to cure (from a preference...)
                            _dispel();
                        }
                    }
                }
            }
        }
        #endregion

        #region ISaveMode
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }

    [Serializable]
    public class LesserRestorationCure : PreReqListStepBase
    {
        public LesserRestorationCure(PowerApplyStep<SpellSource> apply, AbilityBase ability)
            : base(apply)
        {
            _Apply = apply;
            _Ability = ability;
            _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Roll", @"Cure Ability Damage", new DieRoller(4), true));
        }

        #region data
        private PowerApplyStep<SpellSource> _Apply;
        private AbilityBase _Ability;
        #endregion

        public AbilityBase Ability => _Ability;
        public PowerApplyStep<SpellSource> Apply => _Apply;

        protected override bool OnDoStep()
        {
            if (IsComplete)
                return true;
            var _roll = GetPrerequisite<RollPrerequisite>();
            if (_roll?.IsReady ?? false)
            {
                Ability.RemoveDamage(_roll.RollValue, Apply.PowerUse.PowerActionSource);
            }
            return true;
        }
    }
}
