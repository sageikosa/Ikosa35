using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Contracts;
using System.Diagnostics;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client.UI
{
    public class ItemListTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DoubleWeaponTemplate { get; set; }
        public DataTemplate MeleeWeaponTemplate { get; set; }
        public DataTemplate LoadableProjectileWeaponTemplate { get; set; }
        public DataTemplate ProjectileWeaponTemplate { get; set; }
        public DataTemplate NaturalWeaponTemplate { get; set; }
        public DataTemplate EmptyTemplate { get; set; }
        public DataTemplate ArmorTemplate { get; set; }
        public DataTemplate ShieldTemplate { get; set; }
        public DataTemplate AmmoBundleTemplate { get; set; }
        public DataTemplate SpellTriggerTemplate { get; set; }
        public DataTemplate AmmoTemplate { get; set; }
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate CreatureObjectTemplate { get; set; }
        public DataTemplate ObjectInfoVMTemplate { get; set; }
        public DataTemplate PowerClassTemplate { get; set; }
        public DataTemplate CasterClassTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            //if (item != null)
            //    Debug.WriteLine(string.Format(@"ItemListTemplateSelector.SelectTemplate={0}", item.GetType().FullName));
            if (item is NaturalWeaponInfo)
            {
                return NaturalWeaponTemplate;
            }
            else if (item is ProjectileWeaponInfo)
            {
                if (item is LoadableProjectileWeaponInfo)
                    return LoadableProjectileWeaponTemplate;
                return ProjectileWeaponTemplate;
            }
            else if (item is DoubleMeleeWeaponInfo)
            {
                return DoubleWeaponTemplate;
            }
            else if (item is MeleeWeaponInfo)
            {
                return MeleeWeaponTemplate;
            }
            else if (item is ArmorInfo)
            {
                return ArmorTemplate;
            }
            else if (item is ShieldInfo)
            {
                return ShieldTemplate;
            }
            else if (item is AmmoInfo)
            {
                return AmmoTemplate;
            }
            else if (item is CreatureObjectInfo)
            {
                return CreatureObjectTemplate;
            }
            else if (item is AmmoBundleInfo)
            {
                return AmmoBundleTemplate;
            }
            else if (item == null)
            {
                // nothing in the slot
                return EmptyTemplate;
            }
            else if (item is SpellTriggerInfo)
            {
                return SpellTriggerTemplate;
            }
            else if (item is ObjectInfoVM)
            {
                return ObjectInfoVMTemplate;
            }
            else if (item is PowerClassInfo _power)
            {
                if (_power is CasterClassInfo)
                {
                    return CasterClassTemplate;
                }
                return PowerClassTemplate;
            }
            else
            {
                // generic
                return ItemTemplate;
            }
        }

        public static ItemListTemplateSelector GetMenuDefault(ResourceDictionary dictionary)
        {
            var _selector = GetDefault(dictionary);
            _selector.CreatureObjectTemplate = dictionary[@"iconCreatureObject"] as DataTemplate;
            // TODO: consider troves...
            return _selector;
        }

        public static ItemListTemplateSelector GetDefault(ResourceDictionary dictionary)
        {
            DataTemplateKey _key<TT>() =>
                new DataTemplateKey(typeof(TT));

            return new ItemListTemplateSelector
            {
                DoubleWeaponTemplate = dictionary[_key<DoubleMeleeWeaponInfo>()] as DataTemplate,
                MeleeWeaponTemplate = dictionary[_key<MeleeWeaponInfo>()] as DataTemplate,
                NaturalWeaponTemplate = dictionary[_key<NaturalWeaponInfo>()] as DataTemplate,
                LoadableProjectileWeaponTemplate = dictionary[_key<LoadableProjectileWeaponInfo>()] as DataTemplate,
                ProjectileWeaponTemplate = dictionary[_key<ProjectileWeaponInfo>()] as DataTemplate,
                AmmoBundleTemplate = dictionary[_key<AmmoBundleInfo>()] as DataTemplate,
                ArmorTemplate = dictionary[_key<ArmorInfo>()] as DataTemplate,
                ShieldTemplate = dictionary[_key<ShieldInfo>()] as DataTemplate,
                AmmoTemplate = dictionary[_key<AmmoInfo>()] as DataTemplate,
                SpellTriggerTemplate = dictionary[_key<SpellTriggerInfo>()] as DataTemplate,
                EmptyTemplate = dictionary[@"tmplEmpty"] as DataTemplate,
                CreatureObjectTemplate = dictionary[@"tmplCreatureObject"] as DataTemplate,
                ObjectInfoVMTemplate = dictionary[_key<ObjectInfoVM>()] as DataTemplate,
                PowerClassTemplate = dictionary[_key<PowerClassInfo>()] as DataTemplate,
                CasterClassTemplate = dictionary[_key<CasterClassInfo>()] as DataTemplate,
                // TODO: consider troves...
                ItemTemplate = dictionary[@"tmplItem"] as DataTemplate
            };
        }
    }
}
