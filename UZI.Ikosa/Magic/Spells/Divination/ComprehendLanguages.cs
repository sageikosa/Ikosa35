using System;
using System.Collections.Generic;
using Uzi.Ikosa.Languages;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class ComprehendLanguages : SpellDef, ISpellMode, IDurableCapable
    {
        public override string DisplayName { get { return @"Comprehend Languages"; } }
        public override string Description { get { return @"Understand all spoken words and written writing."; } }
        public override MagicStyle MagicStyle { get { return new Divination(Divination.SubDivination.Illumination); } }

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
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new PersonalAim(@"Self", actor);
            yield break;
        }

        public bool AllowsSpellResistance { get { return false; } }
        public bool IsHarmless { get { return true; } }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            OmniLanguage _omni = new OmniLanguage(source);
            Creature _critter = target as Creature;
            if (_critter != null)
            {
                _critter.Languages.Add(_omni);
            }
            return _omni;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            OmniLanguage _omni = ((DurableMagicEffect)source).ActiveAdjunctObject as OmniLanguage;
            Creature _critter = target as Creature;
            if ((_omni!= null) && (_critter != null))
            {
                _critter.Languages.Remove(_omni);
            }
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
        {
            return false;
        }

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
        {
            return string.Empty;
        }

        public DurationRule DurationRule(int subMode)
        {
            return new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1));
        }
        #endregion
    }

    [Serializable]
    public class OmniLanguage : Language
    {
        public OmniLanguage(object source)
            : base(source)
        {
        }
        public override string Alphabet { get { return string.Empty; } }
        public override bool CanProject { get { return false; } }
        public override bool IsAlphabetCompatible(string alpha) { return true; }
        public override bool IsCompatible(Type type) { return true; }
        public override Language GetCopy(object source) => new OmniLanguage(source);
    }
}
