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
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for CellSpaceEditor.xaml
    /// </summary>
    public partial class CellSpaceEditor : UserControl
    {
        public CellSpaceEditor()
        {
            InitializeComponent();
        }

        public LocalMap Map
        {
            get { return (LocalMap)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Map.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register(@"Map", typeof(LocalMap), typeof(CellSpaceEditor),
            new UIPropertyMetadata(null, new PropertyChangedCallback(LocalMapChanged)));

        private static void LocalMapChanged(DependencyObject depends, DependencyPropertyChangedEventArgs args)
        {
            var _editor = depends as CellSpaceEditor;
            if (_editor.Map != null)
            {
                _editor.Resources.Add(@"roomMaterials", _editor.Map.AllCellMaterials);
            }
            else
            {
                if (_editor.Resources.Contains(@"roomMaterials")) _editor.Resources.Remove(@"roomMaterials");
            }
        }
    }
}
