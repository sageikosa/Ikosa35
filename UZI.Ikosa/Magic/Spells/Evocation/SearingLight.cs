using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class SearingLight : SpellDef, ISpellMode, IDamageCapable, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Searing Light";
        public override string Description => @"Ray Damage: 1d8/2-levels; 1d6/level (undead); 1d8/level (light vulnerable undead); 1d6/2-levels objects and constructs";
        public override MagicStyle MagicStyle => new Evocation();

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, false, false);

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        // ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new TouchAim(@"Target", @"Target", Lethality.AlwaysLethal,
                ImprovedCriticalRayFeat.CriticalThreatStart(actor as Creature),
                this, new FixedRange(1), new FixedRange(1), new MediumRange(),
                new TargetType[] { new ObjectTargetType(), new CreatureTargetType() })
            .ToEnumerable();

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        #region protected int GetDamageMode(IInteract target)
        protected int GetDamageMode(IInteract target)
        {
            if (target is IItemBase || target is IObjectBase)
            {
                // object or item
                return 3;
            }

            if (target is Creature _critter)
            {
                if (!_critter.CreatureType.IsLiving)
                {
                    if (_critter.CreatureType is UndeadType)
                    {
                        if (_critter.HasActiveAdjunct<LightVulnerability>())
                        {
                            // light-vulnerable undead
                            return 2;
                        }

                        // unead
                        return 1;
                    }

                    // construct
                    return 3;
                }
            }

            // living creature
            return 0;
        }
        #endregion

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            var _target = deliver.TargetingProcess.Targets[0].Target;
            SpellDef.DeliverDamageToTouch(deliver, GetDamageMode(_target));
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDamage(apply, apply, GetDamageMode(apply.DeliveryInteraction.Target));
        }

        // IDamageCapable Members
        public IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            var _max = subMode switch
            {
                1 => 10,    // undead                       (max: 10d6)
                2 => 10,    // light-vulnerable undead      (max: 10d8)
                _ => 5      // living | construct/object    (max: 5d8 | 5d6)
            };
            byte _die = subMode switch
            {
                1 => 6,     // undead | light-vulnerable        (1d6 per level)
                3 => 6,     // object/item/construct            (1d6 per 2 levels)
                _ => 8      // living | light-vulnerable undead (1d8 per 2-levels | 1d8 per level)
            };
            var _step = subMode switch
            {
                1 => 1,     // undead                       (1d6 per level)
                2 => 1,     // light-vulnerable undead      (1d8 per level)
                _ => 2      // living | construct/object    (1d8 per 2-levels | 1d6 per 2-levels)
            };

            // 1d8 per 2-levels (max 5d8)
            yield return new DamageRule(@"SearingLight.Damage",
                new DiceRange(@"SearingLight", DisplayName, _max, new DieRoller(_die), _step), false, @"Searing Light Damage");
            if (isCriticalHit)
                yield return new DamageRule(@"SearingLight.Damage.Critical",
                    new DiceRange(@"SearingLight (Critical)", DisplayName, _max, new DieRoller(_die), _step), false, @"Searing Light Damage (Critical)");
            yield break;
        }

        public IEnumerable<int> DamageSubModes
            => new int[] { 0, 1, 2, 3 };

        public string DamageSaveKey(Interaction workSet, int subMode)
            => string.Empty;

        public bool CriticalFailDamagesItems(int subMode) => false;

        #region IPowerDeliverVisualize Members

        public VisualizeTransferType GetTransferType() => VisualizeTransferType.Beam;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Medium;
        public string GetTransferMaterialKey() => @"#C0FFFF00";
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Pulse;
        public string GetSplashMaterialKey() => @"#80B0B000|#C0FFFF00|#80B0B000";

        #endregion
    }
}
