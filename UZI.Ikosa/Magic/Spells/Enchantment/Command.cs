using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Movement;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Command : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Command";
        public override string Description => @"Issue a command which must be obeyed";
        public override MagicStyle MagicStyle => new Enchantment(Enchantment.SubEnchantment.Compulsion);

        public const string Approach = @"Approach";
        public const string Drop = @"Drop";
        public const string Fall = @"Fall";
        public const string Flee = @"Flee";
        public const string Halt = @"Halt";

        #region public override IEnumerable<Descriptor> Descriptors
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new LanguageDependent();
                yield return new MindAffecting();
                yield return new SoundBased();
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

        #region public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Creature", @"Living Creature", FixedRange.One, FixedRange.One, new NearRange(), new LivingCreatureTargetType());
            if (actor is Creature _critter)
            {
                yield return new OptionAim(@"Language", @"Language", true, FixedRange.One, FixedRange.One, _critter.Languages.ProjectableLanguageOptions);
            }
            else
            {
                yield return new OptionAim(@"Language", @"Language", true, FixedRange.One, FixedRange.One,
                    new OptionAimValue<Language>[]{ new OptionAimValue<Language>()
                        {
                            Value= new Common(this),
                            Key=@"Common",
                            Name=@"Common",
                            Description=@"Common"
                        }
                    });
            }
            yield return new OptionAim(@"Command", @"Command", true, FixedRange.One, FixedRange.One, Commands());
            yield break;
        }
        #endregion

        #region private IEnumerable<OptionAimOption> Commands()
        private IEnumerable<OptionAimOption> Commands()
        {
            yield return new OptionAimOption()
            {
                Name = Approach,
                Key = Approach,
                Description = @"Creature must approach you"
            };
            yield return new OptionAimOption()
            {
                Name = Drop,
                Key = Drop,
                Description = @"Creature must drop what it holds"
            };
            yield return new OptionAimOption()
            {
                Name = Fall,
                Key = Fall,
                Description = @"Creature falls to the ground"
            };
            yield return new OptionAimOption()
            {
                Name = Flee,
                Key = Flee,
                Description = @"Creature must move away from you"
            };
            yield return new OptionAimOption()
            {
                Name = Halt,
                Key = Halt,
                Description = @"Creature may not do anything"
            };
            yield break;
        }
        #endregion

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            if ((deliver.TargetingProcess.Targets[0].Target is Creature _critter)
                && _critter.CreatureType.IsLiving)
            {
                // targetting a living creature
                Language _lang = null;
                if (deliver.TargetingProcess.Targets.Where(_t => _t.Key.Equals(@"Language")).FirstOrDefault() is OptionTarget _langTarg)
                {
                    // make sure target can understand the language
                    _lang = (_langTarg.Option as OptionAimValue<Language>).Value;
                    if (_critter.Languages.CanUnderstandLanguage(_lang.GetType()))
                    {
                        SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
                    }
                }
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.CopyActivityTargetsToSpellEffects(apply);
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        #endregion

        #region IDurableMode Members
        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _source)
            {
                var _effect = new CommandEffect(source as MagicPowerEffect);
                target.AddAdjunct(_effect);
                return _effect;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if (source.ActiveAdjunctObject is CommandEffect _effect)
            {
                target.RemoveAdjunct(_effect);
            }
        }

        public bool IsDismissable(int subMode) => false;
        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode) => @"Save.Will";
        public DurationRule DurationRule(int subMode) => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round()));
        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, 
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }

    /// <summary>Forces the target to make the designated action on its turn</summary>
    [Serializable]
    public class CommandEffect : Adjunct, IActionProvider, IActionFilter, IMonitorChange<CoreActivity>
    {
        #region Construction
        /// <summary>Forces the target to make the designated action on its turn</summary>
        public CommandEffect(MagicPowerEffect source)
            : base(source)
        {
            if (source.AllTargets.Where(_t => _t.Key.Equals(@"Command")).FirstOrDefault() is OptionTarget _option)
            {
                _Cmd = _option.Key;
            }
            else
            {
                _Cmd = Command.Halt;
            }

            _Complete = false;
        }
        #endregion

        #region data
        private string _Cmd;
        private bool _Complete;
        #endregion

        public string IssuedCommand => _Cmd;
        public bool Completed => _Complete;
        public MagicPowerEffect SpellEffect => Source as MagicPowerEffect;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
            {
                _critter.Actions.Providers.Add(this, this);
                _critter.Actions.Filters.Add(this, this);
                _critter.AddChangeMonitor(this);
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                _critter.Actions.Providers.Remove(this);
                _critter.Actions.Filters.Remove(this);
                _critter.RemoveChangeMonitor(this);
            }
            base.OnDeactivate(source);
        }
        #endregion

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            if (_Cmd.Equals(Command.Drop))
            {
                var _critter = Anchor as Creature;
                foreach (var _slot in _critter.Body.ItemSlots.AllSlots
                    .OfType<HoldingSlot>().Where(_is => _is.SlottedItem != null))
                {
                    // go right to the holding slots...
                    yield return new DropHeldObject(_slot, @"100");
                }
            }
            else if (_Cmd.Equals(Command.Fall))
            {
                // drop prone!
                yield return new DropProne(SpellEffect.MagicPowerActionSource, @"101");
            }
            else if (_Cmd.Equals(Command.Halt))
            {
                // full round action that does nothing
                yield return new NoAction(SpellEffect.MagicPowerActionSource, @"102");
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Command", ID);
        #endregion

        #region IActionFilter Members
        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            if (!_Complete)
            {
                if (_Cmd.Equals(Command.Approach) || _Cmd.Equals(Command.Flee))
                {
                    // allow movement only
                    return typeof(StartMove).IsAssignableFrom(action.GetType());
                }
                else
                {
                    // suppress everything (except our actions) until this has completed...
                    return (!action.Source.Equals(this));
                }
            }
            else
            {
                // completed, but which one?
                if (_Cmd.Equals(Command.Drop))
                {
                    // cannot pick anything up if told to drop
                    return typeof(PickUpObject).IsAssignableFrom(action.GetType());
                }
                else if (_Cmd.Equals(Command.Fall))
                {
                    // if we were told to fall, we stay down
                    return typeof(StandUp).IsAssignableFrom(action.GetType());
                }
                return false;
            }
        }
        #endregion

        #region IMonitorChange<CoreActivity> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<CoreActivity> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<CoreActivity> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<CoreActivity> args)
        {
            if ((args.NewValue.Action.Source == this) && args.Action.Equals(@"Stop"))
            {
                if (_Cmd.Equals(Command.Fall))
                {
                    _Complete = true;
                }
                else if (_Cmd.Equals(Command.Drop))
                {
                    // complete once all items are dropped...
                    var _critter = Anchor as Creature;
                    _Complete = (_critter.Body.ItemSlots.AllSlots
                        .Count(_is => _is.SlotType.Equals(ItemSlot.HoldingSlot) && _is.SlottedItem != null) == 0);
                }
            }
        }
        #endregion

        public override object Clone()
            => new CommandEffect(SpellEffect);
    }
}
