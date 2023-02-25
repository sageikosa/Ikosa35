using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class HoldShut : SpellDef, ISpellMode, IDurableCapable, IRegionCapable
    {
        public override string DisplayName => @"Hold Shut";
        public override string Description => @"Holds a portal shut.";
        public override MagicStyle MagicStyle => new Abjuration();
        public override IEnumerable<SpellComponent> ArcaneComponents => new VerbalComponent().ToEnumerable();
        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        #region ISpellMode Members

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Portal", @"Portal", FixedRange.One, FixedRange.One, new MediumRange(), new ObjectTargetType());
            yield break;
        }

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // must be a portal
            if ((apply.DeliveryInteraction.Target is PortalBase _portal) && _portal.OpenState.IsClosed)
            {
                // size limits...
                var _rMode = apply.PowerUse.CapabilityRoot.GetCapability<IRegionCapable>();
                var _maxArea = _rMode.Dimensions(apply.Actor, apply.PowerUse.PowerActionSource.CasterLevel).First();
                if (_maxArea >= _portal.Area)
                {
                    // apply the hold shut effect
                    SpellDef.ApplyDurableMagicEffects(apply);
                }
            }
        }

        #endregion

        #region IDurableMode Members

        public bool IsDismissable(int subMode) => true;
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();
        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode) => string.Empty;

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _hsfo = new HoldShutForceOpen(target);
                var _block = new HoldShutAdjunct(_spellEffect.MagicPowerActionSource, _hsfo);
                _hsfo.Target = _block.ID;
                target.AddAdjunct(_block);
                return _block;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as Adjunct)?.Eject();
        }

        public DurationRule DurationRule(int subMode) { return new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1)); }
        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        #endregion

        #region IRegionMode Members
        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
        {
            yield return Convert.ToDouble(20 * casterLevel);
            yield break;
        }
        #endregion
    }

    /// <summary>Not only does it block the portal, but it makes it harder to force open.</summary>
    [Serializable]
    public class HoldShutAdjunct : OpenBlocked
    {
        /// <summary>Not only does it block the portal, but it makes it harder to force open.</summary>
        public HoldShutAdjunct(IMagicPowerActionSource source, HoldShutForceOpen forceOpen)
            : base(source, forceOpen, true)
        {
            _Hold = new Delta(5, typeof(HoldShut));
        }

        private Delta _Hold;

        protected override void OnActivate(object source)
        {
            (Anchor as PortalBase)?.ForceOpenDifficulty.Deltas.Add(_Hold);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            _Hold.DoTerminate();
            base.OnDeactivate(source);
        }
    }

    [Serializable]
    public class HoldShutForceOpen : IForceOpenTarget
    {
        public HoldShutForceOpen(IAdjunctable adjunctable)
        {
            _Adjunctable = adjunctable;
        }

        #region data
        private IAdjunctable _Adjunctable;
        private Guid _Target;
        #endregion

        public IAdjunctable Adjunctable => _Adjunctable;
        public Guid Target { get => _Target; set => _Target = value; }

        public Guid ID
            => Guid.Empty;

        public void DoForcedOpen()
        {
            // look for relavant objects in adjunctable
            var _hold = Adjunctable.Adjuncts.OfType<HoldShutAdjunct>().FirstOrDefault(_hs => _hs.ID == Target);
            var _effect = Adjunctable.Adjuncts.OfType<MagicPowerEffect>()
                .FirstOrDefault(_mpe => _mpe.ActiveAdjunctObject == _hold);
            if (_effect != null)
            {
                // eject effect
                _effect.Eject();
            }
            else
            {
                // eject hold
                _hold.Eject();
            }
        }
    }
}
