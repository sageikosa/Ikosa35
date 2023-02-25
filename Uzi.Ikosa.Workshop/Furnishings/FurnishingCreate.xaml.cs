using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for FurnishingCreate.xaml
    /// </summary>
    public partial class FurnishingCreate : UserControl
    {
        public FurnishingCreate()
        {
            InitializeComponent();
            cboMaterial.SelectedIndex = cboMaterial.Items.Count - 1;
        }

        public bool CanCreate
        {
            get
            {
                if ((cboFurnishing.SelectedItem != null)
                    && (cboSize.SelectedItem != null)
                    && (cboMaterial.SelectedItem != null))
                {
                    return (new TextBox[]
                    {
                        txtStructure, txtWeight, txtWidth, txtHeight, txtLength
                    }).All(_txt => _txt.Tag == null);
                }
                return false;
            }
        }

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
                if (_out < 1)
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

            if (_out <= 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Cannot be zero or negative";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        public Furnishing GetFurnishing(string name)
        {
            var _weight = Convert.ToDouble(txtWeight.Text);
            var _width = Convert.ToDouble(txtWidth.Text);
            var _height = Convert.ToDouble(txtHeight.Text);
            var _length = Convert.ToDouble(txtLength.Text);
            var _struct = Convert.ToInt32(txtStructure.Text);
            if (cboFurnishing.SelectedItem is ComboBoxItem _pItem)
            {
                var _material = cboMaterial.SelectedItem as Material;
                Furnishing _obj = null;
                switch (_pItem.Content.ToString())
                {
                    case @"Altar":
                        _obj = new Altar(_material);
                        break;

                    case @"Anvil":
                        _obj = new Anvil(_material);
                        break;

                    case @"Barrel":
                        {
                            _obj = new Barrel(_material)
                            {
                                LiddedModelKey = @"Barrel-Closed"
                            };
                            var _lid = new BarrelLid(_material)
                            {
                                Width = _width,
                                Length = _length,
                                Height = 0.0825,
                                TareWeight = 1,
                                MaxStructurePoints = 5
                            };
                            _lid.BindToObject(_obj);
                        }
                        break;

                    case @"Barrel Lid":
                        _obj = new BarrelLid(_material);
                        break;

                    case @"Bed":
                        _obj = new Bed(_material);
                        break;

                    case @"Bench":
                        _obj = new Bench(_material);
                        break;

                    case @"Cabinet":
                        _obj = new Cabinet(_material);
                        break;

                    case @"Chair":
                        _obj = new Chair(_material);
                        break;

                    case @"Crate":
                        {
                            _obj = new Crate(_material);
                            var _lid = new CrateLid(_material)
                            {
                                Width = _width,
                                Length = _length,
                                Height = 0.0825,
                                TareWeight = 1,
                                MaxStructurePoints = 5
                            };
                            _lid.BindToObject(_obj);
                        }
                        break;

                    case @"Crate Lid":
                        _obj = new CrateLid(_material);
                        break;

                    case @"Desk":
                        _obj = new Desk(_material);
                        break;

                    case @"Drawers":
                        _obj = new Drawers(_material, 1);
                        break;

                    case @"Lectern":
                        _obj = new Lectern(_material);
                        break;

                    case @"Pedestal":
                        _obj = new Pedestal(_material);
                        break;

                    case @"Rack":
                        break;

                    case @"Sarcophagus":
                        // TODO: sarcophagus and lid
                        break;

                    case @"Sarcophagus Lid":
                        // TODO: sarcophagus and lid
                        break;

                    case @"Standing Shelves":
                        _obj = new StandingShelves(_material, true, true);
                        break;

                    case @"Stool":
                        _obj = new Stool(_material);
                        break;

                    case @"Table":
                        _obj = new Table(_material);
                        break;

                    default:
                        break;
                }
                if (_obj != null)
                {
                    _obj.Width = _width;
                    _obj.Height = _height;
                    _obj.Length = _length;
                    _obj.TareWeight = _weight;
                    _obj.MaxStructurePoints = _struct;
                    return _obj;
                }
            }
            return null;
        }
    }
}
