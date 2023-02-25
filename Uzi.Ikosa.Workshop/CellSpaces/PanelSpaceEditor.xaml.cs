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
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for PanelSpaceEditor.xaml
    /// </summary>
    public partial class PanelSpaceEditor : UserControl
    {
        public PanelSpaceEditor()
        {
            InitializeComponent();
        }

        public LocalMap Map
        {
            get { return (LocalMap)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

        private PanelCellSpace _Panels => DataContext as PanelCellSpace;

        // Using a DependencyProperty as the backing store for Map.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register(@"Map", typeof(LocalMap), typeof(PanelSpaceEditor),
            new UIPropertyMetadata(null, new PropertyChangedCallback(LocalMapChanged)));

        private static void LocalMapChanged(DependencyObject depends, DependencyPropertyChangedEventArgs args)
        {
            var _editor = depends as PanelSpaceEditor;
            if (_editor.Map != null)
            {
                _editor.Resources.Add(@"normalPanels", _editor.Map.Panels.All().OfType<NormalPanel>());
                _editor.Resources.Add(@"cornerPanels", _editor.Map.Panels.All().OfType<CornerPanel>());
                _editor.Resources.Add(@"lFramePanels", _editor.Map.Panels.All().OfType<LFramePanel>());
                _editor.Resources.Add(@"slopePanels", _editor.Map.Panels.All().OfType<SlopeComposite>());
                _editor.Resources.Add(@"fillMaterials", _editor.Map.AllCellMaterials);
                _editor.Resources.Add(@"materialPanels", _editor.Map.Panels.All().OfType<MaterialFill>());
                _editor.Resources.Add(@"diagonalPanels", _editor.Map.Panels.All().OfType<DiagonalComposite>());
            }
            else
            {
                if (_editor.Resources.Contains(@"normalPanels")) _editor.Resources.Remove(@"normalPanels");
                if (_editor.Resources.Contains(@"cornerPanels")) _editor.Resources.Remove(@"cornerPanels");
                if (_editor.Resources.Contains(@"lFramePanels")) _editor.Resources.Remove(@"lFramePanels");
                if (_editor.Resources.Contains(@"slopePanels")) _editor.Resources.Remove(@"slopePanels");
                if (_editor.Resources.Contains(@"fillMaterials")) _editor.Resources.Remove(@"fillMaterials");
                if (_editor.Resources.Contains(@"materialPanels")) _editor.Resources.Remove(@"materialPanels");
                if (_editor.Resources.Contains(@"diagonalPanels")) _editor.Resources.Remove(@"diagonalPanels");
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if ((cboAvailableSlopes?.SelectedItem != null) && (_Panels?.Slopes.Count < 16))
            {
                _Panels.Slopes.Add(cboAvailableSlopes.SelectedItem as SlopeComposite);
                var _dc = _Panels;
                DataContext = null;
                DataContext = _dc;
            }
            e.Handled = true;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if ((lstCurrentSlopes?.SelectedIndex >= 0) && (_Panels?.Slopes.Count > 1))
            {
                _Panels.Slopes.RemoveAt((int)lstCurrentSlopes?.SelectedIndex);
                var _dc = _Panels;
                DataContext = null;
                DataContext = _dc;
            }
            e.Handled = true;
        }
    }
}
