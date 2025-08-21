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
using Uzi.Ikosa.Tactical;
using System.Collections.ObjectModel;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for BackgroundCellGroupCreate.xaml
    /// </summary>
    public partial class BackgroundCellGroupCreate : Window
    {
        public BackgroundCellGroupCreate(LocalMap map)
        {
            InitializeComponent();
            DataContext = map;

            // positions
            var _index = 0;
            foreach (var _current in _Map.Backgrounds.All())
            {
                cboAddPosition.Items.Add(new ComboBoxItem { Content = string.Format(@"Before {0}", _current.Name), Tag = _index });
                _index++;
            }
            cboAddPosition.Items.Add(new ComboBoxItem { Content = @"As Base Material", Tag = -1 });

            if (cboAddPosition.Items.Count > 0)
            {
                cboAddPosition.SelectedIndex = 0;
            }
        }

        private LocalMap _Map { get { return DataContext as LocalMap; } }

        public BackgroundCellGroup CreateGroup()
        {
            // values
            if (!TryParseInt(cboMinZ.Text, out var _zLo) || !TryParseInt(cboMaxZ.Text, out var _zHi)
                || !TryParseInt(cboMinY.Text, out var _yLo) || !TryParseInt(cboMaxY.Text, out var _yHi)
                || !TryParseInt(cboMinX.Text, out var _xLo) || !TryParseInt(cboMaxX.Text, out var _xHi))
            {
                MessageBox.Show(@"Invalid values for geometry");
                return null;
            }

            var _loc = new CellLocation(_zLo, _yLo, _xLo);
            var _size = new GeometricSize((double)_zHi - (double)_zLo + 1, (double)_yHi - (double)_yLo + 1, (double)_xHi - (double)_xLo + 1);
            return new BackgroundCellGroup(new CellStructure(
                cboCellSpace.SelectedItem as CellSpace,
                (cntCellSpace.Content != null) ? (cntCellSpace.Content as IParamControl).ParamData : 0),
                _loc, _size, _Map, txtName.Text, (bool)chkDeepShadows.IsChecked);
        }

        public int TargetIndex
        {
            get
            {
                try
                {
                    return Convert.ToInt32((cboAddPosition.SelectedItem as ComboBoxItem).Tag);
                }
                catch (Exception)
                {
                    return -1;
                }
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
        }


        private bool TryParseInt(string source, out int result)
        {
            if (!int.TryParse(source, out result))
            {
                if (source.Equals(@"Minimum", StringComparison.OrdinalIgnoreCase))
                {
                    result = int.MinValue;
                }
                else if (source.Equals(@"Maximum", StringComparison.OrdinalIgnoreCase))
                {
                    result = int.MaxValue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                // name
                if (string.IsNullOrEmpty(txtName.Text))
                {
                    txtMessage.Text = @"Name is empty";
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }
                if (_Map.Backgrounds.All().Any(_bg => _bg.Name.Equals(txtName.Text, StringComparison.OrdinalIgnoreCase)))
                {
                    txtMessage.Text = @"Name is not unique";
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }

                // values
                int _zLo = 0;
                int _zHi = 0;
                int _yLo = 0;
                int _yHi = 0;
                int _xLo = 0;
                int _xHi = 0;
                if (!TryParseInt(cboMinZ.Text, out _zLo) || !TryParseInt(cboMaxZ.Text, out _zHi)
                    || !TryParseInt(cboMinY.Text, out _yLo) || !TryParseInt(cboMaxY.Text, out _yHi)
                    || !TryParseInt(cboMinX.Text, out _xLo) || !TryParseInt(cboMaxX.Text, out _xHi))
                {
                    txtMessage.Text = @"One or more invalid values";
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }

                // ranges
                if ((_zHi < _zLo) || (_yHi < _yLo) || (_xHi < _xLo))
                {
                    txtMessage.Text = @"One or more invalid ranges";
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }

                if (cboCellSpace.SelectedItem == null)
                {
                    txtMessage.Text = @"Must select a cell space template";
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }

                // all clear
                txtMessage.Text = string.Empty;
                e.CanExecute = true;
                e.Handled = true;
            }
            catch
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }

        private void cmdbndOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void cboMin_LostFocus(object sender, RoutedEventArgs e)
        {
            var _cbo = sender as ComboBox;
            if (!_cbo.Text.Equals(@"Minimum", StringComparison.OrdinalIgnoreCase))
            {
                int _out = 0;
                if (!int.TryParse(_cbo.Text, out _out))
                {
                    _cbo.Tag = @"Invalid";
                    return;
                }
            }
            _cbo.Tag = null;
        }

        private void cboMax_LostFocus(object sender, RoutedEventArgs e)
        {
            var _cbo = sender as ComboBox;
            if (!_cbo.Text.Equals(@"Maximum", StringComparison.OrdinalIgnoreCase))
            {
                int _out = 0;
                if (!int.TryParse(_cbo.Text, out _out))
                {
                    _cbo.Tag = @"Invalid";
                    return;
                }
            }
            _cbo.Tag = null;
        }

        private void cboCellSpace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.cntCellSpace.Content = ParamPicker.GetParamControl(cboCellSpace.SelectedItem as CellSpace);
        }
    }
}
