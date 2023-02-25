using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    /// <summary>
    /// Visualization that implies a transition in time or space
    /// </summary>
    public abstract class TransientVisualizer
    {
        protected TransientVisualizer()
        {
            Followers = new List<TransientVisualizer>();
            MyName = string.Concat(@"my", (Guid.NewGuid()).ToString().Replace(@"-", string.Empty));
        }

        public string MyName { get; private set; }

        /// <summary>Origin of the signal</summary>
        public Point3D Source { get; set; }

        /// <summary>Length of time the signal should last</summary>
        public TimeSpan Duration { get; set; }

        /// <summary>Signal that should happen after this signal</summary>
        public List<TransientVisualizer> Followers { get; private set; }

        public Timeline GetTimeline(Visualization visualization)
        {
            var _direct = GetDirectAnimations(visualization);
            var _tail = _direct.Duration.TimeSpan;
            if (Followers.Any())
            {
                var _extra = TimeSpan.FromTicks(0);
                var _parallel = new ParallelTimeline();
                _parallel.Children.Add(_direct);
                foreach (var _follow in Followers)
                {
                    // add follower timelines
                    var _inner = _follow.GetTimeline(visualization);
                    _inner.BeginTime = _tail;
                    _parallel.Children.Add(_inner);

                    // see how long the duration may need to be extended
                    if (_inner.Duration.TimeSpan > _extra)
                        _extra = _inner.Duration.TimeSpan;
                }

                // this timeline is as long as the direct, and the longest inner timeline
                _parallel.Duration = Duration + _extra;
                return _parallel;
            }
            else
            {
                return _direct;
            }
        }

        protected abstract Timeline GetDirectAnimations(Visualization visualization);

        protected TVInfo ToTransientVisualizerInfo<TVInfo>()
            where TVInfo: TransientVisualizerInfo, new()
        {
            return new TVInfo
            {
                MyName = MyName,
                Duration = Duration.Ticks,
                Source = Source,
                Followers = Followers.Select(_f => _f.ToInfo()).ToList()
            };
        }

        public abstract TransientVisualizerInfo ToInfo();
    }
}
