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
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for LightCreate.xaml
    /// </summary>
    public partial class LightCreate : UserControl
    {
        public LightCreate()
        {
            InitializeComponent();
        }

        public bool CanCreate
        {
            get
            {
                if (cboMaterial.SelectedItem != null)
                    return (new TextBox[]
                    {
                        txtStructure, txtWeight, txtLightDifficulty,
                        txtLightXOffset, txtLightYOffset, txtLightZOffset,
                        txtBright, txtShadow, txtWeight
                    }).All(_txt => _txt.Tag == null);
                return false;
            }
        }

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
            double _out = 0;
            if (!double.TryParse(_txt.Text, out _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            if (_txt.Name.StartsWith(@"txtBright"))
            {
                if (_out < 0)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Cannot be negative";
                    return;
                }
            }
            else if (!_txt.Name.StartsWith(@"txtLight"))
            {
                if (_out <= 0)
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

        public CoreObject GetLight(string name)
        {
            Func<TextBox, double> _convert =
                (txtBox) =>
                {
                    double _out = 0;
                    double.TryParse(txtBox.Text, out _out);
                    return _out;
                };
            // shared
            double _lxo = _convert(txtLightXOffset);
            double _lyo = _convert(txtLightYOffset);
            double _lzo = _convert(txtLightZOffset);
            double _bright = Convert.ToDouble(txtBright.Text);
            double _shadow = Convert.ToDouble(txtShadow.Text);
            double _weight = Convert.ToDouble(txtWeight.Text);

            int _struct = Convert.ToInt32(txtStructure.Text);
            int _diff = Convert.ToInt32(txtLightDifficulty.Text);

            // structural light
            var _illum = new Illumination(typeof(StructuralLight), _bright, _shadow, chkVeryBright.IsChecked ?? false);
            _illum.ZOffset = _lzo;
            _illum.YOffset = _lyo;
            _illum.XOffset = _lxo;
            return new StructuralLight(name, chkLightVisible.IsChecked ?? false, _illum, true, _diff);
        }
    }
}
