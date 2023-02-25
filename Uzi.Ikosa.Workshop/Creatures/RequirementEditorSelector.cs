using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Workshop
{
    public class RequirementEditorSelector : DataTemplateSelector
    {
        public DataTemplate SingleTemplate { get; set; }
        public DataTemplate MultiTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
            => (item as AdvancementLogItem)?.Requirements.Count() < 2
            ? SingleTemplate
            : MultiTemplate;
    }
}
