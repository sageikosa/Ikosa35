using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;
using Uzi.Visualize.Contracts;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    /// <summary>Flying orb travels over path</summary>
    public class FlyingOrbTransition : LinearTransition
    {
        /// <summary>Flying orb travels over path</summary>
        public FlyingOrbTransition()
            : base()
        {
            ThetaDiv = 8;
            PhiDiv = 8;
        }

        /// <summary>Radius of orb</summary>
        public double Radius { get; set; }

        public int ThetaDiv { get; set; }
        public int PhiDiv { get; set; }

        protected override IEnumerable<Timeline> GetVisibleAnimations(Visualization visualization)
        {
            return GetOffsetAnimations(visualization);
        }

        protected FOTInfo ToFlyingOrbTransitionInfo<FOTInfo>()
            where FOTInfo : FlyingOrbTransitionInfo, new()
        {
            var _info = ToLinearTransitionInfo<FOTInfo>();
            _info.Radius = this.Radius;
            _info.ThetaDiv = this.ThetaDiv;
            _info.PhiDiv = this.PhiDiv;
            return _info;
        }

        public override TransientVisualizerInfo ToInfo()
        {
            return ToFlyingOrbTransitionInfo<FlyingOrbTransitionInfo>();
        }

        protected override void DoDrawVisibleModels(Visualization visualization)
        {
            // build sphere
            var _builder = new MeshBuilder();
            _builder.AddSphere(Source, Radius, ThetaDiv, PhiDiv);
            var _ball = new GeometryModel3D(_builder.ToMesh(true), null);
            _ball.Transform = new TranslateTransform3D(0, 0, 0);
            visualization.TransientGroup.Children.Add(_ball);
            visualization.Scope.RegisterName(MyName, _ball);
        }
    }
}
