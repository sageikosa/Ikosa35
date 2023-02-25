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
    public class MarkerBall : MaterialVisualizer
    {
        public MarkerBall()
            : base()
        {
            ThetaDiv = 8;
            PhiDiv = 8;
        }

        public int ThetaDiv { get; set; }

        public int PhiDiv { get; set; }

        /// <summary>Initial Radius for sphere</summary>
        public double StartRadius { get; set; }

        /// <summary>Final Radius for sphere</summary>
        public double EndRadius { get; set; }

        // ???: animating brushes for materials...? general purpose...linear

        protected override IEnumerable<Timeline> GetVisibleAnimations(Visualization visualization)
        {
            Func<string, DoubleAnimation> _makeAnimation = (name) =>
            {
                var _anim = new DoubleAnimation(StartRadius, EndRadius, Duration, FillBehavior.Stop);
                Storyboard.SetTargetName(_anim, MyName);
                Storyboard.SetTargetProperty(_anim, new PropertyPath(string.Concat(@"Transform.(ScaleTransform3D.", name, @")")));
                _anim.FillBehavior = FillBehavior.Stop;
                return _anim;
            };

            yield return _makeAnimation(@"ScaleX");
            yield return _makeAnimation(@"ScaleY");
            yield return _makeAnimation(@"ScaleZ");
            yield break;
        }

        protected override void DoDrawVisibleModels(Visualization visualization)
        {
            // build sphere
            var _builder = new MeshBuilder();
            _builder.AddSphere(Source, 1d, ThetaDiv, PhiDiv);
            var _ball = new GeometryModel3D(_builder.ToMesh(true), Material);
            _ball.Transform = new ScaleTransform3D(new Vector3D(), Source);
            visualization.TransientGroup.Children.Add(_ball);
            visualization.Scope.RegisterName(MyName, _ball);
        }

        public override TransientVisualizerInfo ToInfo()
        {
            var _mInfo = ToMaterialVisualizerInfo<MarkerBallInfo>();
            _mInfo.ThetaDiv = this.ThetaDiv;
            _mInfo.PhiDiv = this.PhiDiv;
            _mInfo.StartRadius = this.StartRadius;
            _mInfo.EndRadius = this.EndRadius;
            return _mInfo;
        }
    }
}
