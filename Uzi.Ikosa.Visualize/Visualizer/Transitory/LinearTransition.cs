using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts;
using System.Windows.Media.Animation;
using System.Windows;

namespace Uzi.Visualize
{
    public abstract class LinearTransition : MaterialVisualizer
    {
        public LinearTransition()
            : base()
        {
        }

        #region protected IEnumerable<Timeline> GetOffsetAnimations(Visualization visualization)
        /// <summary>
        /// Derived classes that need to animate over the linear path can use this.  
        /// Transform *must* be a TranslateTransform3D.
        /// </summary>
        protected IEnumerable<Timeline> GetOffsetAnimations(Visualization visualization)
        {
            var _vector = Target - Source;
            Func<string, double, DoubleAnimation> _makeAnimation = (name, endValue) =>
            {
                var _anim = new DoubleAnimation(0, endValue, Duration, FillBehavior.Stop);
                Storyboard.SetTargetName(_anim, MyName);
                Storyboard.SetTargetProperty(_anim, new PropertyPath(string.Concat(@"Transform.(TranslateTransform3D.", name, @")")));
                _anim.FillBehavior = FillBehavior.HoldEnd;
                return _anim;
            };

            // animate position
            yield return _makeAnimation(@"OffsetX", _vector.X);
            yield return _makeAnimation(@"OffsetY", _vector.Y);
            yield return _makeAnimation(@"OffsetZ", _vector.Z);
            yield break;
        }
        #endregion

        /// <summary>Target of the signal</summary>
        public Point3D Target { get; set; }

        protected LTInfo ToLinearTransitionInfo<LTInfo>()
            where LTInfo : LinearTransitionInfo, new()
        {
            var _info = this.ToMaterialVisualizerInfo<LTInfo>();
            _info.Target = this.Target;
            return _info;
        }
    }
}
