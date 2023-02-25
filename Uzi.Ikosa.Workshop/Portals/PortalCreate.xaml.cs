using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Items.Materials;
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for PortalCreate.xaml
    /// </summary>
    public partial class PortalCreate : UserControl
    {
        public PortalCreate()
        {
            InitializeComponent();
        }

        public bool CanCreate
        {
            get
            {
                if ((cboPortal.SelectedItem != null)
                    && (cboPortalObject.SelectedItem != null)
                    && (cboMaterial.SelectedItem != null))
                {
                    return (new TextBox[]
                    {
                        txtStructure, txtWeight, txtWidth, txtHeight, txtThickness
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

        public PortalBase GetPortal(string name)
        {
            var _weight = Convert.ToDouble(txtWeight.Text);
            var _width = Convert.ToDouble(txtWidth.Text);
            var _height = Convert.ToDouble(txtHeight.Text);
            var _thick = Convert.ToDouble(txtThickness.Text);
            var _struct = Convert.ToInt32(txtStructure.Text);
            PortalledObjectBase _pObjA = null;
            PortalledObjectBase _pObjB = null;
            if (cboPortalObject.SelectedItem is ComboBoxItem _pItem)
            {
                if (_pItem.Content.ToString().Equals(@"Door", StringComparison.OrdinalIgnoreCase))
                {
                    _pObjA = new Door(@"Outside", cboMaterial.SelectedItem as Material, _thick / 2);
                    _pObjB = new Door(@"Inside", cboMaterial.SelectedItem as Material, _thick / 2);
                }
                else if (_pItem.Content.ToString().Equals(@"Bars", StringComparison.OrdinalIgnoreCase))
                {
                    _pObjA = new Bars(@"Outside", cboMaterial.SelectedItem as Material, _thick / 2);
                    _pObjB = new Bars(@"Inside", cboMaterial.SelectedItem as Material, _thick / 2);
                }

                if ((_pObjA != null) && (_pObjB != null))
                {
                    _pObjA.Width = _width;
                    _pObjA.Height = _height;
                    _pObjA.TareWeight = _weight / 2;
                    _pObjA.MaxStructurePoints = _struct;
                    _pObjB.Width = _width;
                    _pObjB.Height = _height;
                    _pObjB.TareWeight = _weight / 2;
                    _pObjB.MaxStructurePoints = _struct;
                    PortalBase _portal = null;
                    if (cboPortal.SelectedItem is ComboBoxItem _item)
                    {
                        if (_item.Content.ToString().Equals(@"Corner Pivot", StringComparison.OrdinalIgnoreCase))
                        {
                            _portal = new CornerPivotPortal(@"Hinged Door", AnchorFace.YLow, AnchorFace.XLow, _pObjA, _pObjB);
                        }
                        else if (_item.Content.ToString().Equals(@"Sliding", StringComparison.OrdinalIgnoreCase))
                        {
                            _portal = new SlidingPortal(@"Sliding Door", AnchorFace.YLow, Axis.X, -5, 0, 0, 0, _pObjA, _pObjB);
                        }

                        return _portal;
                    }
                }
            }
            return null;
        }
    }
}
