using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Client
{
    public class InventoryAdminTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemSlotWithSubInUseTemplate { get; set; }
        public DataTemplate ItemSlotWithSubEmptyTemplate { get; set; }
        public DataTemplate ItemSlotInUseTemplate { get; set; }
        public DataTemplate ItemSlotEmptyTemplate { get; set; }
        public DataTemplate MountSlotWithSubInUseTemplate { get; set; }
        public DataTemplate MountSlotWithSubEmptyTemplate { get; set; }
        public DataTemplate MountSlotInUseTemplate { get; set; }
        public DataTemplate MountSlotEmptyTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ItemSlotInfo)
            {
                var _slot = item as ItemSlotInfo;
                if (_slot.SlotType.Contains(@"-Mount"))
                {
                    if (string.IsNullOrEmpty(_slot.SubType))
                        if (_slot.ItemInfo != null)
                            return MountSlotInUseTemplate;
                        else
                            return MountSlotEmptyTemplate;
                    else
                        if (_slot.ItemInfo != null)
                            return MountSlotWithSubInUseTemplate;
                        else
                            return MountSlotWithSubEmptyTemplate;
                }
                else
                {
                    if (string.IsNullOrEmpty(_slot.SubType))
                        if (_slot.ItemInfo != null)
                            return ItemSlotInUseTemplate;
                        else
                            return ItemSlotEmptyTemplate;
                    else
                        if (_slot.ItemInfo != null)
                            return ItemSlotWithSubInUseTemplate;
                        else
                            return ItemSlotWithSubEmptyTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
