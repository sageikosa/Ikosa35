using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client.UI
{
    public class TimelineManagerActorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ActionSetup { get; set; }
        public DataTemplate ActionCancel { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is CreatureTimelinePendingModel _critter)
            {
                if (_critter.CreatureTrackerInfo?.LocalActionBudgetInfo?.HeldActivity != null)
                {
                    return ActionCancel;
                }
                return ActionSetup;
            }
            return null;
        }
    }
}
