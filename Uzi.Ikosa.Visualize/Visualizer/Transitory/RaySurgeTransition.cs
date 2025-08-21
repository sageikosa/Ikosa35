using System;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using HelixToolkit.Wpf;
using System.Windows;

namespace Uzi.Visualize
{
    /// <summary>Head from source to target, then tail from source to target, each taking half time</summary>
    public abstract class RaySurgeTransition : RayTransition
    {
        /// <summary>Head from source to target, then tail from source to target, each taking half time</summary>
        protected RaySurgeTransition()
            : base()
        {
        }

        protected DoubleAnimation DoMakeAnimation(int index, double startValue, double endValue)
        {
            var _anim = new DoubleAnimation(startValue, endValue, new Duration(TimeSpan.FromTicks(Duration.Ticks / 2)), FillBehavior.HoldEnd);
            Storyboard.SetTargetName(_anim, MyName);
            Storyboard.SetTargetProperty(_anim, new PropertyPath(@"Transform.Children[0].(ScaleTransform3D.ScaleZ)"));
            _anim.BeginTime = TimeSpan.FromTicks(Duration.Ticks / 2 * index);
            _anim.FillBehavior = FillBehavior.HoldEnd;
            return _anim;
        }

        protected abstract ScaleTransform3D InitialTransform(Point3D target);

        protected override void DoDrawVisibleModels(Visualization visualization)
        {
            // animate along the Z-Axis
            var _vector = Target - Source;
            var _target = Source;
            _target.Offset(0, 0, _vector.Length);

            // scale along Z-Axis, first growing from soruce, then shrinking towards target
            var _transform = new Transform3DGroup();
            _transform.Children.Add(InitialTransform(_target));

            // rotate the Z-Axis to match the ray-vector (if needed)
            // ... _axis will be the vector around which the rotation needs to occur
            var _axis = Vector3D.CrossProduct(new Vector3D(0, 0, 1), _vector);
            if (_axis.Length != 0)
            {
                _transform.Children.Add(
                    new RotateTransform3D(
                        new QuaternionRotation3D(
                            new Quaternion(_axis, _axis.AxisAngleBetween(new Vector3D(0, 0, 1), _vector))))
                    {
                        CenterX = Source.X,
                        CenterY = Source.Y,
                        CenterZ = Source.Z
                    });
            }

            // build ray along Z-Axis
            var _builder = new MeshBuilder();
            _builder.AddCone(Source, new Vector3D(0, 0, 1), TailWidth / 2d, HeadWidth / 2d, _vector.Length, true, true, 7,
                TranslateTransform3D.Identity, TranslateTransform3D.Identity);

            // prepare the ray for animation
            var _ray = new GeometryModel3D(_builder.ToMesh(true), null);
            _ray.Transform = _transform;
            visualization.TransientGroup.Children.Add(_ray);
            visualization.Scope.RegisterName(MyName, _ray);
        }
    }
}
