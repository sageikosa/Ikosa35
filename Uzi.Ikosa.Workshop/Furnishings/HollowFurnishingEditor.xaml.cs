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
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for HollowFurnishingEditor.xaml
    /// </summary>
    public partial class HollowFurnishingEditor : UserControl
    {
        public HollowFurnishingEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(HollowFurnishingEditor_DataContextChanged);
        }

        void HollowFurnishingEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Refresh();
        }

        #region private void Refresh()
        private void Refresh(bool renderOnly = false)
        {
            var _furnishing = HollowFurnishing;
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

        public HollowFurnishing HollowFurnishing
            => (DataContext as HollowFurnishingVM)?.HollowFurnishing;

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

            if (_txt.Name.Equals(@"txtMaxStruct", StringComparison.OrdinalIgnoreCase))
            {
                HollowFurnishing.MaxStructurePoints = _out;
            }
            else if (_txt.Name.Equals(@"txtStructure", StringComparison.OrdinalIgnoreCase))
            {
                HollowFurnishing.StructurePoints = _out;
            }
            else if (_txt.Name.Equals(@"txtSoundDifficulty", StringComparison.OrdinalIgnoreCase))
            {
                HollowFurnishing.ExtraSoundDifficulty.BaseValue = _out;
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

            if (_txt.Name.Equals(@"txtWidth", StringComparison.OrdinalIgnoreCase))
            {
                HollowFurnishing.Width = _out;
            }
            else if (_txt.Name.Equals(@"txtHeight", StringComparison.OrdinalIgnoreCase))
            {
                HollowFurnishing.Height = _out;
            }
            else if (_txt.Name.Equals(@"txtLength", StringComparison.OrdinalIgnoreCase))
            {
                HollowFurnishing.Length = _out;
            }
            else if (_txt.Name.Equals(@"txtWeight", StringComparison.OrdinalIgnoreCase))
            {
                HollowFurnishing.TareWeight = _out;
            }
        }
        #endregion

        #region object properties
        private void cboMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HollowFurnishing.ObjectMaterial = cboMaterial.SelectedItem as Material;
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
            HollowFurnishing.ProvidesConcealment = cboConcealment.SelectedIndex > 0;
            HollowFurnishing.ProvidesTotalConcealment = cboConcealment.SelectedIndex > 1;
        }
        #endregion
    }
}
