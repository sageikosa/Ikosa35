using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class KeenEdge : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Keen Edge";
        public override string Description => @"Increase threat range of piercing and slashing manufactured weapon or ammunition.";
        public override MagicStyle MagicStyle => new Transformation();

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, false, false);

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Weapon", @"Weapon/Ammo to Enhance",
                FixedRange.One, FixedRange.One, new NearRange(), new ObjectTargetType());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            if (apply.DeliveryInteraction.Target is IWeapon _weapon)
            {
                if (!(_weapon is NaturalWeapon))
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

        // IDurableMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // apply the delta to each weapon head, do not apply the enhanced adjunct
            if (source is MagicPowerEffect _spellEffect)
            {
                return new KeenEdgeAdjunct(source as MagicPowerEffect);
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as Adjunct)?.Eject();
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1));

        // ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
            => new SaveMode(SaveType.Will, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
    }

    [Serializable]
    public class KeenEdgeAdjunct : Adjunct, IInteractHandler
    {
        public KeenEdgeAdjunct(MagicPowerEffect effect)
            : base(effect)
        {
            _Keen = new ConstDelta(1, typeof(Deltas.CriticalRange), @"Keen Edge");
        }

        #region state
        private readonly ConstDelta _Keen;
        #endregion

        public MagicPowerEffect MagicPowerEffect => Source as MagicPowerEffect;
        public ConstDelta Keen => _Keen;

        public override object Clone()
            => new KeenEdgeAdjunct(MagicPowerEffect);

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            if (Anchor is IAmmunitionBase _ammoBase)
            {
                _ammoBase.AddIInteractHandler(this);
                _ammoBase.CriticalRangeFactor.Deltas.Add(_Keen);
            }
            else if (Anchor is IMeleeWeapon _melee)
            {
                foreach (var _head in _melee.AllHeads)
                {
                    _head.CriticalRangeFactor.Deltas.Add(_Keen);
                }
            }
            else if (Anchor is IProjectileWeapon _project)
            {
                _project.CriticalRangeFactor.Deltas.Add(_Keen);
            }
            base.OnActivate(source);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            if (Anchor is IAmmunitionBase _ammoBase)
            {
                _ammoBase.RemoveIInteractHandler(this);
            }
            _Keen.DoTerminate();
            base.OnDeactivate(source);
        }
        #endregion

        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet?.InteractData is Drop _dropData)
                && (workSet?.Target is IAmmunitionBase _ammo)
                && (_ammo == Anchor))
            {
                // not dropped gently is equivalent to using it (more or less)
                if (!_dropData.DropGently)
                {
                    // this should take care of this adjunct also...
                    MagicPowerEffect.Eject();
                }
            }
        }
        #endregion

        public IEnumerable<Type> GetInteractionTypes()
            => typeof(Drop).ToEnumerable();

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // pre-empt DropHandlers
            return (existingHandler is DropHandler) || (existingHandler is AmmoDropHandler);
        }
    }
}
