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
using System.Windows.Media.Media3D;

namespace ModelMaker
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        int _Sides = 3;
        //double _LatBand = 5;
        public Window1()
        {
            InitializeComponent();

            //geomModel.Geometry = VolumetricMeshMaker.CylindricalSurface(0.5d, 1d, 2.25d, 12, 5, 115, 235);
            //geomModel.Geometry = VolumetricMeshMaker.SphericalSurface(1.5, 9, Convert.ToInt32(_LatBand * 2 / 10), 175, 275, -_LatBand, _LatBand);
            //geomModel.Geometry = VolumetricMeshMaker.ConicSurface(0, 1, 0, 3, 90, 180, 16, 3);
            geomModel.Geometry = FlatMeshMaker.RingSurface(1, 2.5, 3, 2, 0, 270);
            //geomModel.Geometry = FlatMeshMaker.Polygon(2, 3, 0);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //_LatBand += 5;
            //geomModel.Geometry = VolumetricMeshMaker.SphericalSurface(1.5, 9, Convert.ToInt32(_LatBand * 2 / 10), 175, 275, -_LatBand, _LatBand);
            _Sides++;
            geomModel.Geometry = FlatMeshMaker.RingSurface(1,2.5, _Sides, 3, 0,270);
        }
    }
}
