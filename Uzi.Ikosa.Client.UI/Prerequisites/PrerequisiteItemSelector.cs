using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Client.UI
{
    public class PrerequisiteItemSelector : DataTemplateSelector
    {
        public DataTemplate AimTargetPrerequisiteTemplate { get; set; }
        public DataTemplate CheckPrerequisiteTemplate { get; set; }
        public DataTemplate ChoicePrerequisiteTemplate { get; set; }
        public DataTemplate CoreSelectPrerequisiteTemplate { get; set; }
        public DataTemplate ReactivePrerequisiteTemplate { get; set; }
        public DataTemplate OpportunisticPrerequisiteTemplate { get; set; }
        public DataTemplate PromptTurnTrackerPrerequisiteTemplate { get; set; }
        public DataTemplate RollPrerequisiteTemplate { get; set; }
        public DataTemplate SavePrerequisiteTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var _item = item as PrerequisiteModel;
            if (_item.Prerequisite is ValuePrerequisiteInfo)
            {
                if (_item.Prerequisite is RollPrerequisiteInfo)
                {
                    return RollPrerequisiteTemplate;
                }
                else if (_item.Prerequisite is SavePrerequisiteInfo)
                {
                    return SavePrerequisiteTemplate;
                }
                return CheckPrerequisiteTemplate;
            }
            else if (_item.Prerequisite is ChoicePrerequisiteInfo)
            {
                return ChoicePrerequisiteTemplate;
            }
            else if (_item.Prerequisite is ReactivePrerequisiteInfo)
            {
                return ReactivePrerequisiteTemplate;
            }
            else if (_item.Prerequisite is OpportunisticPrerequisiteInfo)
            {
                return OpportunisticPrerequisiteTemplate;
            }
            else if (_item.Prerequisite is AimTargetPrerequisiteInfo)
            {
                return AimTargetPrerequisiteTemplate;
            }
            else if (_item.Prerequisite is PromptTurnTrackerPrerequisiteInfo)
            {
                return PromptTurnTrackerPrerequisiteTemplate;
            }
            else if (_item.Prerequisite is CoreSelectPrerequisiteInfo)
            {
                return CoreSelectPrerequisiteTemplate;
            }
            return null;
        }
    }
}
