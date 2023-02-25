using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Items.Alchemal;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    /// <summary>
    /// Base class for Sanctify Water, Desecrate Water, Orchestrate Water and Agitate Water
    /// </summary>
    [Serializable]
    public abstract class AlignWater : SpellDef, ISpellMode
    {
        public override ActionTime ActionTime => new ActionTime(Minute.UnitFactor);
        public override MagicStyle MagicStyle => new Transformation();

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new CostlyMaterialComponent(typeof(CostlyComponent<AlignWater>), 25);
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
            // NOTE: consider vessel of water as target
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        protected abstract AlignedWater OnApply();

        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            var _loc = Locator.FindFirstLocator(activation.Actor);
            var _cube = new GeometricRegionTarget(@"Cube", _loc.GeometricRegion, _loc.MapContext);
            var _delivery = SpellDef.InteractSpellTransitToRegion(activation, _cube);
            new PowerApplyStep<SpellSource>(activation, activation.PowerUse, activation.Actor, null, _delivery, false, false);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            var _water = OnApply();

            if (apply.Actor is Creature _critter)
            {
                // possess it
                _water.Possessor = _critter;
                if (_critter.ObjectLoad.CanAdd(_water))
                {
                    // create holder
                    var _item = new HoldingWrapper(_critter, _water);

                    // get item slot
                    var _slot = _critter.Body.ItemSlots[ItemSlot.HoldingSlot, true];
                    if (_slot != null)
                    {
                        // slot the item
                        _item.SetItemSlot(_slot);

                        // slotted!
                        if (_item.MainSlot == _slot)
                        {
                            return;
                        }

                    }
                }

                // just drop it in the environment
                Drop.DoDrop(_critter, _water, this, true);
            }
        }

        #endregion
    }

    [Serializable]
    public class SanctifyWater : AlignWater
    {
        protected override AlignedWater OnApply() => new AlignedWater(@"Holy Water", Alignment.NeutralGood);
        public override string DisplayName => @"Sanctify Water";
        public override string Description => @"Create Holy Water";
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Good();
                yield break;
            }
        }
    }

    [Serializable]
    public class DescrateWater : AlignWater
    {
        protected override AlignedWater OnApply() => new AlignedWater(@"Unholy Water", Alignment.NeutralEvil);
        public override string DisplayName => @"Descrate Water";
        public override string Description => @"Create Unholy Water";
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Evil();
                yield break;
            }
        }
    }

    [Serializable]
    public class OrchestrateWater : AlignWater
    {
        protected override AlignedWater OnApply() => new AlignedWater(@"Lawful Water", Alignment.LawfulNeutral);
        public override string DisplayName => @"Orchestrate Water";
        public override string Description => @"Create Axiomatic Water";
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Lawful();
                yield break;
            }
        }
    }

    [Serializable]
    public class AgitateWater : AlignWater
    {
        protected override AlignedWater OnApply() => new AlignedWater(@"Chaotic Water", Alignment.ChaoticNeutral);
        public override string DisplayName => @"Agitate Water";
        public override string Description => @"Create Anarchic Water";
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Chaotic();
                yield break;
            }
        }
    }
}
