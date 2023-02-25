using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class SpiritualWeapon : SpellDef, ISpellMode, IDurableCapable, IDurableAnchorCapable, IDamageCapable
    {
        public override string DisplayName => @"Spiritual weapon";
        public override string Description => @"Force weapon attacks opponents at a distance as directed";
        public override MagicStyle MagicStyle => new Evocation();

        public override IEnumerable<Descriptor> Descriptors => (new Force()).ToEnumerable();
        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

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

        // ISpellMode
        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new AwarenessAim(@"Creature", @"Creature", FixedRange.One, FixedRange.One, new MediumRange(), new CreatureTargetType())
            .ToEnumerable();

        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            // deliver to creature as target
            SpellDef.DeliverDurable(activation, activation.TargetingProcess.GetFirstTarget<AwarenessTarget>(@"Creature"), 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // favor material plane
            var _tProc = apply.TargetingProcess;
            var _target = _tProc.GetFirstTarget<AwarenessTarget>(@"Creature")?.Target as Creature;
            var _tLoc = _target?.GetLocated()?.Locator;
            var _aLoc = apply.Actor?.GetLocated()?.Locator;
            if ((_aLoc != null) && (_tLoc != null))
            {
                var _mapContext = _aLoc.MapContext;
                var _ethereal = _aLoc.PlanarPresence.HasEtherealPresence();

                // find closest cell in target locator
                var _location = _tLoc.GeometricRegion.AllCellLocations()
                    .OrderBy(_cl => _aLoc.GeometricRegion.NearDistanceToCell(_cl))
                    .FirstOrDefault();

                // get delivered effect
                var _effect = apply.DurableMagicEffects.FirstOrDefault();
                var _powerSource = apply.PowerUse.PowerActionSource;

                // submode == 0 indicates initial setup
                if (_effect.SubMode == 0)
                {
                    // virtual object
                    var _evocObj = MagicEffectHolder.CreateMagicEffectHolder(@"Spiritual Weapon", Size.Tiny, GeometricSize.UnitSize(),
                        @"spiritual_weapon", @"spiritual_weapon", _ethereal, _location, _mapContext, true);
                    // TODO: add interaction handlers (observe, visual, magic targeting)

                    // group
                    var _swGroup = new SpiritualWeaponGroup(_powerSource);
                    _effect.AllTargets.Add(new ValueTarget<SpiritualWeaponGroup>(nameof(SpiritualWeaponGroup), _swGroup));
                    apply.Actor.AddAdjunct(new SpiritualWeaponController(_powerSource, _swGroup));
                    _target.AddAdjunct(new SpiritualWeaponTarget(_powerSource, _swGroup));

                    // magic power effect on evoc holder
                    _evocObj.AddAdjunct(_effect);
                }
                else
                {
                    // NOTE: transit was taken care of in the action to redirect, this is the apply that come afterwards
                    var _swGroup = _effect.GetTargetValue<SpiritualWeaponGroup>(nameof(SpiritualWeaponGroup));
                    _target.AddAdjunct(new SpiritualWeaponTarget(_powerSource, _swGroup));
                }
            }
        }

        // IDurableCapable: durable for the physical presence and the connection to the caster
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();
        public bool IsDismissable(int subMode) => true;

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // enable if possible
            (source.AnchoredAdjunctObject as SpiritualWeaponEvocation)?.ActivateAdjunct();
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            // disable if possible
            (source.AnchoredAdjunctObject as SpiritualWeaponEvocation)?.DeActivateAdjunct();
        }

        // IDurableAnchorCapable
        public object OnAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _swGroup = _spellEffect.GetTargetValue<SpiritualWeaponGroup>(nameof(SpiritualWeaponGroup));
                if (_swGroup != null)
                {
                    // create spiritual weapon master
                    var _swMaster = new SpiritualWeaponEvocation(_spellEffect, _swGroup)
                    { InitialActive = false };
                    target.AddAdjunct(_swMaster);
                    return _swMaster;
                }
            }
            return null;
        }

        public void OnEndAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            // when the durable spell effect is removed from the virtual object, the object is removed from context
            (target as MagicEffectHolder)?.Destroy();
        }

        // IDamageMode
        public IEnumerable<int> DamageSubModes => 0.ToEnumerable();
        public bool CriticalFailDamagesItems(int subMode) => false;

        public string DamageSaveKey(Interaction workSet, int subMode)
            => string.Empty;

        public IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            var _dice = new DiceRange(@"Force", @"Spiritual Weapon", new DieRoller(8), 5, new ConstantRoller(1), 3);
            yield return new EnergyDamageRule(
                $@"Force.Damage{(subMode > 0 ? $".{subMode}" : string.Empty)}", _dice,
                $@"Spiritual Weapon{(subMode > 0 ? $" Critical {subMode}" : string.Empty)}", EnergyType.Force);
            yield break;
        }
    }
}
