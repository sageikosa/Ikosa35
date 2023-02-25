using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Workshop
{
    public class TeamGroupTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
            => ((item as TeamMember)?.IsPrimary ?? false)
                ? IsPrimaryTemplate
                : IsAssociateTemplate;

        public DataTemplate IsPrimaryTemplate { get; set; }
        public DataTemplate IsAssociateTemplate { get; set; }
    }
}
