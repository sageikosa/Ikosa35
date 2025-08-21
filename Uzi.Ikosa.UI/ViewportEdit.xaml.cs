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
using System.Windows.Shapes;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.UI
{
    /// <summary>
    /// Interaction logic for ViewportEdit.xaml
    /// </summary>
    public partial class ViewportEdit : Window
    {
        public static RoutedCommand OKCommand = new RoutedCommand();

        public ViewportEdit(Cubic cubic)
        {
            InitializeComponent();
            txtXPos.Text = cubic.X.ToString();
            txtYPos.Text = cubic.Y.ToString();
            txtZPos.Text = cubic.Z.ToString();
            txtXSize.Text = cubic.XLength.ToString();
            txtYSize.Text = cubic.YLength.ToString();
            txtZSize.Text = cubic.ZHeight.ToString();
        }

        public Cubic GetCubic()
        {
            var _z = Convert.ToInt32(txtZPos.Text);
            var _y = Convert.ToInt32(txtYPos.Text);
            var _x = Convert.ToInt32(txtXPos.Text);
            var _zex = Convert.ToInt64(txtZSize.Text);
            var _yex = Convert.ToInt64(txtYSize.Text);
            var _xex = Convert.ToInt64(txtXSize.Text);
            return new Cubic(new CellLocation(_z, _y, _x), _zex, _yex, _xex);
        }

        #region position text field validation
        private void txtPos_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            int _out = 0;
            if (!int.TryParse(_txt.Text, out _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }
            else if (int.MaxValue - 200 < _out)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Position too close to Max (must be 200 or more less than Max)";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        #region size text field validation
        private void txtSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            long _out = 0;
            if (!long.TryParse(_txt.Text, out _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }
            else if (_out > 120)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Cannot exceed 120 cells";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            Func<TextBox, bool> _notInvalid = (txtBox) =>
                {
                    if (txtBox != null)
                    {
                        if (txtBox.Tag != null)
                        {
                            if (txtBox.Tag.ToString() == @"Invalid")
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                };
            if (txtXPos != null)
            {
                e.CanExecute = _notInvalid(txtXPos) && _notInvalid(txtYPos) && _notInvalid(txtZPos)
                     && _notInvalid(txtXSize) && _notInvalid(txtYSize) && _notInvalid(txtZSize);
            }
            e.Handled = true;
        }

        private void cmdbndOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // done
            this.DialogResult = true;
            this.Close();
            e.Handled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

    }
}
