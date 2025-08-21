using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    public abstract class MaterialVisualizer : TransientVisualizer
    {
        protected MaterialVisualizer()
            : base()
        {
            _MaterialKey = string.Empty;
            _Material = null;
        }

        #region private data
        private string _MaterialKey;
        private Material _Material = null;
        #endregion

        /// <summary>Material key to use to draw the ray</summary>
        public string MaterialKey
        {
            get { return _MaterialKey; }
            set
            {
                _MaterialKey = value; 
                _Material = null;
            }
        }

        /// <summary>Material to use to draw the ray</summary>
        public Material Material
        {
            get
            {
                _Material ??= VisualEffectMaterial.ResolveMaterial(MaterialKey, VisualEffect.Normal);
                return _Material;
            }
        }

        /// <summary>
        /// Turns on the Material for the duration of the animation
        /// </summary>
        /// <returns></returns>
        protected ObjectAnimationUsingKeyFrames GetMaterialOnAnimation()
        {
            // hide the ray when not is use
            var _materialAnim = new ObjectAnimationUsingKeyFrames();
            _materialAnim.KeyFrames.Add(new DiscreteObjectKeyFrame(Material, KeyTime.FromPercent(0d)));
            Storyboard.SetTargetName(_materialAnim, MyName);
            Storyboard.SetTargetProperty(_materialAnim, new PropertyPath(GeometryModel3D.MaterialProperty));
            _materialAnim.Duration = new Duration(Duration);
            _materialAnim.FillBehavior = FillBehavior.Stop;
            return _materialAnim;
        }

        /// <summary>Provides material animation (on/off) and any other animations needed when the material is on</summary>
        protected override Timeline GetDirectAnimations(Visualization visualization)
        {
            // draw stuff that needs to be drawn
            DoDrawVisibleModels(visualization);

            // fetch any animations needed during the visible phase of the timeline
            var _anim = GetVisibleAnimations(visualization).ToList();

            if (_anim.Any())
            {
                var _visible = new ParallelTimeline(TimeSpan.FromTicks(0), Duration);
                _visible.Children.Add(GetMaterialOnAnimation());
                foreach (var _a in _anim)
                {
                    _visible.Children.Add(_a);
                }

                return _visible;
            }
            else
            {
                // no visible animations, so only animate the material on
                return GetMaterialOnAnimation();
            }
        }

        /// <summary>Draw stuff that needs to be drawn (using null Material!)</summary>
        protected abstract void DoDrawVisibleModels(Visualization visualization);

        /// <summary>Provide animations needed to make the model do its stuff</summary>
        protected abstract IEnumerable<Timeline> GetVisibleAnimations(Visualization visualization);

        protected MVInfo ToMaterialVisualizerInfo<MVInfo>()
            where MVInfo : MaterialVisualizerInfo, new()
        {
            var _info = this.ToTransientVisualizerInfo<MVInfo>();
            _info.MaterialKey = this.MaterialKey;
            return _info;
        }
    }
}
