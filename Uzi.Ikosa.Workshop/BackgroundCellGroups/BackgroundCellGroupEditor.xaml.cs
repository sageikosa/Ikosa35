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

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for BackgroundCellGroupEditor.xaml
    /// </summary>
    public partial class BackgroundCellGroupEditor : TabItem, IPackageItem, IHostedTabItem
    {
        public BackgroundCellGroupEditor(BackgroundCellGroup cellGroup, IHostTabControl host)
        {
            InitializeComponent();
            _Group = cellGroup;
            this.DataContext = cellGroup;
            cboAmbient.Items.Clear();
            cboAmbient.Items.Add(new ComboBoxItem { Content = @"-None-", Tag = null });
            if (_Group.Light == null)
            {
                cboAmbient.SelectedIndex = 0;
            }
            foreach (var _ambient in cellGroup.Map.AmbientLights.Select(_kvp => _kvp.Value))
            {
                var _cboItem = new ComboBoxItem
                {
                    Content = _ambient.Name,
                    Tag = _ambient,
                    ToolTip = string.Format(@"({0},{1},{2}) {3}", _ambient.ZOffset, _ambient.YOffset, _ambient.XOffset, _ambient.AmbientLevel)
                };

                cboAmbient.Items.Add(_cboItem);
                if (_Group.Light == _cboItem.Tag)
                {
                    cboAmbient.SelectedItem = _cboItem;
                }
            }
            _Host = host;
        }

        private IHostTabControl _Host;

        private BackgroundCellGroup _Group;
        public BackgroundCellGroup CellGroup { get { return _Group; } }

        public object PackageItem { get { return _Group; } }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        private void DrawSample()
        {
            try
            {
                var _space = CellGroup.TemplateCell.CellSpace as CellSpace;
                var _sample = new SampleCellSpace(_space.CellMaterial, _space.Tiling);
                var _model = _sample.GenerateModel(0, 0, 0, 0, null);
                if (_model.Opaque != null)
                {
                    grpObjects.Children.Add(_model.Opaque);
                }

                if (_model.Alpha != null)
                {
                    grpObjects.Children.Add(_model.Alpha);
                }
            }
            catch (Exception /*_except*/)
            {
                //MessageBox.Show(_except.Message);
            }
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DrawSample();
        }

        private void cboAmbient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Uzi.Ikosa.Tactical.AmbientLight _light = (cboAmbient.SelectedItem as ComboBoxItem).Tag as Uzi.Ikosa.Tactical.AmbientLight;
            if (_light != _Group.Light)
            {
                _Group.Light = _light;
            }
        }

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion
    }
}
