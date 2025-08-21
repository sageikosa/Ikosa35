using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Prayer : SpellDef, ISpellMode, IDurableCapable, IRegionCapable, IBurstCaptureCapable,
        IGeometryCapable<SpellSource>, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Prayer";
        public override string Description => @"+1 Luck on attack, damage, saves and skill checks for friends.  -1 penalty for enemies.";
        public override MagicStyle MagicStyle => new Enchantment(Enchantment.SubEnchantment.Compulsion);
        public override IEnumerable<Descriptor> Descriptors => new MindAffecting().ToEnumerable();
        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        public override IEnumerable<SpellComponent> DivineComponents
            => YieldComponents(true, true, false, false, true);

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, true, false);

        // ISpellMode
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new PersonalStartAim(@"Self", @"Self", actor).ToEnumerable();

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        #region public void ActivateSpell(PowerDeliveryStep<SpellSource> activation)
        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            // get burst geometry
            var _target = activation.TargetingProcess.GetFirstTarget<LocationTarget>(@"Self");
            var _sphere = new Geometry(GetDeliveryGeometry(activation), new Intersection(_target.Location), true);
            SpellDef.DeliverBurstToMultipleSteps(activation, new Intersection(_target.Location), _sphere, null);
        }
        #endregion

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        // IRegionCapable Members
        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
            => 40d.ToEnumerable();

        // IGeometryCapable Members
        public IGeometryBuilder GetBuilder(IPowerUse<SpellSource> powerUse, CoreActor actor)
        {
            // get burst geometry
            var _reg = powerUse.CapabilityRoot.GetCapability<IRegionCapable>();
            return new SphereBuilder(Convert.ToInt32(_reg.Dimensions(actor,
                powerUse.PowerActionSource.CasterLevel).FirstOrDefault() / 5));
        }

        // IBurstCaptureCapable members
        public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        {
            // get the burst
            if (burst is PowerBurstCapture<SpellSource> _spellBurst)
            {
                var _actor = _spellBurst.Activation.Actor as Creature;

                // friends directly on the locator (for now)
                foreach (var _step in SpellDef.DeliverDurableDirectFromBurst(_spellBurst, locator,
                    (Locator loc, ICore core) => (core is Creature) && _actor.IsFriendly(core.ID),
                    (CoreStep step) => true, 0))
                {
                    yield return _step;
                }

                // enemies directly on the locator (for now)
                foreach (var _step in SpellDef.DeliverDurableDirectFromBurst(_spellBurst, locator,
                    (Locator loc, ICore core) => (core is Creature) && _actor.IsUnfriendly(core.ID),
                    (CoreStep step) => true, 1))
                {
                    yield return _step;
                }
            }
            yield break;
        }

        public void PostInitialize(BurstCapture burst)
        {
            return;
        }

        public IEnumerable<Locator> ProcessOrder(BurstCapture burst, IEnumerable<Locator> selection)
            => selection;

        // IDurableMode
        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        #region public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (subMode == 0)
            {
                // submode 0 is for friends
                var _luck = new Delta(1, typeof(Luck), @"Prayer");
                if (target is Creature _critter)
                {
                    // attack
                    _critter.MeleeDeltable.Deltas.Add(_luck);
                    _critter.RangedDeltable.Deltas.Add(_luck);
                    _critter.OpposedDeltable.Deltas.Add(_luck);

                    // damage
                    _critter.ExtraWeaponDamage.Deltas.Add(_luck);

                    // saves
                    _critter.FortitudeSave.Deltas.Add(_luck);
                    _critter.ReflexSave.Deltas.Add(_luck);
                    _critter.WillSave.Deltas.Add(_luck);

                    // skills
                    _critter.ExtraSkillCheck.Deltas.Add(_luck);
                }
                return _luck;
            }

            // submode 1 is for enemies
            var _penalty = new Delta(-1, typeof(Prayer), @"Prayer");
            if (target is Creature _victim)
            {
                // attack
                _victim.MeleeDeltable.Deltas.Add(_penalty);
                _victim.RangedDeltable.Deltas.Add(_penalty);
                _victim.OpposedDeltable.Deltas.Add(_penalty);

                // damage
                _victim.ExtraWeaponDamage.Deltas.Add(_penalty);

                // saves
                _victim.FortitudeSave.Deltas.Add(_penalty);
                _victim.ReflexSave.Deltas.Add(_penalty);
                _victim.WillSave.Deltas.Add(_penalty);

                // skills
                _victim.ExtraSkillCheck.Deltas.Add(_penalty);
            }
            return _penalty;
        }
        #endregion

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
            => (source.ActiveAdjunctObject as Delta)?.DoTerminate();

        #region IPowerDeliverVisualize

        public VisualizeTransferType GetTransferType() => VisualizeTransferType.FullSurge;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Large;
        public string GetTransferMaterialKey() => @"#C0FFFFB0";
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Pulse;
        public string GetSplashMaterialKey() => @"#80FFFFB0|#D0FFFFB0|#80FFFFB0";

        #endregion
    }
}
