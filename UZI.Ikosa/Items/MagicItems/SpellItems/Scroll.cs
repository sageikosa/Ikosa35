using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class Scroll : SlottedItemBase, IActionProvider, IProcessFeedback
    {
        #region Construction
        /// <summary>New empty scroll</summary>
        /// <param name="name">name of the scroll</param>
        public Scroll(string name, Material itemMaterial, int maxHealthPoints, Size size)
            : base(name, ItemSlot.HoldingSlot)
        {
            InitItem(itemMaterial, maxHealthPoints, size);
            AddIInteractHandler(this);
        }

        /// <summary>Scroll with one or more spells on it</summary>
        /// <param name="name">name of the scroll</param>
        /// <param name="storedSpells">enumeration of spells to seed the scroll</param>
        public Scroll(string name, Material itemMaterial, int maxHealthPoints, Size size,
            IEnumerable<SpellSource> storedSpells)
            : this(name, itemMaterial, maxHealthPoints, size)
        {
            foreach (SpellSource _storeSpell in storedSpells)
            {
                AddAdjunct(new SpellCompletion(_storeSpell, false));
            }
        }

        /// <summary>Scroll with one stored spell on it</summary>
        /// <param name="name">name of the scroll</param>
        /// <param name="storedSpell">spell to seed the scroll</param>
        public Scroll(string name, Material itemMaterial, int maxHealthPoints, Size size,
            SpellSource storedSpell, bool autoActivation)
            : this(name, itemMaterial, maxHealthPoints, size)
        {
            AddAdjunct(new SpellCompletion(storedSpell, autoActivation));
        }

        private void InitItem(Material itemMaterial, int maxHP, Size size)
        {
            Sizer.NaturalSize = size;
            ItemMaterial = itemMaterial;
            MaxStructurePoints.BaseValue = maxHP;
            // TODO: ... break DC of 8.
            BaseWeight = 0.1d;
        }
        #endregion

        #region protected override void OnSetItemSlot()
        protected override void OnSetItemSlot()
        {
            CreaturePossessor.Actions.Providers.Add(this, this);
            base.OnSetItemSlot();
        }
        #endregion

        #region protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        {
            CreaturePossessor.Actions.Providers.Remove(this);
            base.OnClearSlots(slotA, slotB);
        }
        #endregion

        public override bool IsTransferrable
            => true;

        /// <summary>Enumerate all spells on the scroll</summary>
        public IEnumerable<SpellCompletion> Spells
            => SpellCompletion.GetSpellCompletions(this);

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
            => (from _complete in SpellCompletion.GetSpellCompletions(this)
                from _act in _complete.GetActions(budget)
                select _act);

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);

        #region IInteractHandler Members

        void IInteractHandler.HandleInteraction(Interaction workSet)
        {
        }

        IEnumerable<Type> IInteractHandler.GetInteractionTypes()
        {
            yield return typeof(GetInfoData);    // mainline interaction
            yield return typeof(AddAdjunctData); // feedback processing
            yield break;
        }

        bool IInteractHandler.LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            if (typeof(GetInfoData).Equals(interactType))
            {
                return true;
            }

            // last feedback processor
            if (typeof(AddAdjunctData).Equals(interactType))
                return true;

            if (typeof(RemoveAdjunctData).Equals(interactType))
                return true;

            return false;
        }

        #endregion

        protected override string ClassIconKey
            => @"scroll";

        #region IProcessFeedback Members

        void IProcessFeedback.ProcessFeedback(Interaction workSet)
        {
            if (((workSet?.InteractData as AddAdjunctData)?.Adjunct is SpellCompletion)
                || ((workSet?.InteractData as RemoveAdjunctData)?.Adjunct is SpellCompletion))
            {
                // changed up spell completions
                DoPropertyChanged(nameof(Spells));
            }
            else if ((workSet?.InteractData is GetInfoData _getInfo)
                && (workSet.Actor != null)
                && (workSet.Feedback.OfType<InfoFeedback>().FirstOrDefault() is InfoFeedback _info)
                && (_info.Information is ObjectInfo _objInfo))
            {
                // decipher information on anything deciphered
                _objInfo.AdjunctInfos = _objInfo.AdjunctInfos.Union(from _sc in SpellCompletion.GetSpellCompletions(this)
                                                                    select _sc.GetDescription(workSet.Actor.ID)).ToArray();
            }
        }

        #endregion

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
