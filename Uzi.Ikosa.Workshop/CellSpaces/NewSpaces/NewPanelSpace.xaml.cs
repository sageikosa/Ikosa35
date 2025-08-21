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

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for NewPanelSpace.xaml
    /// </summary>
    public partial class NewPanelSpace : Window
    {
        public NewPanelSpace(LocalMap map)
        {
            InitializeComponent();
            _Map = map;
            this.Resources.Add(@"roomFillMaterials", map.AllCellMaterials);
            this.Resources.Add(@"roomDiagonalMaterials", map.AllCellMaterials.OfType<SolidCellMaterial>());
            cboPanel.ItemsSource = map.Panels.All().OfType<NormalPanel>();
            cboSlope.ItemsSource = map.Panels.All().OfType<SlopeComposite>();
            cboCorner.ItemsSource = map.Panels.All().OfType<CornerPanel>();
            cboLFrame.ItemsSource = map.Panels.All().OfType<LFramePanel>();
        }

        private LocalMap _Map;

        public PanelCellSpace GetPanelCellSpace(CellSpace parent)
        {
            var _panel = new PanelCellSpace(cboMaterial.SelectedItem as CellMaterial, cboTiling.SelectedItem as TileSet,
                cboDiagonalMaterial.SelectedItem as SolidCellMaterial, cboDiagonalTiling.SelectedItem as TileSet);
            _panel.Name = txtName.Text;
            _panel.Parent = parent;
            if (cboPanel.SelectedItem != null)
            {
                foreach (var _f in PanelCellSpace.AllFaces)
                {
                    _panel.Panel1s[_f] = cboPanel.SelectedItem as NormalPanel;
                    _panel.Panel2s[_f] = cboPanel.SelectedItem as NormalPanel;
                    _panel.Panel3s[_f] = cboPanel.SelectedItem as NormalPanel;
                }
            }
            if (cboSlope.SelectedItem != null)
            {
                _panel.Slopes.Add(cboSlope.SelectedItem as SlopeComposite);
            }
            if (cboCorner.SelectedItem != null)
            {
                foreach (var _f in PanelCellSpace.AllFaces)
                {
                    _panel.Corners[_f] = cboCorner.SelectedItem as CornerPanel;
                }
            }
            if (cboLFrame.SelectedItem != null)
            {
                foreach (var _f in PanelCellSpace.AllFaces)
                {
                    _panel.LFrames[_f] = cboLFrame.SelectedItem as LFramePanel;
                }
            }
            return _panel;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void cmdbndOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // values
            if (string.IsNullOrEmpty(txtName.Text) || (cboMaterial.SelectedItem == null) || (cboDiagonalMaterial.SelectedItem == null))
            {
                txtMessage.Text = @"Missing name and/or material";
                e.CanExecute = false;
                e.Handled = true;
                return;
            }

            if ((cboPanel.SelectedItem == null) || (cboSlope.SelectedItem == null)
                || (cboCorner.SelectedItem == null) || (cboLFrame.SelectedItem == null))
            {
                txtMessage.Text = @"Missing panel selection";
                e.CanExecute = false;
                e.Handled = true;
                return;
            }

            txtMessage.Text = string.Empty;
            e.CanExecute = _Map.CanUseName(txtName.Text, typeof(CellSpace));
            e.Handled = true;
        }
    }
}
