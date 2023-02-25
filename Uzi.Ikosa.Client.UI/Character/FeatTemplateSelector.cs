using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Client.UI
{
    public class FeatTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FeatTemplate { get; set; }
        public DataTemplate ActionFeatTemplate { get; set; }
        public IEnumerable<ActionInfo> Actions { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
            => ((item is FeatInfo _feat)
                && (ActionFeatTemplate != null)
                && (Actions?.Any(_a => _a.Provider.ID == _feat.ID) ?? false))
            ? ActionFeatTemplate
            : FeatTemplate;
    }
}

