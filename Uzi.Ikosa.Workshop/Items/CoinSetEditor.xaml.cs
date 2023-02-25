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
using Uzi.Ikosa.Items.Wealth;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for CoinSetEditor.xaml
    /// </summary>
    public partial class CoinSetEditor : UserControl
    {
        public static RoutedCommand NewCoins = new RoutedCommand();
        public static RoutedCommand DeleteCoins = new RoutedCommand();
        public CoinSetEditor()
        {
            InitializeComponent();
        }

        private CoinSet _Coins => (DataContext as CoinSetVM)?.Thing;

        #region Int32 text field validation
        private void txtInt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            int _out = 0;
            if (!int.TryParse(_txt.Text, out _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            if (_out < 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Negatives not allowed";
                _txt.Text = 0.ToString();
                return;
            }

            var _coinCount = (CoinCount)_txt.DataContext;
            _Coins[_coinCount.CoinType] = _out;
            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        #region private void cmdbndNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cmdbndNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion

        #region private void cmdbndNew_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cmdbndNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            switch (e.Parameter.ToString())
            {
                case @"GoldPiece":
                    _Coins[GoldPiece.Static] = 100;
                    break;
                case @"SilverPiece":
                    _Coins[SilverPiece.Static] = 100;
                    break;
                case @"CopperPiece":
                    _Coins[CopperPiece.Static] = 100;
                    break;
                case @"PlatinumPiece":
                    _Coins[PlatinumPiece.Static] = 100;
                    break;
            }
            lstCoins.ItemsSource = null;
            lstCoins.ItemsSource = _Coins.Coins;
            e.Handled = true;
        }
        #endregion

        #region private void cmdbndDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cmdbndDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (lstCoins != null)
            {
                e.CanExecute = lstCoins.SelectedItem != null;
            }
            e.Handled = true;
        }
        #endregion

        #region private void cmdbndDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cmdbndDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (lstCoins.SelectedItem != null)
            {
                var _cc = (CoinCount)lstCoins.SelectedItem;
                _Coins[_cc.CoinType] = 0;
                lstCoins.ItemsSource = null;
                lstCoins.ItemsSource = _Coins.Coins;
            }
            e.Handled = true;
        }
        #endregion
    }
}
