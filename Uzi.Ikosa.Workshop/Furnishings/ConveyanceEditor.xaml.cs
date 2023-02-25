using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ConveyanceEditor.xaml
    /// </summary>
    public partial class ConveyanceEditor : UserControl
    {
        public ConveyanceEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(ConveyanceEditor_DataContextChanged);
        }

        void ConveyanceEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Refresh();
        }

        #region private Mechanism ManeuverabilityMechanism
        /// <summary>Get the maneuverability mechanism for the conveyance</summary>
        private Mechanism ManeuverabilityMechanism
        {
            get
            {
                var _convey = Conveyance;
                if (_convey != null)
                {
                    return _convey.Connected.OfType<RollingCasters>().Cast<Mechanism>().FirstOrDefault()
                        ?? _convey.Connected.OfType<SteerableWheels>().Cast<Mechanism>().FirstOrDefault()
                        ?? _convey.Connected.OfType<FixedWheels>().Cast<Mechanism>().FirstOrDefault();
                }
                return null;
            }
        }
        #endregion

        #region private void Refresh()
        private void Refresh(bool renderOnly = false)
        {
            var _convey = Conveyance;
            if (_convey != null)
            {
                txtHeight.Text = _convey.Height.ToString();
                txtWidth.Text = _convey.Width.ToString();
                txtLength.Text = _convey.Length.ToString();
                txtWeight.Text = _convey.TareWeight.ToString();
                txtStructure.Text = _convey.StructurePoints.ToString();
                txtMaxStruct.Text = _convey.MaxStructurePoints.ToString();
                txtSoundDifficulty.Text = _convey.ExtraSoundDifficulty.BaseValue.ToString();
                cboConcealment.SelectedIndex = _convey.DoesSupplyTotalConcealment ? 2
                    : (_convey.DoesSupplyConcealment ? 1
                    : 0);

                var _mech = ManeuverabilityMechanism;
                if (_mech != null)
                {
                    txtWheelDisable.Text = _mech.DisableDifficulty.BaseValue.ToString();
                    txtWheelDisable.IsEnabled = true;
                }
                else
                {
                    txtWheelDisable.IsEnabled = false;
                }

                // TODO: opacity

                var _omType = _convey.ObjectMaterial.GetType();
                foreach (var _item in cboMaterial.Items)
                {
                    if (_item.GetType() == _omType)
                    {
                        cboMaterial.SelectedItem = _item;
                        break;
                    }
                }
            }
        }
        #endregion

        public Conveyance Conveyance
            => (DataContext as ConveyanceVM)?.Conveyance;

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

            _txt.Tag = null;
            _txt.ToolTip = null;

            if (_txt.Name.Equals(nameof(txtMaxStruct), StringComparison.OrdinalIgnoreCase))
            {
                Conveyance.MaxStructurePoints = _out;
            }
            else if (_txt.Name.Equals(nameof(txtStructure), StringComparison.OrdinalIgnoreCase))
            {
                Conveyance.StructurePoints = _out;
            }
            else if (_txt.Name.Equals(nameof(txtSoundDifficulty), StringComparison.OrdinalIgnoreCase))
            {
                Conveyance.ExtraSoundDifficulty.BaseValue = _out;
            }
            else if (_txt.Name.Equals(nameof(txtWheelDisable), StringComparison.OrdinalIgnoreCase))
            {

                var _mech = ManeuverabilityMechanism;
                if (_mech != null)
                {
                    _mech.DisableDifficulty.BaseValue = _out;
                }
            }
        }
        #endregion

        #region Size double text field validation
        private void txtDblSize_TextChanged(object sender, TextChangedEventArgs e)
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
                _txt.ToolTip = @"Negatives not allowed for Dimensions";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;

            if (_txt.Name.Equals(nameof(txtWidth), StringComparison.OrdinalIgnoreCase))
            {
                Conveyance.Width = _out;
            }
            else if (_txt.Name.Equals(nameof(txtHeight), StringComparison.OrdinalIgnoreCase))
            {
                Conveyance.Height = _out;
            }
            else if (_txt.Name.Equals(nameof(txtLength), StringComparison.OrdinalIgnoreCase))
            {
                Conveyance.Length = _out;
            }
            else if (_txt.Name.Equals(nameof(txtWeight), StringComparison.OrdinalIgnoreCase))
            {
                Conveyance.TareWeight = _out;
            }
        }
        #endregion

        #region object properties
        private void cboMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Conveyance.ObjectMaterial = cboMaterial.SelectedItem as Material;
        }

        private void cboConcealment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Conveyance.DoesSupplyConcealment = cboConcealment.SelectedIndex > 0;
            Conveyance.DoesSupplyTotalConcealment = cboConcealment.SelectedIndex > 1;
        }
        #endregion
    }
}
