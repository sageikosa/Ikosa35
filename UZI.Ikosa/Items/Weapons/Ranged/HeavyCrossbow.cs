using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [
    Serializable,
    WeaponHead(@"1d10", DamageType.Piercing, 19, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Crossbow, Heavy", @"Simple Ranged 1d10 (19-20/x2) Piercing", @"heavy_xbow")
    ]
    public class HeavyCrossbow : CrossbowBase, IWieldMountable
    {
        public HeavyCrossbow()
            : base(@"Crossbow, Heavy", 120, Size.Medium)
        {
            _Term = new TerminateController(this);
            _OneHand = new Delta(-4, typeof(HeavyCrossbow), @"One Hand Firing");
            Setup();
        }

        /// <summary>-4 penalty for firing with one hand</summary>
        private Delta _OneHand;

        private TerminateController _Term;
        protected override IWeaponHead GetProxyHead() { return this.GetWeaponHead<HeavyCrossbow>(true); }

        private void Setup()
        {
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Simple;
            this.Price.CorePrice = 50m;
            this.MaxStructurePoints.BaseValue = 7;
            this.BaseWeight = 8d;
        }

        public override TimeType ReloadAction
        {
            get
            {
                if (this.CreaturePossessor != null)
                {
                    if (CreaturePossessor.Feats.Contains(typeof(Feats.RapidReloadFeat<HeavyCrossbow>)))
                    {
                        return TimeType.Brief;
                    }
                }

                return TimeType.Total;
            }
        }

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

        public IDelta QualifiedDelta(Qualifier qualify)
        {
            // only relevant if this is being used by one hand
            if (this.SecondarySlot == null)
            {
                var _iAct = qualify as Interaction;
                if (_iAct != null)
                {
                    var _rAtk = _iAct.InteractData as RangedAttackData;
                    if (_rAtk != null)
                    {
                        var _rHead = qualify.Source as RangedAmmunition;
                        if (_rHead != null)
                        {
                            // must be a ranged attack interaction with ranged ammunition from this projector
                            if (_rHead.Projector == this)
                            {
                                return _OneHand;
                            }
                        }
                    }
                }
            }
            return null;
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

        protected override string ClassIconKey { get { return @"heavy_xbow"; } }
    }
}
