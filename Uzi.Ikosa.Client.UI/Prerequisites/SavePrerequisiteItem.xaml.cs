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

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for SavePrerequisiteItem.xaml
    /// </summary>
    public partial class SavePrerequisiteItem : UserControl
    {
        public SavePrerequisiteItem()
        {
            try { InitializeComponent(); } catch { }
            DataContextChanged += new DependencyPropertyChangedEventHandler(CheckPrerequisiteItem_DataContextChanged);
        }

        #region void CheckPrerequisiteItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        void CheckPrerequisiteItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _save = this.SavePrerequisiteInfo;
            if (_save != null)
            {
                cboRoll.SelectedIndex = _save.Value ?? 0;
                if (_save.Value != null)
                {
                    txtValue.Text = _save.Value.ToString();
                }
            }
            else
            {
                cboRoll.SelectedIndex = -1;
            }
        }
        #endregion

        public PrerequisiteModel PrerequisiteModel
            => DataContext as PrerequisiteModel;

        public SavePrerequisiteInfo SavePrerequisiteInfo
            => PrerequisiteModel?.Prerequisite as SavePrerequisiteInfo;

        #region private void btnRoll_Click(object sender, RoutedEventArgs e)
        private void btnRoll_Click(object sender, RoutedEventArgs e)
        {
            var _save = SavePrerequisiteInfo;
            if (_save != null)
            {
                // assumes parent is prerequisite list control
                var _proxy = PrerequisiteModel.PrerequisiteProxy.Proxies;
                if (_proxy != null)
                {
                    var _rollLog = _proxy.RollDice(SavePrerequisiteInfo.Name, SavePrerequisiteInfo.SaveType, @"1d20", PrerequisiteModel.PrerequisiteProxy.FulfillerID);
                    _save.Value = _rollLog.Total;
                    _save.IsReady = true;
                    txtValue.Text = _rollLog.Total.ToString();
                    txtValue.ToolTip = _rollLog;
                    if (PrerequisiteModel.IsSingleton)
                    {
                        _proxy.IkosaProxy.Service.SetPreRequisites(new[] { SavePrerequisiteInfo });
                        PrerequisiteModel.IsSent = true;
                    }
                }
            }
        }
        #endregion

        #region private void cboRoll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void cboRoll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var _save = this.SavePrerequisiteInfo;
            if (_save != null)
            {
                if (cboRoll.SelectedIndex == 0)
                {
                    _save.Value = null;
                    _save.IsReady = false;
                    txtValue.Text = string.Empty;
                    txtValue.ToolTip = @"-";
                }
                else
                {
                    _save.Value = cboRoll.SelectedIndex;
                    _save.IsReady = true;
                    txtValue.Text = _save.Value.ToString();
                    txtValue.ToolTip = @"Manual";
                    if (PrerequisiteModel.IsSingleton)
                    {
                        PrerequisiteModel.PrerequisiteProxy.Proxies.IkosaProxy.Service
                            .SetPreRequisites(new[] { _save });
                        PrerequisiteModel.IsSent = true;
                    }
                }
            }
        }
        #endregion

        private void Val_Click(object sender, RoutedEventArgs e)
        {
            var _check = SavePrerequisiteInfo;
            if (_check != null)
            {
                // assumes parent is prerequisite list control
                _check.Value = Convert.ToInt32((sender as Button)?.Content.ToString() ?? @"1");
                _check.IsReady = true;
                txtValue.Text = _check.Value.ToString();
                txtValue.ToolTip = null;
                if (PrerequisiteModel.IsSingleton)
                {
                    PrerequisiteModel.PrerequisiteProxy.Proxies.IkosaProxy.Service
                        .SetPreRequisites(new[] { _check });
                    PrerequisiteModel.IsSent = true;
                }
            }
        }
    }
}
