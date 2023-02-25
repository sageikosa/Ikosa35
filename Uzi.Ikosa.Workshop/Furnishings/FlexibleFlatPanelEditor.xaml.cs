using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for FlatPanelEditor.xaml
    /// </summary>
    public partial class FlexibleFlatPanelEditor : UserControl
    {
        #region ctor()
        public FlexibleFlatPanelEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(FlexibleFlatPanelEditor_DataContextChanged);
        }
        #endregion

        void FlexibleFlatPanelEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Refresh();
        }

        public FlexibleFlatPanel FlexibleFlatPanel
            => (DataContext as FlexibleFlatPanelVM)?.FlexibleFlatPanel;

        #region private void Refresh()
        private void Refresh(bool renderOnly = false)
        {
            var _flexFlat = FlexibleFlatPanel;
            if (_flexFlat != null)
            {
                txtHeight.Text = _flexFlat.Height.ToString();
                txtWidth.Text = _flexFlat.Width.ToString();
                txtLength.Text = _flexFlat.Length.ToString();
                txtWeight.Text = _flexFlat.TareWeight.ToString();
                txtStructure.Text = _flexFlat.StructurePoints.ToString();
                txtMaxStruct.Text = _flexFlat.MaxStructurePoints.ToString();
                txtSoundDifficulty.Text = _flexFlat.ExtraSoundDifficulty.BaseValue.ToString();
                cboConcealment.SelectedIndex = _flexFlat.ProvidesTotalConcealment ? 2
                    : (_flexFlat.ProvidesConcealment ? 1
                    : 0);

                // TODO: opacity

                var _omType = _flexFlat.ObjectMaterial.GetType();
                foreach (var _item in cboMaterial.Items)
                {
                    if (_item.GetType() == _omType)
                    {
                        cboMaterial.SelectedItem = _item;
                        break;
                    }
                }

                cboFlexState.SelectedItem = FlexibleFlatPanel.FlexState;
            }
        }
        #endregion

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
                FlexibleFlatPanel.MaxStructurePoints = _out;
            }
            else if (_txt.Name.Equals(@"txtStructure", StringComparison.OrdinalIgnoreCase))
            {
                FlexibleFlatPanel.StructurePoints = _out;
            }
            else if (_txt.Name.Equals(@"txtSoundDifficulty", StringComparison.OrdinalIgnoreCase))
            {
                FlexibleFlatPanel.ExtraSoundDifficulty.BaseValue = _out;
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
                FlexibleFlatPanel.Width = _out;
            }
            else if (_txt.Name.Equals(@"txtHeight", StringComparison.OrdinalIgnoreCase))
            {
                FlexibleFlatPanel.Height = _out;
            }
            else if (_txt.Name.Equals(@"txtLength", StringComparison.OrdinalIgnoreCase))
            {
                FlexibleFlatPanel.Length = _out;
            }
            else if (_txt.Name.Equals(@"txtWeight", StringComparison.OrdinalIgnoreCase))
            {
                FlexibleFlatPanel.TareWeight = _out;
            }
        }
        #endregion

        #region cboMaterial
        private void cboMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FlexibleFlatPanel.ObjectMaterial = cboMaterial.SelectedItem as Material;
        }
        #endregion

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

        #region cboConcealment
        private void cboConcealment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FlexibleFlatPanel.ProvidesConcealment = cboConcealment.SelectedIndex > 0;
            FlexibleFlatPanel.ProvidesTotalConcealment = cboConcealment.SelectedIndex > 1;
        }
        #endregion

        #region cboFlexState
        private void cboFlexState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FlexibleFlatPanel.FlexState = (FlexibleFlatState)cboFlexState.SelectedItem;
        }
        #endregion
    }
}
