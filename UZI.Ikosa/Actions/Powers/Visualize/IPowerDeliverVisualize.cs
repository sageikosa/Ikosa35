using System;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{
    public interface IPowerDeliverVisualize : ICapability
    {
        VisualizeTransferType GetTransferType();
        VisualizeTransferSize GetTransferSize();
        string GetTransferMaterialKey();

        VisualizeSplashType GetSplashType();
        string GetSplashMaterialKey();
    }

    public static class IPowerDeliverVisualizeHelper
    {
        #region private void AddVisualizersToContext(Tactical.MapContext context, int sequence, params TransientVisualizer[] visualizers)
        private static void AddVisualizersToContext(Tactical.MapContext context, int sequence, params TransientVisualizer[] visualizers)
        {
            // TODO: consider moving this into mapContext?
            if (sequence > 0)
            {
                var _delay = new TransientDelay { Duration = TimeSpan.FromMilliseconds(sequence * 100d) };
                _delay.Followers.AddRange(visualizers.Where(_v => _v != null));
                context.TransientVisualizers.Add(_delay);
            }
            else
            {
                foreach (var _vis in visualizers.Where(_v => _v != null))
                    context.TransientVisualizers.Add(_vis);
            }
        }
        #endregion

        #region public void GeneratePowerDeliverVisualizers(Point3D source, Point3D target, int sequence, bool showSplash)
        public static void GeneratePowerDeliverVisualizers(this IPowerDeliverVisualize self, MapContext mapContext, Point3D source, Point3D target, int sequence, bool showSplash)
        {
            if ((mapContext != null)
                && ((self?.GetTransferType() ?? VisualizeTransferType.None) != VisualizeTransferType.None))
            {
                var _material = self.GetTransferMaterialKey();
                MarkerBall _marker = null;
                if (showSplash)
                {
                    #region splash marker
                    switch (self.GetSplashType())
                    {
                        case VisualizeSplashType.Uniform:
                            _marker = new MarkerBall()
                            {
                                MaterialKey = self.GetSplashMaterialKey(),
                                StartRadius = 2.5,
                                EndRadius = 2.5,
                                Source = target,
                                Duration = TimeSpan.FromMilliseconds(1000)
                            };
                            break;
                        case VisualizeSplashType.Pop:
                            _marker = new MarkerBall()
                            {
                                MaterialKey = self.GetSplashMaterialKey(),
                                StartRadius = 0,
                                EndRadius = 2.5,
                                Source = target,
                                Duration = TimeSpan.FromMilliseconds(1000)
                            };
                            break;
                        case VisualizeSplashType.Drain:
                            _marker = new MarkerBall()
                            {
                                MaterialKey = self.GetSplashMaterialKey(),
                                StartRadius = 2.5,
                                EndRadius = 0,
                                Source = target,
                                Duration = TimeSpan.FromMilliseconds(1000)
                            };
                            break;
                        case VisualizeSplashType.Pulse:
                            _marker = new MarkerBall()
                            {
                                MaterialKey = self.GetSplashMaterialKey(),
                                StartRadius = 0,
                                EndRadius = 2.5,
                                Source = target,
                                Duration = TimeSpan.FromMilliseconds(600)
                            };
                            _marker.Followers.Add(new MarkerBall()
                            {
                                MaterialKey = self.GetSplashMaterialKey(),
                                StartRadius = 2.5,
                                EndRadius = 0,
                                Source = target,
                                Duration = TimeSpan.FromMilliseconds(600)
                            });
                            break;
                        case VisualizeSplashType.None:
                        default:
                            break;
                    }
                    #endregion
                }

                var _size = 0.2d;
                switch (self.GetTransferSize())
                {
                    case VisualizeTransferSize.Small:
                        _size = 0.1d;
                        break;
                    case VisualizeTransferSize.Large:
                        _size = 0.3d;
                        break;
                }

                var _distance = (target - source).Length;
                var _duration = _distance < 50 ? _distance * 20 : 1000;

                // power delivery
                switch (self.GetTransferType())
                {
                    case VisualizeTransferType.Beam:
                        var _beam = new RayTransition()
                        {
                            MaterialKey = _material,
                            Source = source,
                            Target = target,
                            HeadWidth = _size,
                            TailWidth = _size,
                            Duration = TimeSpan.FromMilliseconds(_duration)
                        };
                        AddVisualizersToContext(mapContext, sequence, _beam, _marker);
                        break;

                    case VisualizeTransferType.Cone:
                        var _cone = new RayTransition()
                        {
                            MaterialKey = _material,
                            Source = source,
                            Target = target,
                            HeadWidth = _size * 5,
                            TailWidth = _size,
                            Duration = TimeSpan.FromMilliseconds(_duration)
                        };
                        AddVisualizersToContext(mapContext, sequence, _cone, _marker);
                        break;

                    case VisualizeTransferType.SurgeFrom:
                        var _surgeFrom = new RaySurgeFromTransition()
                        {
                            MaterialKey = _material,
                            Source = source,
                            Target = target,
                            HeadWidth = _size,
                            TailWidth = _size,
                            Duration = TimeSpan.FromMilliseconds(_duration)
                        };
                        AddVisualizersToContext(mapContext, sequence, _surgeFrom, _marker);
                        break;

                    case VisualizeTransferType.FullSurge:
                        var _fullSurge = new RaySurgeFromTransition()
                        {
                            MaterialKey = _material,
                            Source = source,
                            Target = target,
                            HeadWidth = _size,
                            TailWidth = _size,
                            Duration = TimeSpan.FromMilliseconds(_duration / 2)
                        };
                        AddVisualizersToContext(mapContext, sequence, _fullSurge);
                        _fullSurge.Followers.Add(
                            new RaySurgeToTransition()
                            {
                                MaterialKey = _material,
                                Source = source,
                                Target = target,
                                HeadWidth = _size,
                                TailWidth = _size,
                                Duration = TimeSpan.FromMilliseconds(_duration / 2)
                            });
                        if (_marker != null)
                            _fullSurge.Followers.Add(_marker);
                        break;

                    case VisualizeTransferType.SurgeTo:
                        var _surgeTo = new RaySurgeToTransition()
                        {
                            MaterialKey = _material,
                            Source = source,
                            Target = target,
                            HeadWidth = _size,
                            TailWidth = _size,
                            Duration = TimeSpan.FromMilliseconds(_duration)
                        };
                        AddVisualizersToContext(mapContext, sequence, _surgeTo);
                        if (_marker != null)
                            _surgeTo.Followers.Add(_marker);
                        break;

                    case VisualizeTransferType.ConeBolt:
                        var _coneBolt = new RayBoltTransition()
                        {
                            MaterialKey = _material,
                            Source = source,
                            Target = target,
                            HeadWidth = 0d,
                            TailWidth = _size,
                            Duration = TimeSpan.FromMilliseconds(_duration),
                            Length = _size * 5d
                        };
                        AddVisualizersToContext(mapContext, sequence, _coneBolt);
                        if (_marker != null)
                            _coneBolt.Followers.Add(_marker);
                        break;

                    case VisualizeTransferType.CylinderBolt:
                        var _cylinerBolt = new RayBoltTransition()
                        {
                            MaterialKey = _material,
                            Source = source,
                            Target = target,
                            HeadWidth = _size,
                            TailWidth = _size,
                            Duration = TimeSpan.FromMilliseconds(_duration),
                            Length = _size * 4d
                        };
                        AddVisualizersToContext(mapContext, sequence, _cylinerBolt);
                        if (_marker != null)
                            _cylinerBolt.Followers.Add(_marker);
                        break;

                    case VisualizeTransferType.Orb:
                        var _flyingOrb = new FlyingOrbTransition()
                        {
                            MaterialKey = _material,
                            Source = source,
                            Target = target,
                            Duration = TimeSpan.FromMilliseconds(_duration),
                            Radius = _size
                        };
                        AddVisualizersToContext(mapContext, sequence, _flyingOrb);
                        if (_marker != null)
                            _flyingOrb.Followers.Add(_marker);
                        break;
                }
            }
        }
        #endregion

        public static void GeneratePowerDeliverVisualizers(this ICapabilityRoot root, MapContext mapContext, Point3D source, Point3D target, int sequence, bool showSplash)
        {
            GeneratePowerDeliverVisualizers(root.GetCapability<IPowerDeliverVisualize>(), mapContext, source, target, sequence, showSplash);
        }
    }
}
