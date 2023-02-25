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
using Uzi.Visualize;
using System.Windows.Media.Media3D;

namespace VisualizationLab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IPresentationInputBinder, IResolveMaterial
    {
        public MainWindow()
        {
            InitializeComponent();
            _Visualization = new Visualization(this, grpTokens, grpRooms, grpAlphaRooms, grpTransients, this.vp3D);

            var _gradient = new LinearGradientBrush();
            _gradient.StartPoint = new Point(0.5, 0);
            _gradient.EndPoint = new Point(0.5, 1);
            _gradient.GradientStops.Add(new GradientStop(Colors.Red, 0));
            _gradient.GradientStops.Add(new GradientStop(Color.FromArgb(224, 255, 165, 0), 0.5));//#FF FF A5 00 = Orange
            _gradient.GradientStops.Add(new GradientStop(Colors.Red, 1));

            _Material = new DiffuseMaterial(_gradient);
            _Material.Freeze();
        }

        private Visualization _Visualization;
        private DiffuseMaterial _Material;

        #region IPresentationInputBinder Members

        public IEnumerable<InputBinding> GetBindings(Presentable presentable)
        {
            yield break;
        }

        #endregion

        private void SetResolvers()
        {
            VisualEffectMaterial.PushResolver(this);
        }

        private void Ray_Click(object sender, RoutedEventArgs e)
        {
            SetResolvers();
            var _ray = new RayTransition(@"Ray")
            {
                Source = new Point3D(0, 0, 5),
                Target = new Point3D(15, 15, 5),
                HeadWidth = 0.5d,
                TailWidth = 0.1d,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            _Visualization.SetTransients(new[] { _ray });
            _Visualization.AnimateTransients();
        }

        private void Bolt_Click(object sender, RoutedEventArgs e)
        {
            SetResolvers();
            var _bolt = new RayBoltTransition(@"RayBolt")
            {
                Source = new Point3D(0, 0, 5),
                Target = new Point3D(20, 20, 5),
                HeadWidth = 0.0d,
                TailWidth = 0.2d,
                Duration = TimeSpan.FromMilliseconds(500),
                Length = 1d
            };
            _Visualization.SetTransients(new[] { _bolt });
            _Visualization.AnimateTransients();
        }

        private void Orb_Click(object sender, RoutedEventArgs e)
        {
            SetResolvers();
            //var _orb = new FlyingOrbTransition(@"Orb")
            var _orb = new FlyingOrbTransition(@"#E000FF00|#C0A5FF00|#E000FF00")
            {
                Source = new Point3D(0, 0, 5),
                Target = new Point3D(20, 20, 5),
                Radius = 0.5d,
                Duration = TimeSpan.FromMilliseconds(500),
            };
            _Visualization.SetTransients(new[] { _orb });
            _Visualization.AnimateTransients();
        }

        private void Surge_Click(object sender, RoutedEventArgs e)
        {
            SetResolvers();
            var _surge1 = new RaySurgeFromTransition(@"SurgeFrom")
            {
                Source = new Point3D(0, 0, 5),
                Target = new Point3D(20, 20, 5),
                HeadWidth = 0.2d,
                TailWidth = 0.2d,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            var _surge2 = new RaySurgeToTransition(@"SurgeTo")
            {
                Source = new Point3D(0, 0, 5),
                Target = new Point3D(20, 20, 5),
                HeadWidth = 0.2d,
                TailWidth = 0.2d,
                Duration = TimeSpan.FromMilliseconds(250)
            };
            _surge1.Followers.Add(_surge2);

            _Visualization.SetTransients(new[] { _surge1 });
            _Visualization.AnimateTransients();
        }

        private void Marker_Click(object sender, RoutedEventArgs e)
        {
            SetResolvers();
            var _ball = new MarkerBall(@"Ball")
            {
                Source = new Point3D(0, 0, 5),
                StartRadius = 1d,
                EndRadius = 5d,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            _Visualization.SetTransients(new[] { _ball });
            _Visualization.AnimateTransients();
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            _Visualization.ReAnimateTransients();
        }

        private void Transfer1_Click(object sender, RoutedEventArgs e)
        {
            SetResolvers();
            var _ball1 = new MarkerBall(@"Ball")
            {
                Source = new Point3D(0, 0, 5),
                StartRadius = 2.5d,
                EndRadius = 0.5d,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            var _ray = new RayTransition(@"Ray")
            {
                Source = new Point3D(0, 0, 5),
                Target = new Point3D(15, 15, 5),
                HeadWidth = 1d,
                TailWidth = 0.2d,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            var _ball2 = new MarkerBall(@"Ball")
            {
                Source = new Point3D(15, 15, 5),
                StartRadius = 0.5d,
                EndRadius = 2.5d,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            _Visualization.SetTransients(new TransientVisualizer[] { _ball1, _ray, _ball2 });
            _Visualization.AnimateTransients();
        }

        private void Transfer2_Click(object sender, RoutedEventArgs e)
        {
            SetResolvers();
            var _bolt = new RayBoltTransition(@"RayBolt")
            {
                Source = new Point3D(0, 0, 5),
                Target = new Point3D(20, 20, 5),
                HeadWidth = 0.0d,
                TailWidth = 0.2d,
                Duration = TimeSpan.FromMilliseconds(500),
                Length = 1d
            };

            var _delay = new TransientDelay { Duration = TimeSpan.FromMilliseconds(100) };
            _bolt.Followers.Add(_delay);

            var _ball = new MarkerBall(@"Ball")
            {
                Source = new Point3D(20, 20, 5),
                StartRadius = 0.5d,
                EndRadius = 2.5d,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            _delay.Followers.Add(_ball);

            _Visualization.SetTransients(new TransientVisualizer[] { _bolt });
            _Visualization.AnimateTransients();
        }

        private void Transfer3_Click(object sender, RoutedEventArgs e)
        {
            SetResolvers();
            var _ball0 = new MarkerBall(@"Ball")
            {
                Source = new Point3D(0, 0, 5),
                StartRadius = 0.5d,
                EndRadius = 2.5d,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            var _ball1 = new MarkerBall(@"Ball")
            {
                Source = new Point3D(0, 0, 5),
                StartRadius = 2.5d,
                EndRadius = 0.5d,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            var _surge1 = new RaySurgeFromTransition(@"SurgeFrom")
            {
                Source = new Point3D(0, 0, 5),
                Target = new Point3D(20, 20, 5),
                HeadWidth = 0.2d,
                TailWidth = 0.2d,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            _ball0.Followers.Add(_ball1);
            _ball0.Followers.Add(_surge1);

            var _surge2 = new RaySurgeToTransition(@"SurgeTo")
            {
                Source = new Point3D(0, 0, 5),
                Target = new Point3D(20, 20, 5),
                HeadWidth = 0.2d,
                TailWidth = 0.2d,
                Duration = TimeSpan.FromMilliseconds(250)
            };
            _surge1.Followers.Add(_surge2);

            var _ball2 = new MarkerBall(@"Ball")
            {
                Source = new Point3D(20, 20, 5),
                StartRadius = 0.5d,
                EndRadius = 2.5d,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            _surge1.Followers.Add(_ball2);

            _Visualization.SetTransients(new[] { _ball0 });
            _Visualization.AnimateTransients();
        }

        #region IResolveMaterial Members

        public Material GetMaterial(object key, VisualEffect effect)
        {
            return _Material;
        }

        public IResolveMaterial IResolveMaterialParent { get { return null; } }
        public IEnumerable<BrushDefinitionListItem> ResolvableBrushes { get { yield break; } }

        #endregion
    }
}