using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using HelixToolkit.Wpf;
using System.Windows;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    /// <summary>Fixed length segment travels from source to target</summary>
    public class RayBoltTransition : RayTransition
    {
        /// <summary>Fixed length segment travels from source to target</summary>
        public RayBoltTransition()
            : base()
        {
            Length = 1d;
        }

        /// <summary>Length of the bolt</summary>
        public double Length { get; set; }

        protected override IEnumerable<Timeline> GetVisibleAnimations(Visualization visualization)
        {
            return GetOffsetAnimations(visualization);
        }

        protected override void DoDrawVisibleModels(Visualization visualization)
        {
            // build bolt
            var _builder = new MeshBuilder();
            var _vector = Target - Source;
            _builder.AddCone(Source, _vector, TailWidth / 2d, HeadWidth / 2d, Length, true, true, 6,
                TranslateTransform3D.Identity, TranslateTransform3D.Identity);
            var _bolt = new GeometryModel3D(_builder.ToMesh(true), null);

            _bolt.Transform = new TranslateTransform3D(0, 0, 0);

            visualization.TransientGroup.Children.Add(_bolt);
            visualization.Scope.RegisterName(MyName, _bolt);
        }

        public override TransientVisualizerInfo ToInfo()
        {
            var _info = ToRayTransitionInfo<RayBoltTransitionInfo>();
            _info.Length = this.Length;
            return _info;
        }
    }
}
