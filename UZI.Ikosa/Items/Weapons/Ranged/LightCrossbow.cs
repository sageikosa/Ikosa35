using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Piercing, 19, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Crossbow, Light", @"Simple Ranged 1d8 (19-20/x2) Piercing", @"light_xbow")
    ]
    public class LightCrossbow : CrossbowBase, IQualifyDelta, IWieldMountable
    {
        public LightCrossbow()
            : base(@"Crossbow, Light", 80, Size.Small)
        {
            _Term = new TerminateController(this);
            _OneHand = new QualifyingDelta(-2, typeof(LightCrossbow), @"One Hand Firing");
            Setup();
        }

        /// <summary>-2 penalty for firing with one hand</summary>
        private IDelta _OneHand;

        private readonly TerminateController _Term;
        protected override IWeaponHead GetProxyHead() { return GetWeaponHead<LightCrossbow>(true); }

        private void Setup()
        {
            ItemMaterial = Materials.WoodMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Simple;
            Price.CorePrice = 35m;
            MaxStructurePoints.BaseValue = 5;
            BaseWeight = 4d;
        }

        public override TimeType ReloadAction
            => (CreaturePossessor?.Feats.Contains(typeof(Feats.RapidReloadFeat<LightCrossbow>)) ?? false)
            ? TimeType.Free
            : TimeType.Brief;

        protected override void OnSetItemSlot()
        {
            CreaturePossessor.RangedDeltable.Deltas.Add((IQualifyDelta)this);
            base.OnSetItemSlot();
        }

        protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        {
            CreaturePossessor.RangedDeltable.Deltas.Remove((IQualifyDelta)this);
            base.OnClearSlots(slotA, slotB);
        }

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // only relevant if this is being used by one hand
            if (SecondarySlot == null)
            {
                if (qualify is Interaction _iAct)
                {
                    if (_iAct.InteractData is RangedAttackData)
                    {
                        if (qualify.Source is RangedAmmunition _rHead)
                        {
                            // must be a ranged attack interaction with ranged ammunition from this projector
                            if (_rHead.Projector == this)
                            {
                                yield return _OneHand;
                            }
                        }
                    }
                }
            }
            yield break;
        }

        #endregion

        #region IControlTerminate Members

        public void DoTerminate()
        {
            _Term.DoTerminate();
        }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;

        #endregion

        #region IWieldMountable Members

        public IEnumerable<string> SlotTypes
        {
            get
            {
                yield return ItemSlot.LargeWieldMount;
                yield break;
            }
        }

        #endregion

        protected override string ClassIconKey => @"light_xbow";
    }
}
