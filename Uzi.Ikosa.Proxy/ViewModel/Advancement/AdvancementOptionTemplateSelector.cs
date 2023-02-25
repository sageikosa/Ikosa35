using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class AdvancementOptionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LeafNodeTemplate { get; set; }
        public DataTemplate BranchNodeTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is AdvancementOptionVM _advOptVM)
            {
                if (_advOptVM.AvailableParameters.Any())
                    return BranchNodeTemplate;
                return LeafNodeTemplate;
            }
            return null;
        }
    }
}
