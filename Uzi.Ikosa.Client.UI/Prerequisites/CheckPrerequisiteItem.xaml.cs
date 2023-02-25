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
    /// Interaction logic for CheckPrerequisiteItem.xaml
    /// </summary>
    public partial class CheckPrerequisiteItem : UserControl
    {
        public CheckPrerequisiteItem()
        {
            try { InitializeComponent(); } catch { }
            DataContextChanged += new DependencyPropertyChangedEventHandler(CheckPrerequisiteItem_DataContextChanged);
        }

        #region void CheckPrerequisiteItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        void CheckPrerequisiteItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _check = this.CheckPrerequisiteInfo;
            if (_check != null)
            {
                cboRoll.SelectedIndex = _check.Value ?? 0;
                if (_check.Value != null)
                {
                    txtValue.Text = _check.Value.ToString();
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

        public CheckPrerequisiteInfo CheckPrerequisiteInfo
            => PrerequisiteModel?.Prerequisite as CheckPrerequisiteInfo;

        #region private void btnRoll_Click(object sender, RoutedEventArgs e)
        private void btnRoll_Click(object sender, RoutedEventArgs e)
        {
            var _check = CheckPrerequisiteInfo;
            if (_check != null)
            {
                // assumes parent is prerequisite list control
                var _proxy = PrerequisiteModel.PrerequisiteProxy.Proxies;
                if (_proxy != null)
                {
                    var _rollLog = _proxy.RollDice(CheckPrerequisiteInfo.Name, @"Success Check", @"1d20", PrerequisiteModel.PrerequisiteProxy.FulfillerID);
                    _check.Value = _rollLog.Total;
                    _check.IsReady = true;
                    txtValue.Text = _rollLog.Total.ToString();
                    txtValue.ToolTip = _rollLog;
                    if (PrerequisiteModel.IsSingleton)
                    {
                        _proxy.IkosaProxy.Service.SetPreRequisites(new[] { _check });
                        PrerequisiteModel.IsSent = true;
                    }
                }
            }
        }
        #endregion

        #region private void cboRoll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void cboRoll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var _check = CheckPrerequisiteInfo;
            if (_check != null)
            {
                if (cboRoll.SelectedIndex == 0)
                {
                    _check.Value = null;
                    _check.IsReady = false;
                    txtValue.Text = string.Empty;
                    txtValue.ToolTip = @"-";
                }
                else
                {
                    _check.Value = cboRoll.SelectedIndex;
                    _check.IsReady = true;
                    txtValue.Text = _check.Value.ToString();
                    txtValue.ToolTip = @"Manual";
                    if (PrerequisiteModel.IsSingleton)
                    {
                        PrerequisiteModel.PrerequisiteProxy.Proxies.IkosaProxy.Service
                            .SetPreRequisites(new[] { _check });
                        PrerequisiteModel.IsSent = true;
                    }
                }
            }
        }
        #endregion

        private void Val_Click(object sender, RoutedEventArgs e)
        {
            var _check = CheckPrerequisiteInfo;
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