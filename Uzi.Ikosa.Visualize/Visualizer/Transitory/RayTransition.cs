using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using HelixToolkit.Wpf;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    /// <summary>Linear transition for drawing a ray-like effect</summary>
    public class RayTransition : LinearTransition
    {
        public RayTransition()
            : base()
        {
            HeadWidth = 0.15d;
            TailWidth = 0.15d;
        }

        /// <summary>Leading (contact) width</summary>
        public double HeadWidth { get; set; }

        /// <summary>Trailing (final) width</summary>
        public double TailWidth { get; set; }

        protected override IEnumerable<Timeline> GetVisibleAnimations(Visualization visualization)
        {
            yield break;
        }

        protected override void DoDrawVisibleModels(Visualization visualization)
        {
            var _builder = new MeshBuilder();
            var _vector = Target - Source;
            _builder.AddCone(Source, _vector, TailWidth / 2d, HeadWidth / 2d, _vector.Length, true, true, 7,
                TranslateTransform3D.Identity, TranslateTransform3D.Identity);
            var _ray = new GeometryModel3D(_builder.ToMesh(true), null);
            visualization.TransientGroup.Children.Add(_ray);
            visualization.Scope.RegisterName(MyName, _ray);
        }

        protected RTInfo ToRayTransitionInfo<RTInfo>()
            where RTInfo : RayTransitionInfo, new ()
        {
            var _info = ToLinearTransitionInfo<RTInfo>();
            _info.HeadWidth = this.HeadWidth;
            _info.TailWidth = this.TailWidth;
            return _info;
        }

        public override TransientVisualizerInfo ToInfo()
        {
            return ToRayTransitionInfo<RayTransitionInfo>();
        }
    }
}