using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.UI
{
    public class AdjunctListItemSelector : DataTemplateSelector
    {
        public DataTemplate EnergyResistorTemplate { get; set; }
        public DataTemplate SlipperyArmorTemplate { get; set; }
        public DataTemplate WeaponSpecialAbilityTemplate { get; set; }
        public DataTemplate MagicAugmentTemplate { get; set; }
        public DataTemplate AbilityEnhanceSlotActivationTemplate { get; set; }
        public DataTemplate NaturalArmorSlotActivationTemplate { get; set; }
        public DataTemplate ResistanceSlotActivationTemplate { get; set; }
        public DataTemplate ArmorSlotActivationTemplate { get; set; }
        public DataTemplate DeflectionSlotActivationTemplate { get; set; }
        public DataTemplate EnergyResistorSlotActivationTemplate { get; set; }
        public DataTemplate SkillBonusSlotActivationTemplate { get; set; }
        public DataTemplate AdjunctSlotActivationTemplate { get; set; }
        public DataTemplate SlottedItemSpellActivationTemplate { get; set; }
        public DataTemplate SpellCommandWordTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is EnergyResistor)
            {
                return EnergyResistorTemplate;
            }

            if (item is SlipperyArmor)
            {
                return SlipperyArmorTemplate;
            }

            if (item is WeaponSpecialAbility)
            {
                return WeaponSpecialAbilityTemplate;
            }

            if (item is MagicAugment)
            {
                return MagicAugmentTemplate;
            }

            if (item is AbilityEnhanceSlotActivation)
            {
                return AbilityEnhanceSlotActivationTemplate;
            }

            if (item is NaturalArmorSlotActivation)
            {
                return NaturalArmorSlotActivationTemplate;
            }

            if (item is ResistanceSlotActivation)
            {
                return ResistanceSlotActivationTemplate;
            }

            if (item is ArmorSlotActivation)
            {
                return ArmorSlotActivationTemplate;
            }

            if (item is DeflectionSlotActivation)
            {
                return DeflectionSlotActivationTemplate;
            }

            if (item is EnergyResistorSlotActivation)
            {
                return EnergyResistorSlotActivationTemplate;
            }

            if (item.GetType().IsGenericType
                && typeof(SkillBonusSlotActivation<>).Equals(item.GetType().GetGenericTypeDefinition()))
            {
                return SkillBonusSlotActivationTemplate;
            }

            if (item is AdjunctSlotActivation)
            {
                return AdjunctSlotActivationTemplate;
            }

            if (item is SlottedItemSpellActivation)
            {
                return SlottedItemSpellActivationTemplate;
            }

            if (item is SpellCommandWord)
            {
                return SpellCommandWordTemplate;
            }
            // TODO:
            return null;
        }
    }
}
