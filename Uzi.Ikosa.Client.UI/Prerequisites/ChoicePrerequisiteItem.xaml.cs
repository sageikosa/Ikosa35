using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for ChoicePrerequisiteItem.xaml
    /// </summary>
    public partial class ChoicePrerequisiteItem : UserControl
    {
        public ChoicePrerequisiteItem()
        {
            try { InitializeComponent(); } catch { }
            DataContextChanged += new DependencyPropertyChangedEventHandler(ChoicePrerequisiteItem_DataContextChanged);
        }

        public PrerequisiteModel PrerequisiteModel
            => DataContext as PrerequisiteModel;

        public ChoicePrerequisiteInfo ChoicePrerequisiteInfo
            => PrerequisiteModel?.Prerequisite as ChoicePrerequisiteInfo;

        #region void ChoicePrerequisiteItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        void ChoicePrerequisiteItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ChoicePrerequisiteInfo?.Selected?.Key))
            {
                cboChoice.SelectedItem = ChoicePrerequisiteInfo.Choices
                    .FirstOrDefault(_c => _c.Key == ChoicePrerequisiteInfo.Selected.Key);
            }
        }
        #endregion

        #region private void cboChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void cboChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var _choice = ChoicePrerequisiteInfo;
            if (_choice != null)
            {
                _choice.Selected = (cboChoice.SelectedItem as OptionAimOption);
                _choice.IsReady = (_choice.Selected != null);
            }
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var _choice = ChoicePrerequisiteInfo;
            if (_choice != null)
            {
                _choice.Selected = (sender as Button)?.Tag as OptionAimOption;
                _choice.IsReady = (_choice.Selected != null);
                PrerequisiteModel.PrerequisiteProxy.Proxies.IkosaProxy.Service
                    .SetPreRequisites(new[] { _choice });
                PrerequisiteModel.IsSent = true;
            }
        }
    }
}
