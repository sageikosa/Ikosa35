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
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Items.Materials;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.UI;
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for PortalEditor.xaml
    /// </summary>
    public partial class PortalEditor : TabItem, IPackageItem, IHostedTabItem
    {
        public static RoutedCommand NewStuck = new();
        public static RoutedCommand NewSearchable = new();

        #region construction
        public PortalEditor(PresentablePortalVM portal, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = portal;

            // portalled object properties
            if (Portal.PortalledObjectA != null)
            {
                var _pA = Portal.PortalledObjectA;
                txtHeight.Text = _pA.Height.ToString();
                txtWidth.Text = _pA.Width.ToString();
                txtThickness.Text = (_pA.Thickness * 2).ToString();
                txtWeight.Text = (_pA.TareWeight * 2).ToString();
                txtStructure.Text = _pA.StructurePoints.ToString();
                txtMaxStruct.Text = _pA.MaxStructurePoints.ToString();
                txtSoundDifficulty.Text = _pA.ExtraSoundDifficulty.BaseValue.ToString();
                sldrOpacity.Value = _pA.Opacity;
                cboConcealment.SelectedIndex = _pA.DoesSupplyTotalConcealment ? 2 : (_pA.DoesSupplyConcealment ? 1 : 0);
                cboCover.SelectedIndex = _pA.DoesSupplyCover switch
                {
                    Tactical.CoverLevel.Soft or Tactical.CoverLevel.Hard => 1,
                    Tactical.CoverLevel.Improved => 2,
                    _ => 0,
                };
                cboDetectLines.SelectedIndex = _pA.DoesBlocksLineOfDetect ? 0 : 1;
                cboEffectLines.SelectedIndex = _pA.DoesBlocksLineOfEffect ? 0 : 1;
                cboMovement.SelectedIndex =
                    (_pA.BlocksMove ? 0 : (_pA.DoesHindersMove ? 1 : 2));
                cboSpread.SelectedIndex = _pA.DoesBlocksSpread ? 0 : 1;
                var _sizeOrder = _pA.Sizer.NaturalSize.Order + 4;
                try { cboSize.SelectedIndex = _sizeOrder; }
                catch { }

                var _omType = _pA.ObjectMaterial.GetType();
                foreach (var _item in cboMaterial.Items)
                {
                    if (_item.GetType() == _omType)
                    {
                        cboMaterial.SelectedItem = _item;
                        break;
                    }
                }
            }
            _Host = host;
        }
        #endregion

        private IHostTabControl _Host;

        private PresentablePortalVM PresentablePortal => DataContext as PresentablePortalVM;

        public PortalBase Portal => PresentablePortal.Thing;

        public object PackageItem => PresentablePortal;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
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

            _txt.Tag = null;
            _txt.ToolTip = null;

            if (_txt.Name.Equals(@"txtMaxStruct", StringComparison.OrdinalIgnoreCase))
            {
                Portal.PortalledObjectA.SetMaxStructurePoints(_out);
                Portal.PortalledObjectB.SetMaxStructurePoints(_out);
            }
            else if (_txt.Name.Equals(@"txtStructure", StringComparison.OrdinalIgnoreCase))
            {
                Portal.PortalledObjectA.StructurePoints = _out;
                Portal.PortalledObjectB.StructurePoints = _out;
            }
            else if (_txt.Name.Equals(@"txtSoundDifficulty", StringComparison.OrdinalIgnoreCase))
            {
                Portal.PortalledObjectA.ExtraSoundDifficulty.BaseValue = _out;
                Portal.PortalledObjectB.ExtraSoundDifficulty.BaseValue = _out;
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
                Portal.PortalledObjectA.Width = _out;
                Portal.PortalledObjectB.Width = _out;
            }
            else if (_txt.Name.Equals(@"txtHeight", StringComparison.OrdinalIgnoreCase))
            {
                Portal.PortalledObjectA.Height = _out;
                Portal.PortalledObjectB.Height = _out;
            }
            else if (_txt.Name.Equals(@"txtThickness", StringComparison.OrdinalIgnoreCase))
            {
                Portal.PortalledObjectA.Thickness = _out / 2;
                Portal.PortalledObjectB.Thickness = _out / 2;
            }
            else if (_txt.Name.Equals(@"txtWeight", StringComparison.OrdinalIgnoreCase))
            {
                Portal.PortalledObjectA.TareWeight = _out / 2;
                Portal.PortalledObjectB.TareWeight = _out / 2;
            }
        }
        #endregion

        #region shared object properties
        private void cboSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Portal.PortalledObjectA.ObjectSizer.NaturalSize = Size.Medium.OffsetSize(cboSize.SelectedIndex - 4);
            Portal.PortalledObjectB.ObjectSizer.NaturalSize = Size.Medium.OffsetSize(cboSize.SelectedIndex - 4);
        }

        private void cboMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Portal.PortalledObjectA.ObjectMaterial = cboMaterial.SelectedItem as Material;
            Portal.PortalledObjectB.ObjectMaterial = Portal.PortalledObjectA.ObjectMaterial;
        }

        private void cboCover_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboCover.SelectedIndex > 1)
            {
                Portal.PortalledObjectA.DoesSupplyCover = Tactical.CoverLevel.Improved;
                Portal.PortalledObjectB.DoesSupplyCover = Tactical.CoverLevel.Improved;
            }
            else if (cboCover.SelectedIndex > 0)
            {
                Portal.PortalledObjectA.DoesSupplyCover = Tactical.CoverLevel.Hard;
                Portal.PortalledObjectB.DoesSupplyCover = Tactical.CoverLevel.Hard;
            }
            else
            {
                Portal.PortalledObjectA.DoesSupplyCover = Tactical.CoverLevel.None;
                Portal.PortalledObjectB.DoesSupplyCover = Tactical.CoverLevel.None;
            }
        }

        private void cboConcealment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Portal.PortalledObjectA.DoesSupplyConcealment = cboConcealment.SelectedIndex > 0;
            Portal.PortalledObjectB.DoesSupplyConcealment = cboConcealment.SelectedIndex > 0;
            Portal.PortalledObjectA.DoesSupplyTotalConcealment = cboConcealment.SelectedIndex > 1;
            Portal.PortalledObjectB.DoesSupplyTotalConcealment = cboConcealment.SelectedIndex > 1;
        }

        private void sldrOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Portal.PortalledObjectA.Opacity = sldrOpacity.Value;
            Portal.PortalledObjectB.Opacity = sldrOpacity.Value;
        }

        private void cboEffectLines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Portal.PortalledObjectA.DoesBlocksLineOfEffect = cboEffectLines.SelectedIndex == 0;
            Portal.PortalledObjectB.DoesBlocksLineOfEffect = cboEffectLines.SelectedIndex == 0;
        }

        private void cboDetectLines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Portal.PortalledObjectA.DoesBlocksLineOfDetect = cboDetectLines.SelectedIndex == 0;
            Portal.PortalledObjectB.DoesBlocksLineOfDetect = cboDetectLines.SelectedIndex == 0;
        }

        private void cboMovement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Portal.PortalledObjectA.BlocksMove = cboMovement.SelectedIndex == 0;
            Portal.PortalledObjectB.BlocksMove = cboMovement.SelectedIndex == 0;
            Portal.PortalledObjectA.DoesHindersMove = cboMovement.SelectedIndex <= 1;
            Portal.PortalledObjectB.DoesHindersMove = cboMovement.SelectedIndex <= 1;
        }

        private void cboSpread_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Portal.PortalledObjectA.DoesBlocksSpread = cboSpread.SelectedIndex == 0;
            Portal.PortalledObjectB.DoesBlocksSpread = cboSpread.SelectedIndex == 0;
        }
        #endregion

        private void cmdbndDeleteAdjunct_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (lstAdjuncts != null)
            {
                e.CanExecute = (lstAdjuncts.SelectedItem != null);
                e.Handled = true;
            }
        }

        private void cmdbndDeleteAdjunct_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (lstAdjuncts.SelectedItem != null)
            {
                Portal.RemoveAdjunct(lstAdjuncts.SelectedItem as Adjunct);
            }
        }

        private void cmdbndNewStuck_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (!Portal.Adjuncts.Any(_a => _a is StuckAdjunct));
            e.Handled = true;
        }

        private void cmdbndNewStuck_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Portal.AddAdjunct(new StuckAdjunct(Portal));
        }

        private void lstAdjuncts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstAdjuncts.SelectedItem is Searchable _searchable)
            {
                var _dlg = new DeltableBaseValue(_searchable.Difficulty)
                {
                    Owner = Window.GetWindow(this)
                };
                _dlg.ShowDialog();
            }
        }

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion
    }
}
