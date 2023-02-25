using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.UI;
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for FurnishingEditor.xaml
    /// </summary>
    public partial class FurnishingEditor : UserControl
    {
        #region ctor()
        public FurnishingEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(FurnishingEditor_DataContextChanged);
        }
        #endregion

        void FurnishingEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Refresh();
        }

        #region private Mechanism ManeuverabilityMechanism { get; }
        /// <summary>Get the maneuverability mechanism for the furnishing</summary>
        private Mechanism ManeuverabilityMechanism
        {
            get
            {
                var _furnish = Furnishing;
                if (_furnish != null)
                {
                    return _furnish.Connected.OfType<RollingCasters>().Cast<Mechanism>().FirstOrDefault()
                        ?? _furnish.Connected.OfType<SteerableWheels>().Cast<Mechanism>().FirstOrDefault()
                        ?? _furnish.Connected.OfType<FixedWheels>().Cast<Mechanism>().FirstOrDefault();
                }
                return null;
            }
        }
        #endregion

        #region private void Refresh()
        private void Refresh(bool renderOnly = false)
        {
            var _furnishing = Furnishing;
            if (_furnishing != null)
            {
                txtHeight.Text = _furnishing.Height.ToString();
                txtWidth.Text = _furnishing.Width.ToString();
                txtLength.Text = _furnishing.Length.ToString();
                txtWeight.Text = _furnishing.TareWeight.ToString();
                txtStructure.Text = _furnishing.StructurePoints.ToString();
                txtMaxStruct.Text = _furnishing.MaxStructurePoints.ToString();
                txtSoundDifficulty.Text = _furnishing.ExtraSoundDifficulty.BaseValue.ToString();
                cboConcealment.SelectedIndex = _furnishing.ProvidesTotalConcealment ? 2
                    : (_furnishing.ProvidesConcealment ? 1
                    : 0);
                //switch (_furnishing.SuppliesCover)
                //{
                //    case Uzi.Ikosa.Tactical.CoverLevel.Soft:
                //    case Uzi.Ikosa.Tactical.CoverLevel.Hard:
                //        cboCover.SelectedIndex = 1;
                //        break;
                //    case Uzi.Ikosa.Tactical.CoverLevel.Improved:
                //        cboCover.SelectedIndex = 2;
                //        break;
                //    default:
                //        cboCover.SelectedIndex = 0;
                //        break;
                //}

                // TODO: wheel selection

                var _mech = ManeuverabilityMechanism;
                if (_mech != null)
                {
                    switch (_mech)
                    {
                        case RollingCasters _caster:
                            cboWheels.SelectedIndex = 3;
                            break;

                        case SteerableWheels _steerable:
                            cboWheels.SelectedIndex = 2;
                            break;

                        case FixedWheels _fixed:
                            cboWheels.SelectedIndex = 1;
                            break;

                        default:
                            break;
                    }
                    txtWheelDisable.Text = _mech.DisableDifficulty.BaseValue.ToString();
                    txtWheelDisable.IsEnabled = true;
                }
                else
                {
                    cboWheels.SelectedIndex = 0;
                    txtWheelDisable.IsEnabled = false;
                }

                // TODO: opacity

                var _omType = _furnishing.ObjectMaterial.GetType();
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

        public Furnishing Furnishing
            => (DataContext as FurnishingVM)?.Furnishing;

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
                Furnishing.MaxStructurePoints = _out;
            }
            else if (_txt.Name.Equals(nameof(txtStructure), StringComparison.OrdinalIgnoreCase))
            {
                Furnishing.StructurePoints = _out;
            }
            else if (_txt.Name.Equals(nameof(txtSoundDifficulty), StringComparison.OrdinalIgnoreCase))
            {
                Furnishing.ExtraSoundDifficulty.BaseValue = _out;
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
                Furnishing.Width = _out;
            }
            else if (_txt.Name.Equals(nameof(txtHeight), StringComparison.OrdinalIgnoreCase))
            {
                Furnishing.Height = _out;
            }
            else if (_txt.Name.Equals(nameof(txtLength), StringComparison.OrdinalIgnoreCase))
            {
                Furnishing.Length = _out;
            }
            else if (_txt.Name.Equals(nameof(txtWeight), StringComparison.OrdinalIgnoreCase))
            {
                Furnishing.TareWeight = _out;
            }
        }
        #endregion

        #region object properties
        private void cboMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Furnishing.ObjectMaterial = cboMaterial.SelectedItem as Material;
        }

        private void cboCover_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cboCover.SelectedIndex > 1)
            //{
            //    Furnishing.SuppliesCover = Uzi.Ikosa.Tactical.CoverLevel.Improved;
            //}
            //else if (cboCover.SelectedIndex > 0)
            //{
            //    Furnishing.SuppliesCover = Uzi.Ikosa.Tactical.CoverLevel.Hard;
            //}
            //else
            //{
            //    Furnishing.SuppliesCover = Uzi.Ikosa.Tactical.CoverLevel.None;
            //}
        }

        private void cboConcealment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Furnishing.ProvidesConcealment = cboConcealment.SelectedIndex > 0;
            Furnishing.ProvidesTotalConcealment = cboConcealment.SelectedIndex > 1;
        }
        #endregion

        #region cboWheels_SelectionChanged
        private void cboWheels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Furnishing != null)
            {
                // current mechanism if any
                var _mech = ManeuverabilityMechanism;
                void _unbind()
                {
                    // remove any existing mechanism
                    if (_mech != null)
                    {
                        _mech.UnbindFromObject(Furnishing);
                    }
                }

                switch (cboWheels.SelectedIndex)
                {
                    case 1:
                        // fixed wheels
                        if (!(_mech is FixedWheels))
                        {
                            _unbind();
                            (new FixedWheels(@"Fixed Wheels", SteelMaterial.Static, 20)).BindToObject(Furnishing);
                            Refresh();
                        }
                        break;

                    case 2:
                        // fixed wheels
                        if (!(_mech is SteerableWheels))
                        {
                            _unbind();
                            (new SteerableWheels(@"Steerable Wheels", SteelMaterial.Static, 20)).BindToObject(Furnishing);
                            Refresh();
                        }
                        break;

                    case 3:
                        // fixed wheels
                        if (!(_mech is RollingCasters))
                        {
                            _unbind();
                            (new RollingCasters(@"Rolling Casters", SteelMaterial.Static, 20)).BindToObject(Furnishing);
                            Refresh();
                        }
                        break;

                    case 0:
                    default:
                        // none
                        _unbind();
                        Refresh();
                        break;
                }
            }
            e.Handled = true;
        }
        #endregion
    }
}
