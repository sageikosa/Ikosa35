using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class AlignWeapon : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Align Weapon";
        public override string Description => @"Aligns a weapon to overcome damage reduction.";
        public override MagicStyle MagicStyle => new Transformation();

        public override IPowerDef ForPowerSource()
            => new AlignWeapon();

        private Alignment _Align = null;

        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                if (_Align != null)
                {
                    if (_Align.Ethicality == GoodEvilAxis.Good)
                        yield return new Good();
                    if (_Align.Ethicality == GoodEvilAxis.Evil)
                        yield return new Evil();
                    if (_Align.Orderliness == LawChaosAxis.Lawful)
                        yield return new Lawful();
                    if (_Align.Orderliness == LawChaosAxis.Chaotic)
                        yield return new Chaotic();
                }
                yield break;
            }
        }

        #region public override IEnumerable<SpellComponent> DivineComponents { get; }
        public override IEnumerable<SpellComponent> DivineComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new DivineFocusComponent();
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

        public IEnumerable<OptionAimOption> Options
        {
            get
            {
                yield return new OptionAimValue<Alignment>
                {
                    Key = @"Good",
                    Description = @"Good",
                    Name = @"Good",
                    Value = new Alignment(LawChaosAxis.Neutral, GoodEvilAxis.Good)
                };
                yield return new OptionAimValue<Alignment>
                {
                    Key = @"Evil",
                    Description = @"Evil",
                    Name = @"Evil",
                    Value = new Alignment(LawChaosAxis.Neutral, GoodEvilAxis.Evil)
                };
                yield return new OptionAimValue<Alignment>
                {
                    Key = @"Lawful",
                    Description = @"Law",
                    Name = @"Law",
                    Value = new Alignment(LawChaosAxis.Lawful, GoodEvilAxis.Neutral)
                };
                yield return new OptionAimValue<Alignment>
                {
                    Key = @"Chaotic",
                    Description = @"Chaos",
                    Name = @"Chaos",
                    Value = new Alignment(LawChaosAxis.Chaotic, GoodEvilAxis.Neutral)
                };
                yield break;
            }
        }

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Weapon", @"Weapon/Ammo to Align", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new ObjectTargetType());
            yield return new OptionAim(@"Alignment", @"Alignment", true, FixedRange.One, FixedRange.One, Options);
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Weapon", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            _Align = (apply.TargetingProcess.GetFirstTarget<OptionTarget>(@"Alignment").Option as OptionAimValue<Alignment>)?.Value;
            CopyActivityTargetsToSpellEffects(apply);

            if (apply.DeliveryInteraction.Target is IWeapon _weapon)
            {
                if (!(_weapon is NaturalWeapon _natrl))
                {
                    SpellDef.ApplyDurableMagicEffects(apply);
                }
            }
            else if ((apply.DeliveryInteraction.Target is IAmmunitionBundle _bundle)
                && (_bundle.Count <= 50))
            {
                SpellDef.ApplySpellEffectAmmunitionBundle(apply);
            }
        }
        #endregion

        #region IDurableMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // apply the delta to each weapon head, do not apply the enhanced adjunct
            if (source is MagicPowerEffect _spellEffect)
            {
                var _alignment = ((_spellEffect.FirstTarget(@"Alignment") as OptionTarget).Option) as OptionAimValue<Alignment>;
                var _list = new List<AlignedItem>();
                AlignedItem _getAligned()
                {
                    var _aligned = new AlignedItem(
                        _spellEffect,
                        _alignment?.Value ?? new Alignment(LawChaosAxis.Neutral, GoodEvilAxis.Good),
                        _spellEffect.MagicPowerActionSource.CasterLevel);
                    _list.Add(_aligned);
                    return _aligned;
                }
                if (target is IMeleeWeapon _melee)
                {
                    foreach (var _head in _melee.AllHeads)
                    {
                        _head.AddAdjunct(_getAligned());
                    }
                }
                else if (target is IProjectileWeapon _project)
                {
                    _project.VirtualHead.AddAdjunct(_getAligned());
                }
                else if (target is IAmmunitionBase _ammoBase)
                {
                    _ammoBase.AddAdjunct(_getAligned());
                }
                return _list;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as List<AlignedItem>)?.ForEach(_ai => _ai.Eject());
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }

        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }
}
