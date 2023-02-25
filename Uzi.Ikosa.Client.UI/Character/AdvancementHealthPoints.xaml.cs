using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for AdvancementHealthPoints.xaml
    /// </summary>
    public partial class AdvancementHealthPoints : UserControl
    {
        public AdvancementHealthPoints()
        {
            try { InitializeComponent(); } catch { }
        }

        private AdvancementVM AdvancementVM
            => DataContext as AdvancementVM;

        private UniformGrid _FocusedGrid = null;

        public PowerDieInfo PowerDieInfo
        {
            get { return (PowerDieInfo)GetValue(PowerDieInfoProperty); }
            set { SetValue(PowerDieInfoProperty, value); }
        }

        public static readonly DependencyProperty PowerDieInfoProperty =
            DependencyProperty.Register(nameof(PowerDieInfo), typeof(PowerDieInfo), typeof(AdvancementHealthPoints),
                new UIPropertyMetadata(null, new PropertyChangedCallback(PowerDieInfoChanged)));

        static void PowerDieInfoChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is AdvancementHealthPoints _aHP)
            {
                _aHP.ug12.Visibility = Visibility.Collapsed;
                _aHP.ug10.Visibility = Visibility.Collapsed;
                _aHP.ug8.Visibility = Visibility.Collapsed;
                _aHP.ug6.Visibility = Visibility.Collapsed;
                _aHP.ug4.Visibility = Visibility.Collapsed;
                _aHP.ug3.Visibility = Visibility.Collapsed;
                _aHP._FocusedGrid = null;
                if (args.NewValue is PowerDieInfo _pd)
                {
                    switch (_aHP.AdvancementVM.SelectedClass.PowerDieSize)
                    {
                        case 3: _aHP._FocusedGrid = _aHP.ug3; break;
                        case 4: _aHP._FocusedGrid = _aHP.ug4; break;
                        case 6: _aHP._FocusedGrid = _aHP.ug6; break;
                        case 8: _aHP._FocusedGrid = _aHP.ug8; break;
                        case 10: _aHP._FocusedGrid = _aHP.ug10; break;
                        case 12: _aHP._FocusedGrid = _aHP.ug12; break;
                    }
                    _aHP._FocusedGrid.Visibility = Visibility.Visible;
                    _aHP.ClearButtons();
                    if (_pd.HealthPoints > 0)
                        _aHP.SelectButton(_pd.HealthPoints.ToString());
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

        #region private void HealthPoint_Click(object sender, RoutedEventArgs e)
        private void HealthPoint_Click(object sender, RoutedEventArgs e)
        {
            if (AdvancementVM?.CurrentPowerDie != null)
            {
                var _button = sender as Button;
                AdvancementVM.HealthPoints = Convert.ToInt32(_button?.Content.ToString() ?? @"1");
                ClearButtons();
                _button.Style = (Style)FindResource(@"bsSelected");
            }
        }
        #endregion

        private void SelectButton(string total)
        {
            foreach (var _btn in _FocusedGrid.Children.OfType<Button>()
                .Where(_b => _b.Content.ToString() == total))
            {
                // select the button rolled
                _btn.Style = (Style)FindResource(@"bsSelected");
                break;
            }
        }

        #region private void RollHealth_Click(object sender, RoutedEventArgs e)
        private void RollHealth_Click(object sender, RoutedEventArgs e)
        {
            if (AdvancementVM?.CurrentPowerDie != null)
            {
                var _rollLog = AdvancementVM.Actor.Proxies.RollDice(@"Health Points", @"Dice Roll",
                    $@"1d{AdvancementVM.SelectedClass.PowerDieSize}", AdvancementVM.Actor.FulfillerID);
                AdvancementVM.HealthPoints = _rollLog.Total;
                ClearButtons();
                var _total = _rollLog.Total.ToString();
                SelectButton(_total);
            }
        }
        #endregion
    }
}
