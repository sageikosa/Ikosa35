using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Consecrate : SpellDef, ISpellMode, IDurableCapable, IRegionCapable, IGeometryCapable<SpellSource>
    {
        public override string DisplayName => @"Consecrate";
        public override string Description => @"Bless area with positive energy.  More driving undead power.  Undead penalties and restrictions.";
        public override MagicStyle MagicStyle => new Evocation();
        public override IEnumerable<Descriptor> Descriptors => new Good().ToEnumerable();

        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();
        // TODO: general mode, target object mode (shrine to own devotion or other)

        #region public override IEnumerable<SpellComponent> DivineComponents { get; }
        public override IEnumerable<SpellComponent> DivineComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new MaterialComponent();
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
                yield return new FocusComponent();
                yield break;
            }
        }
        #endregion

        // IDurableMode
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();
        public bool IsDismissable(int subMode) => false;
        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode) => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(2, new Hour(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            //var _bless = new BlessEffect(source);
            //target.AddAdjunct(_bless);
            //return _bless;
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            //=> (source.ActiveAdjunctObject as BlessEffect)?.Eject();
        }

        // ISpellMode
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new LocationAim(@"Origin", @"Emanation Origin", LocationAimMode.Any, FixedRange.One, FixedRange.One, new NearRange())
            .ToEnumerable();

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        #region public void ActivateSpell(PowerDeliveryStep<SpellSource> activation)
        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            //// get burst geometry
            //var _target = deliver.Activity.GetFirstTarget<LocationTarget>(@"Self");
            //var _sphere = new Geometry(GetDeliveryGeometry(deliver), new Intersection(_target.Location), true);
            //SpellDef.DeliverBurstToMultipleSteps(deliver, new Intersection(_target.Location), _sphere, null);
        }
        #endregion

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        #region IRegionMode Members
        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
        {
            yield return 20;
            yield break;
        }
        #endregion

        #region IGeometryMode Members

        public IGeometryBuilder GetBuilder(IPowerUse<SpellSource> powerUse, CoreActor actor)
        {
            // get burst geometry
            var _reg = powerUse.CapabilityRoot.GetCapability<IRegionCapable>();
            return new SphereBuilder(Convert.ToInt32(_reg.Dimensions(actor,
                powerUse.PowerActionSource.CasterLevel).FirstOrDefault() / 5));
        }

        #endregion
    }
}
