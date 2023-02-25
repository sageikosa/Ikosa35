using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    [WeaponHead(@"-", DamageType.None, 20, 2, typeof(Materials.RopeMaterial), Lethality.AlwaysNonLethal)]
    [ItemInfo(@"Web", @"Exotic Ranged Entangling", @"net")]
    public class SpiderWebNet : ThrowingNet
    {
        public SpiderWebNet(Creature creature, IPowerClass powerClass, int maxRangeIncrements)
            : base()
        {
            _SlotType = ItemSlot.Spinneret;
            switch (creature.Sizer.Size.Order)
            {
                case -2: MaxStructurePoints.BaseValue = 2; break;
                case -1: MaxStructurePoints.BaseValue = 4; break;
                case 0: MaxStructurePoints.BaseValue = 6; break;
                case 1: MaxStructurePoints.BaseValue = 12; break;
                case 2: MaxStructurePoints.BaseValue = 14; break;
                case 3: MaxStructurePoints.BaseValue = 16; break;
                case 4: MaxStructurePoints.BaseValue = 18; break;
                default: break;
            }

            EscapeDifficulty.BaseValue
                = creature.GetIntrinsicPowerDifficulty(powerClass, MnemonicCode.Con, typeof(SpiderWebNet)).EffectiveValue;

            BurstFreeDifficulty.BaseDoubleValue
                = creature.GetIntrinsicPowerDifficulty(powerClass, MnemonicCode.Con, typeof(SpiderWebNet), this).EffectiveValue;

            _MaxRange = 5;
        }

        #region data
        private int _MaxRange;
        #endregion

        /// <summary>Never transferrable</summary>
        public override bool IsTransferrable => false;

        public override bool IsActive => MainSlot != null;

        #region IActionProvider Members

        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // action when being held correctly
            var _budget = budget as LocalActionBudget;
            if (IsActive)
            {
                // find strikes that can be performed based on available ammunition
                if (_budget.CanPerformRegular)
                {
                    foreach (var _strk in WeaponStrikes())
                    {
                        yield return new RegularAttack(_strk);
                    }
                }
            }
            else if (Adjuncts.OfType<Covering>()
                .Any(_c => _c.IsCovering && _c.CoveringWrapper.CreaturePossessor == _budget.Actor))
            {
                if (_budget.CanPerformTotal)
                {
                    yield return new EscapeArtistry(this, false, @"201");
                    yield return new BurstFree(this, false, @"202");
                }
            }

            yield break;
        }

        #endregion

        #region IRangedSource Members

        public override int RangeIncrement
            => CreaturePossessor?.Feats.Contains(typeof(Feats.FarShotFeat)) ?? false
            ? 20
            : 10;

        public override int MaxRange => RangeIncrement * _MaxRange;

        #endregion

        public override ActionTime SlottingTime => new ActionTime(TimeType.Free);
        public override ActionTime UnslottingTime => new ActionTime(TimeType.Free);
        public override ActionTime EscapeTime => new ActionTime(TimeType.Regular);
        public override ActionTime BurstFreeTime => new ActionTime(TimeType.Regular);

        public override void DoEscape()
        {
            // do not leave spider webs around when escaped from
            StructurePoints = 0;
        }
    }
}
