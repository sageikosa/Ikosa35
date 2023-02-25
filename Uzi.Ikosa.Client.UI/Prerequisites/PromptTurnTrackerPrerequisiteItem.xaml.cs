using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for PromptTurnTrackerPrerequisiteItem.xaml
    /// </summary>
    public partial class PromptTurnTrackerPrerequisiteItem : UserControl
    {
        public PromptTurnTrackerPrerequisiteItem()
        {
            try { InitializeComponent(); } catch { }
        }

        public PromptTurnTrackerPrerequisiteModel PrerequisiteModel
            => DataContext as PromptTurnTrackerPrerequisiteModel;

        public PromptTurnTrackerPrerequisiteInfo PromptTurnTrackerPrerequisiteInfo
            => PrerequisiteModel?.Prerequisite as PromptTurnTrackerPrerequisiteInfo;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var _prompt = PromptTurnTrackerPrerequisiteInfo;
            if (_prompt != null)
            {
                _prompt.Done = true;
                PrerequisiteModel.PrerequisiteProxy.Proxies.IkosaProxy.Service
                    .SetPreRequisites(new[] { _prompt });
                PrerequisiteModel.IsSent = true;
            }
        }
    }
}
