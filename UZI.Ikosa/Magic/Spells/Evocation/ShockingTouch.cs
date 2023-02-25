using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Core.Dice;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class ShockingTouch : SpellDef, ISpellMode, IDamageCapable, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Shocking Touch";
        public override string Description => @"Touch attack deals 1d6 Electricity per level (max: 5d6)";
        public override MagicStyle MagicStyle => new Evocation();

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Electricity();
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
            yield return new TouchAim(@"Target", @"Target", Lethality.AlwaysLethal,
                ImprovedCriticalTouchFeat.CriticalThreatStart(actor as Creature),
                this, FixedRange.One, FixedRange.One, new MeleeRange(),
                new ITargetType[] { new CreatureTargetType(), new ObjectTargetType() });
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        private delegate bool MaterialCheck(object obj);

        #region public void Deliver(PowerDeliveryStep<SpellSource> deliver)
        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            var _atkBonus = 0;
            bool _check(object _obj) => _obj is MetalMaterial;
            Type _metalType = typeof(MetalMaterial);

            // +3 attack against metal creatures or metal armor
            if (deliver.TargetingProcess.Targets[0].Target is Creature _critter)
            {
                // is creature made of metal?
                if (_check(_critter.Body.BodyMaterial))
                    _atkBonus = 3;
                else
                {
                    ItemSlot _armorSlot = _critter.Body.ItemSlots[ItemSlot.ArmorRobeSlot];
                    if ((_armorSlot != null) && (_armorSlot.SlottedItem != null))
                    {
                        // is armor made of metal
                        if ((_armorSlot.SlottedItem is SlottedItemBase _slotItem) && _check(_slotItem.ItemMaterial))
                            _atkBonus = 3;
                    }
                    // TODO: consider checking for "a lot of metal" in the ObjectLoad or Parts enumerator
                }
            }
            else
            {
                if (deliver.TargetingProcess.Targets[0].Target is ItemBase _item)
                {
                    // is item made of metal?
                    if (_check(_item.ItemMaterial))
                        _atkBonus = 3;
                }
                else
                {
                    if (deliver.TargetingProcess.Targets[0].Target is ObjectBase _obj)
                    {
                        // is object made of metal?
                        if (_check(_obj.ObjectMaterial))
                            _atkBonus = 3;
                    }
                }
            }

            // apply bonus prior to attack
            if (_atkBonus > 0)
            {
                var _atkDelta = new Delta(_atkBonus, typeof(Deltas.Circumstance<MetalMaterial>), @"Metal Target");
                if (deliver.TargetingProcess.Targets[0] is AttackTarget _target)
                {
                    // both attack and critical rolls get a boost
                    _target.Attack.AttackScore.Deltas.Add(_atkDelta);
                    if (_target.Attack.CriticalConfirmation != null)
                        _target.Attack.CriticalConfirmation.Deltas.Add(_atkDelta);
                }
            }

            // deliver
            SpellDef.DeliverDamageToTouch(deliver, 0);
        }
        #endregion

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDamage(apply, apply, 0);
        }
        #endregion

        #region IDamageMode Members

        #region public IEnumerable<int> SubModes { get; }
        public IEnumerable<int> DamageSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }
        #endregion

        public IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            yield return new EnergyDamageRule(@"Electricity.Damage", new DiceRange(@"Electric", DisplayName, 5, new DieRoller(6), 1), @"Shocking Grasp", EnergyType.Electric);
            if (isCriticalHit)
                yield return new EnergyDamageRule(@"Electricity.Damage.Critical", new DiceRange(@"Electric (Critical)", DisplayName, 5, new DieRoller(6), 1), @"Shocking Grasp (Critical)", EnergyType.Electric);
            yield break;
        }

        public string DamageSaveKey(Interaction workSet, int subMode)
            => string.Empty;

        public bool CriticalFailDamagesItems(int subMode) => false;

        #endregion

        #region IPowerDeliverVisualize Members

        public VisualizeTransferType GetTransferType() => VisualizeTransferType.None;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Medium;
        public string GetTransferMaterialKey() => string.Empty;
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Pulse;
        public string GetSplashMaterialKey() => @"#C000FF40|#8020FF80|#C000FF40";

        #endregion
    }
}
