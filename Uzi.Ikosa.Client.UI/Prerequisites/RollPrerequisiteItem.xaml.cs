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
using System.Windows.Controls.Primitives;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for RollPrerequisiteItem.xaml
    /// </summary>
    public partial class RollPrerequisiteItem : UserControl
    {
        public RollPrerequisiteItem()
        {
            try { InitializeComponent(); } catch { }
            DataContextChanged += new DependencyPropertyChangedEventHandler(RollPrerequisiteItem_DataContextChanged);
        }

        private UniformGrid _FocusedGrid = null;

        #region void RollPrerequisiteItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        void RollPrerequisiteItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _roll = RollPrerequisiteInfo;
            if (_roll != null)
            {
                if (_roll.Value != null)
                {
                    txtValue.Text = _roll.Value.ToString();
                }
                switch (_roll.SingletonSides)
                {
                    case 2:
                        ug2.Visibility = Visibility.Visible;
                        _FocusedGrid = ug2;
                        break;
                    case 3:
                        ug3.Visibility = Visibility.Visible;
                        _FocusedGrid = ug3;
                        break;
                    case 4:
                        ug4.Visibility = Visibility.Visible;
                        _FocusedGrid = ug4;
                        break;
                    case 6:
                        ug6.Visibility = Visibility.Visible;
                        _FocusedGrid = ug6;
                        break;
                    case 8:
                        ug8.Visibility = Visibility.Visible;
                        _FocusedGrid = ug8;
                        break;
                    case 10:
                        ug10.Visibility = Visibility.Visible;
                        _FocusedGrid = ug10;
                        break;
                    case 12:
                        ug12.Visibility = Visibility.Visible;
                        _FocusedGrid = ug12;
                        break;
                    case 20:
                        ug20.Visibility = Visibility.Visible;
                        _FocusedGrid = ug20;
                        break;
                    default:
                        pnlGeneral.Visibility = Visibility.Visible;
                        break;
                }
            }
        }
        #endregion

        public PrerequisiteModel PrerequisiteModel
            => DataContext as PrerequisiteModel;

        public RollPrerequisiteInfo RollPrerequisiteInfo
            => PrerequisiteModel?.Prerequisite as RollPrerequisiteInfo;

        #region private void btnRoll_Click(object sender, RoutedEventArgs e)
        private void btnRoll_Click(object sender, RoutedEventArgs e)
        {
            var _roll = RollPrerequisiteInfo;
            if (_roll != null)
            {
                var _proxy = PrerequisiteModel.PrerequisiteProxy.Proxies;
                if (_proxy != null)
                {
                    var _rollLog = _proxy.RollDice(RollPrerequisiteInfo.Name, @"Dice Roll", RollPrerequisiteInfo.Expression, PrerequisiteModel.PrerequisiteProxy.FulfillerID);
                    _roll.Value = _rollLog.Total;
                    _roll.IsReady = true;
                    txtValue.Text = _rollLog.Total.ToString();
                    txtValue.ToolTip = _rollLog;
                    if (PrerequisiteModel.IsSingleton)
                    {
                        PrerequisiteModel.PrerequisiteProxy.Proxies.IkosaProxy.Service.SetPreRequisites(new[] { RollPrerequisiteInfo });
                    }
                    else
                    {
                        ClearButtons();
                        if (_FocusedGrid != null)
                        {
                            var _total = _rollLog.Total.ToString();
                            foreach (var _btn in _FocusedGrid.Children.OfType<Button>()
                                .Where(_b => _b.Content.ToString() == _total))
                            {
                                _btn.Style = (Style)FindResource(@"bsSelected");
                                break;
                            }
                        }
                    }
                }
            }
            e.Handled = true;
        }
        #endregion

        #region private void txtRoll_TextChanged(object sender, RoutedEventArgs e)
        private void txtRoll_TextChanged(object sender, RoutedEventArgs e)
        {
            var _txt = sender as TextBox;

            var _roll = RollPrerequisiteInfo;
            if (_roll != null)
            {
                // validate value
                int _out = 0;
                if (!int.TryParse(_txt.Text, out _out))
                {
                    _txt.Tag = @"Invalid";
                    _roll.Value = null;
                    _roll.IsReady = false;
                    return;
                }
                else if (_out <= 0)
                {
                    // must be positive
                    _txt.Tag = @"Invalid";
                    _roll.Value = null;
                    _roll.IsReady = false;
                    return;
                }

                // set as prerequisite value
                _roll.Value = _out;
                _roll.IsReady = true;
                _txt.ToolTip = @"Manual";
                _txt.Tag = null;
            }
        }
        #endregion

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            var _roll = RollPrerequisiteInfo;
            if (_roll?.IsReady ?? false)
            {
                if (PrerequisiteModel.IsSingleton)
                {
                    PrerequisiteModel.PrerequisiteProxy.Proxies.IkosaProxy.Service.SetPreRequisites(new[] { _roll });
                    PrerequisiteModel.IsSent = true;
                }
            }
        }

        #region private void ClearButtons()
        private void ClearButtons()
        {
            void _clearGridButtons(UniformGrid grid)
            {
                var _style = (Style)FindResource(@"bsValue");
                foreach (var _btn in grid.Children.OfType<Button>())
                    _btn.Style = _style;
            }

            if (_FocusedGrid != null)
                _clearGridButtons(_FocusedGrid);
        }
        #endregion

        #region private void Val_Click(object sender, RoutedEventArgs e)
        private void Val_Click(object sender, RoutedEventArgs e)
        {
            var _roll = RollPrerequisiteInfo;
            if (_roll != null)
            {
                var _button = sender as Button;
                _roll.Value = Convert.ToInt32(_button?.Content.ToString() ?? @"1");
                _roll.IsReady = true;
                txtValue.Text = _roll.Value.ToString();
                txtValue.ToolTip = null;
                if (PrerequisiteModel.IsSingleton)
                {
                    PrerequisiteModel.PrerequisiteProxy.Proxies.IkosaProxy.Service
                        .SetPreRequisites(new[] { _roll });
                    PrerequisiteModel.IsSent = true;
                }
                else
                {
                    ClearButtons();
                    _button.Style = (Style)FindResource(@"bsSelected");
                }
            }
        }
        #endregion
    }
}
