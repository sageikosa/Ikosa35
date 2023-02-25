using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;
using Uzi.Core.Dice;
using System.Collections;
using Uzi.Ikosa.TypeListers;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Fidelity;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for LocatorCreate.xaml
    /// </summary>
    public partial class LocatorCreate : Window
    {
        public static RoutedCommand OKCommand = new RoutedCommand();

        public LocatorCreate(LocalMap map)
        {
            InitializeComponent();
            _Map = map;
            DataContext = _Map;
            txtZHeight.Text = @"1";
            txtYLength.Text = @"1";
            txtXLength.Text = @"1";
        }

        public LocatorCreate(LocalMap map, ICellLocation cellPosition)
        {
            InitializeComponent();
            _Map = map;
            DataContext = _Map;
            txtZ.Text = cellPosition.Z.ToString();
            txtY.Text = cellPosition.Y.ToString();
            txtX.Text = cellPosition.X.ToString();
            txtZ.IsEnabled = false;
            txtY.IsEnabled = false;
            txtX.IsEnabled = false;
        }

        private LocalMap _Map;
        private string _TagString;

        #region Int32 text field validation
        private void txtInt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            if (!int.TryParse(_txt.Text, out var _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            // exclude txtZ, txtY and txtX
            if (_txt.Name.Length > 4)
            {
                if (_txt.Name.StartsWith(@"txtLight"))
                {
                    if (_out < 0)
                    {
                        _txt.Tag = @"Invalid";
                        _txt.ToolTip = @"Cannot be negative";
                    }
                }
                else if (_out < 1)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Cannot be zero or negative";
                    return;
                }
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        #region Double text field validation
        private void txtDbl_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            if (!double.TryParse(_txt.Text, out var _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            if (_txt.Name.StartsWith(@"txtLight"))
            {
                if (_out < 0)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Cannot be negative";
                }
            }
            else if (_out <= 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Cannot be zero or negative";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        #region public Locator GetLocator()
        public Locator GetLocator(MapContext context)
        {
            var _z = Convert.ToInt32(txtZ.Text);
            var _y = Convert.ToInt32(txtY.Text);
            var _x = Convert.ToInt32(txtX.Text);
            var _zH = Convert.ToInt32(txtZHeight.Text);
            var _yL = Convert.ToInt32(txtYLength.Text);
            var _xL = Convert.ToInt32(txtXLength.Text);
            var _key = cboModelKey.SelectedValue?.ToString();
            var _size = new GeometricSize(_zH, _yL, _xL);
            var _cube = new Cubic(new CellPosition(_z, _y, _x), _zH, _yL, _xL);

            if (_TagString.Equals(@"Portal", StringComparison.OrdinalIgnoreCase))
            {
                #region Portal
                var _portal = createPortal.GetPortal(txtName.Text);
                if (_portal != null)
                {
                    var _presenter = new ObjectPresenter(_portal, context, _key, _size, _cube);
                    return _presenter;
                }
                #endregion
            }
            else if (_TagString.Equals(@"Furnishing", StringComparison.OrdinalIgnoreCase))
            {
                #region Furnishing
                var _furnishing = createFurnishing.GetFurnishing(txtName.Text);
                if (_furnishing != null)
                {
                    var _presenter = new ObjectPresenter(_furnishing, context, _key, _size, _cube);
                    return _presenter;
                }
                #endregion
            }
            else if (_TagString.Equals(@"Container", StringComparison.OrdinalIgnoreCase))
            {
                #region container
                var _container = createContainer.GetContainer(txtName.Text);
                var _presenter = new ObjectPresenter(_container, context, _key, _size, _cube);
                return _presenter;
                #endregion
            }
            else if (_TagString.Equals(@"Light", StringComparison.OrdinalIgnoreCase))
            {
                #region light
                var _light = createLight.GetLight(txtName.Text);
                var _present = new ObjectPresenter(_light, context, _key, _size, _cube);
                return _present;
                #endregion
            }
            else if (_TagString.Equals(@"Creature", StringComparison.OrdinalIgnoreCase))
            {
                #region Creature
                var _critter = createCreature.GetCreature(txtName.Text);
                var _presenter = new ObjectPresenter(_critter, context, _key, _size, _cube);
                return _presenter;
                #endregion
            }
            else if (_TagString.Equals(@"Item", StringComparison.OrdinalIgnoreCase))
            {
                #region Item Trove
                var _trove = new Trove(txtName.Text);
                var _present = new ObjectPresenter(_trove, context, null, _size, _cube);
                return _present;
                #endregion
            }
            return null;
        }
        #endregion

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
            e.Handled = true;
        }

        private void cbOKCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((txtName != null)
                && (txtName.Text.Trim() != string.Empty)
                && (new TextBox[] {
                            txtZ, txtY, txtX, txtZHeight, txtYLength, txtXLength
                        }).All(_txt => _txt.Tag == null))
                if (tiCreature.IsSelected)
                    e.CanExecute = createCreature.CanCreate;
                else if (tiPortal.IsSelected)
                    e.CanExecute = createPortal.CanCreate;
                else if (tiFurnishing.IsSelected)
                    e.CanExecute = createFurnishing.CanCreate;
                else if (tiContainer.IsSelected)
                    e.CanExecute = createContainer.CanCreate;
                else if (tiLight.IsSelected)
                    e.CanExecute = createLight.CanCreate;
                else if (tiItem.IsSelected)
                    e.CanExecute = true;
            e.Handled = true;
        }

        private void cbOKCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (tiCreature.IsSelected)
                _TagString = @"Creature";
            else if (tiPortal.IsSelected)
                _TagString = @"Portal";
            else if (tiFurnishing.IsSelected)
                _TagString = @"Furnishing";
            else if (tiContainer.IsSelected)
                _TagString = @"Container";
            else if (tiLight.IsSelected)
                _TagString = @"Light";
            else if (tiItem.IsSelected)
                _TagString = @"Item";

            // all of the above
            DialogResult = true;
            Close();
            e.Handled = true;
        }
    }
}
