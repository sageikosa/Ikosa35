using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions;
using Uzi.Core.Dice;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Alchemal
{
    [Serializable]
    public class AlignedWater : ItemBase, IActionProvider, ISplatterWeapon
    {
        #region Construction
        public AlignedWater(string name, Alignment axialAlignment)
            : base(name, Size.Miniature)
        {
            _Align = axialAlignment;
            Weight = 1;
            Price.CorePrice = 25;
            Price.IsTradeGood = true;
        }
        #endregion

        private Alignment _Align;
        public Alignment Alignment => _Align;

        #region public void DoneUseItem()
        /// <summary>removes the item from the creature (permanently)</summary>
        public void DoneUseItem()
        {
            // unslot (the holding wrapper)
            var _slot = CreaturePossessor.Body.ItemSlots.AllSlots
                .Where(_s => _s.SlotType.Equals(ItemSlot.HoldingSlot) && _s.SlottedItem.BaseObject.Equals(this))
                .FirstOrDefault();
            _slot.SlottedItem.ClearSlots();
            Possessor = null;
        }
        #endregion

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            if (_budget.CanPerformRegular)
            {
                yield return new DirectSplatterAttack(this, @"101");
                yield return new IndirectSplatterAttack(this, @"102");
                // TODO: ranged touch attack "pour" onto incorporeal (or the like)
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);

        #endregion

        #region ISplatterWeapon Members

        #region public void ApplyDirect(ApplySplatterStep step)
        public void ApplyDirect(ApplySplatterStep step)
        {
            if (step.Target is Creature _critter)
            {
                // make sure the creature is affected by the water (opposable alignment, or holy water against an undead)
                var _cType = _critter.CreatureType;
                if (((_cType is OutsiderType) && Alignment.Opposable(_critter.Alignment))
                    || ((Alignment.Ethicality == GoodEvilAxis.Good) && (_cType is UndeadType)))
                {
                    if (step.GetPrerequisite<RollPrerequisite>() is RollPrerequisite _roll)
                    {
                        var _dmg = new DamageData(_roll.RollValue, false, @"Hit", 0);
                        var _deliver = new DeliverDamageData(step.Activity.Actor, _dmg.ToEnumerable(), false, false);
                        var _interact = new Interaction(step.Activity.Actor, this, _critter, _deliver);
                        _critter.HandleInteraction(_interact);
                    }
                }
            }
        }
        #endregion

        #region public void ApplySplatter(ApplySplatterStep step)
        public void ApplySplatter(ApplySplatterStep step)
        {
            if (step.Target is Creature _critter)
            {
                // make sure the creature is affected by the water (opposable alignment, or holy water against an undead)
                var _cType = _critter.CreatureType;
                if (((_cType is OutsiderType) && Alignment.Opposable(_critter.Alignment))
                    || ((Alignment.Ethicality == GoodEvilAxis.Good) && (_cType is UndeadType)))
                {
                    var _dmg = new DamageData(1, false, @"Splash", 0);
                    var _deliver = new DeliverDamageData(step.Activity.Actor, _dmg.ToEnumerable(), false, false);
                    var _interact = new Interaction(step.Activity.Actor, this, _critter, _deliver);
                    _critter.HandleInteraction(_interact);
                }
            }
        }
        #endregion

        public virtual int RangeIncrement => 10;
        public virtual int MaxRange => RangeIncrement * 5;

        #region public IEnumerable<StepPrerequisite> GetDirectPrerequisites(CoreActivity activity)
        public IEnumerable<StepPrerequisite> GetDirectPrerequisites(CoreActivity activity)
        {
            if (activity != null)
            {
                yield return new RollPrerequisite(this, null, activity.Actor, @"Roll.Damage", @"Direct Damage", new DiceRoller(2, 4), false);
            }
            else
            {
                yield return new RollPrerequisite(this, @"Roll.Damage", @"Direct Damage", new DiceRoller(2, 4), false);
            }
            yield break;
        }
        #endregion

        #region public IEnumerable<StepPrerequisite> GetSplatterPrerequisites(CoreActivity activity)
        public IEnumerable<StepPrerequisite> GetSplatterPrerequisites(CoreActivity activity)
        {
            // NONE
            yield break;
        }
        #endregion

        public Lethality Lethality => Lethality.AlwaysLethal;

        #endregion

        protected override string ClassIconKey => string.Empty;
    }
}
