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
using System.Windows.Media.Media3D;
using Uzi.Ikosa.UI;
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for CellMaterialEditor.xaml
    /// </summary>
    public partial class CellMaterialEditor : TabItem, IPackageItem, IHostedTabItem
    {
        public CellMaterialEditor(CellMaterial cellMaterial, IHostTabControl host)
        {
            InitializeComponent();
            _CellMaterial = cellMaterial;
            DataContext = _CellMaterial;
            lstTilings.ItemsSource = _CellMaterial.AvailableTilings.Select(_t => new TileSetViewModel { TileSet = _t });
            _Host = host;
        }

        private IHostTabControl _Host;

        private CellMaterial _CellMaterial;
        public CellMaterial CellMaterial { get { return _CellMaterial; } }

        public object PackageItem { get { return _CellMaterial; } }

        private void cmdbndTileNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cmdbndTileNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _window = new TileName()
            {
                Owner = Window.GetWindow(this)
            };
            bool? _result = _window.ShowDialog();
            if (_result.HasValue && _result.Value)
            {
                var _tileSet = new TileSet(_window.NewName, null, CellMaterial.LocalMap);
                CellMaterial.AddTiling(_tileSet);
                lstTilings.Items.Refresh();
            }
        }

        private void cmdbndTileDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (lstTilings.SelectedItem != null)
            {
                e.CanExecute = true;
            }
            e.Handled = true;
        }

        private void cmdbndTileDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: implement!
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        private void DrawSample()
        {
            try
            {
                var _tiles = lstTilings.SelectedItem as TileSetViewModel;
                var _gas = new GasCellMaterial(@"Gas", CellMaterial.LocalMap);
                var _sample = new WedgeCellSpace(_gas, null, CellMaterial, _tiles.TileSet, 1.5, 1.5);
                var _model = _sample.GenerateModel(0, 0, 0, 0, null);
                if (_model.Opaque != null)
                    grpObjects.Children.Add(_model.Opaque);
                if (_model.Alpha != null)
                    grpObjects.Children.Add(_model.Alpha);
            }
            catch (Exception _except)
            {
                MessageBox.Show(_except.Message);
            }
        }

        private void lstTilings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstTilings.SelectedItem != null)
                DrawSample();
        }

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion
    }
}
