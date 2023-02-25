using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using HelixToolkit.Wpf;
using System.Diagnostics;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Visualize
{
    public static class ModelGenerator
    {
        static ModelGenerator()
        {
            var _matteBrush = new LinearGradientBrush()
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };
            _matteBrush.GradientStops.Add(new GradientStop(Colors.Blue, 0));
            _matteBrush.GradientStops.Add(new GradientStop(Colors.Cyan, 0.5));
            _matteBrush.GradientStops.Add(new GradientStop(Colors.Blue, 1));
            _MatteMaterial = new DiffuseMaterial(_matteBrush);
            _MatteMaterial.Freeze();
        }

        private static DiffuseMaterial _MatteMaterial;

        #region public static Model3D GenerateModel(IResolveModel3D, List<VisualEffectValue, string, Dictionary<DependencyProperty, object>)
        public static Model3D GenerateModel(IResolveModel3D resolver, List<VisualEffectValue> visualEffects,
            string modelKey, Dictionary<string, int> externalValues)
        {
            if (!string.IsNullOrWhiteSpace(modelKey))
            {
                // sense effects (markup extensions, so needed before materializing from XAML)
                var _base = visualEffects.FirstOrDefault(_ve => _ve.Type == typeof(SenseEffectExtension))?.Effect ?? VisualEffect.Skip;
                VisualEffectProcessor.InitializeEffects(_base);
                foreach (var _ve in visualEffects)
                {
                    // sets the thread static field values for every defined effect
                    var _field = _ve.Type.GetField(@"EffectValue", BindingFlags.Static | BindingFlags.Public);
                    _field.SetValue(null, _ve.Effect);
                }

                // external values
                ExternalVal.Values.Clear();
                foreach (var _kvp in externalValues)
                {
                    ExternalVal.Values.Add(_kvp.Key, _kvp.Value);
                }

                var _resolve = resolver;
                while (_resolve != null)
                {
                    // resolve model
                    var _model = _resolve.GetPrivateModel3D(modelKey);
                    if (_model != null)
                    {
                        return _model;
                    }
                    _resolve = _resolve.IResolveModel3DParent;
                }
            }
            return null;
        }
        #endregion

        #region public static RotateTransform3D GetBaseFaceTransform(AnchorFace face, Point3D position)
        public static RotateTransform3D GetBaseFaceTransform(AnchorFace face, Point3D position)
        {
            RotateTransform3D _transform = null;
            switch (face)
            {
                case AnchorFace.ZHigh:
                    _transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 180), position);
                    break;
                case AnchorFace.YLow:
                    _transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), -90), position);
                    break;
                case AnchorFace.YHigh:
                    _transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90), position);
                    break;
                case AnchorFace.XLow:
                    _transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 90), position);
                    break;
                case AnchorFace.XHigh:
                    _transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), -90), position);
                    break;
            }
            if (_transform != null)
                _transform.Freeze();
            return _transform;
        }
        #endregion

        #region public static void TransformModel(...)
        public static void TransformModel(Model3D model, IModelTransformer transform, Point3D centroid,
            out ScaleTransform3D expressedScale, out AxisAngleRotation3D twistAxis, out AxisAngleRotation3D tiltAxis,
            out AxisAngleRotation3D pivotAxis, out TranslateTransform3D expressedLocation)
        {
            if (model != null)
            {
                // set the transforms to the expressed model
                model.Transform = GetTransform(transform, centroid, 
                    out expressedScale, out twistAxis, 
                    out tiltAxis, out pivotAxis, 
                    out expressedLocation);

                // TODO: invisible (besides sense effect)?
                // TODO: adding aggregate models (for effect marking)
            }
            else
            {
                expressedScale = null;
                expressedLocation = null;
                twistAxis = null;
                tiltAxis = null;
                pivotAxis = null;
            }
        }

        /// <summary>NOTE: transform created unfrozen in thread context.</summary>
        public static Transform3D GetTransform(IModelTransformer transform, Point3D centroid, 
            out ScaleTransform3D expressedScale, out AxisAngleRotation3D twistAxis, 
            out AxisAngleRotation3D tiltAxis, out AxisAngleRotation3D pivotAxis, 
            out TranslateTransform3D expressedLocation)
        {
            // build new transforms
            // TODO: may need more/better control of model
            var _transGroup = new Transform3DGroup();

            // 1: size
            Vector3D _scale = transform.CubeFitScale * transform.ApparentScale;
            expressedScale = new ScaleTransform3D(_scale.X, _scale.Y, _scale.Z);
            _transGroup.Children.Add(expressedScale);

            // 2: twist (since twist precedes heading; when used, allows sideways tilt)
            var _expressedTwist = new RotateTransform3D();
            _transGroup.Children.Add(_expressedTwist);
            twistAxis = new AxisAngleRotation3D(new Vector3D(0, 0, 1), transform.Twist);
            _expressedTwist.Rotation = twistAxis;

            // 3: tilt
            var _expressedTilt = new RotateTransform3D() { CenterZ = transform.TiltElevation };
            _transGroup.Children.Add(_expressedTilt);
            tiltAxis = new AxisAngleRotation3D(transform.TiltAxis, transform.Tilt);
            _expressedTilt.Rotation = tiltAxis;

            // 4: pivot/heading (front must face some direction)
            var _expressedPivot = new RotateTransform3D();
            _transGroup.Children.Add(_expressedPivot);
            pivotAxis = new AxisAngleRotation3D(new Vector3D(0, 0, 1), transform.Pivot);
            _expressedPivot.Rotation = pivotAxis;

            // 5: custom (whee!!!)
            if (transform.CustomTransforms.Children.Count > 0)
                _transGroup.Children.Add(transform.CustomTransforms);

            // 6: global position (must sit somewhere in the "world")
            if (transform.IsAdjustingPosition)
                // since the model is adjusting its own position, position from center
                expressedLocation = new TranslateTransform3D(transform.X * 5d, transform.Y * 5d, transform.Z * 5d);
            else
                // ...otherwise, position from base of model
                expressedLocation = new TranslateTransform3D(
                    ((double)transform.X + ((double)transform.XLength / 2d)) * 5d,
                    ((double)transform.Y + ((double)transform.YLength / 2d)) * 5d,
                    transform.IsFullOrigin ? ((double)transform.Z + ((double)transform.ZHeight / 2d)) * 5d : transform.Z * 5d);
            _transGroup.Children.Add(expressedLocation);

            // 7: base face binding (no bias for ZLow as "down")
            var _baseBind = GetBaseFaceTransform(transform.BaseFace, centroid);
            if (_baseBind != null)
            {
                _transGroup.Children.Add(_baseBind);
            }

            // 8: intra-model offset (defined by the presenter for various reasons such as terrain hugging)
            _transGroup.Children.Add(new TranslateTransform3D(transform.IntraModelOffset));

            return _transGroup;
        }
        #endregion

        #region GetQuadPoints, GetMatteQuadPoints, GetIconQuadPoints
        private static IEnumerable<Point3D> GetQuadPoints(Point3D center, Vector3D side, Vector3D up, double left, double right, double top, double bottom)
        {
            yield return center + (up * bottom) + (side * left);
            yield return center + (up * bottom) + (side * right);
            yield return center + (up * top) + (side * right);
            yield return center + (up * top) + (side * left);
            yield break;
        }

        private static IEnumerable<Point3D> GetMatteQuadPoints(Point3D center, Vector3D side, Vector3D up, int count, double scale)
        {
            // since matte is slightly behind the icons, it needs to be *slightly* larger to avoid Z-order alpha culling from the icons
            scale *= 1.1;
            return count switch
            {
                1 => GetQuadPoints(center, side, up, scale, -scale, scale, -scale),
                2 => GetQuadPoints(center, side, up, scale * 1.5, scale * -1.5, scale * 0.75, scale * -0.75),
                _ => GetQuadPoints(center, side, up, scale, -scale, scale, -scale),
            };
        }

        private static IEnumerable<Point3D> GetIconQuadPoints(Point3D center, Vector3D side, Vector3D up, int count, int index, double scale)
        {
            return count switch
            {
                1 => GetQuadPoints(center, side, up, scale, -scale, scale, -scale),
                2 => index switch
                {
                    0 => GetQuadPoints(center, side, up, scale * 1.5, 0, scale * 0.75, scale * -0.75),
                    _ => GetQuadPoints(center, side, up, 0, scale * -1.5, scale * 0.75, scale * -0.75),
                },
                3 => index switch
                {
                    0 => GetQuadPoints(center, side, up, scale, 0, scale, 0),
                    1 => GetQuadPoints(center, side, up, 0, -scale, scale, 0),
                    _ => GetQuadPoints(center, side, up, scale * 0.5, scale * -0.5, 0, -scale),
                },
                4 => index switch
                {
                    0 => GetQuadPoints(center, side, up, scale, 0, scale, 0),
                    1 => GetQuadPoints(center, side, up, 0, -scale, scale, 0),
                    2 => GetQuadPoints(center, side, up, scale, 0, 0, -scale),
                    _ => GetQuadPoints(center, side, up, 0, -scale, 0, -scale),
                },
                _ => index switch
                {
                    0 => GetQuadPoints(center, side, up, scale, 0, scale, 0),
                    1 => GetQuadPoints(center, side, up, 0, -scale, scale, 0),
                    2 => GetQuadPoints(center, side, up, scale, 0, 0, -scale),
                    3 => GetQuadPoints(center, side, up, 0, -scale, 0, -scale),
                    _ => GetQuadPoints(center, side, up, scale * 0.5, scale * -0.5, scale * 0.5, scale * -0.5),
                },
            };
        }
        #endregion

        #region public static Model3D GenerateMarker(IResolveIcon, List<VisualEffectValue>, List<List<string>>, Point3D, Point3D, AnchorFace, int, double)
        public static Model3D GenerateMarker(IResolveIcon resolver, List<VisualEffectValue> visualEffects,
            List<IconReferenceInfo> iconRefs, Point3D observer, Point3D target, AnchorFace baseFace, int heading, double zoom)
        {
            var _base = visualEffects.FirstOrDefault(_ve => _ve.Type == typeof(SenseEffectExtension))?.Effect ?? VisualEffect.Skip;
            //Debug.WriteLine($@"ModelGenerator.GenerateMarker: obs=({observer}), tgt=({target}), face={baseFace}, hdg={heading}, zoom={zoom}");
            if (iconRefs.Any() && (_base != VisualEffect.Skip) && (_base != VisualEffect.Unseen))
            {
                #region adjustments for detail level
                var _scale = zoom;
                var _detail = IconDetailLevel.Lowest;
                var _distance = (target - observer).Length;
                if (_distance < 120)
                {
                    _scale = zoom * 0.9d;
                    _detail = IconDetailLevel.Low;
                    if (_distance < 60)
                    {
                        _scale = zoom * 0.85d;
                        _detail = IconDetailLevel.Normal;
                        if (_distance < 30)
                        {
                            _scale = zoom * 0.8d;
                            _detail = IconDetailLevel.High;
                            if (_distance < 15)
                            {
                                _scale = zoom * 0.75d;
                                _detail = IconDetailLevel.Highest;
                            }
                        }
                    }
                }
                #endregion

                // get vectors
                var _look = observer - target;
                var _side = Vector3D.CrossProduct(baseFace.GetNormalVector(), _look);
                if (_side.Length == 0)
                {
                    // look and "true up" were co-linear, need to use something else, such as heading
                    _side = Vector3D.CrossProduct(baseFace.GetHeadingVector(heading), _look);
                }
                _side.Normalize();

                // now we can find the natural down vector
                var _up = Vector3D.CrossProduct(_side, _look);
                _up.Normalize();

                var _group = new Model3DGroup();

                // ICONS
                var _materials = (from _iRef in iconRefs
                                  let _icon = (from _key in _iRef.Keys
                                               let _ico = resolver.ResolveIconMaterial(_key, _iRef, _detail)
                                               where _ico != null
                                               select _ico).FirstOrDefault()
                                  where _icon != null
                                  select _icon).Take(5).ToList();

                // build matte quad
                var _mattePoint = observer - (_look * 1.01d);
                var _vector = baseFace.GetNormalVector() * 2.5d;
                var _pediPoint = _mattePoint + _vector;
                var _mQuad = GetMatteQuadPoints(_mattePoint, _side, _up, _materials.Count, _scale).ToList();
                var _mBuild = new MeshBuilder();
                _mBuild.AddQuad(_mQuad[0], _mQuad[1], _mQuad[2], _mQuad[3], new Point(0, 1), new Point(1, 1), new Point(1, 0), new Point(0, 0));
                _mBuild.AddCone(_mattePoint, _vector, 0d, 0.075d, _vector.Length, false, false, 5, TranslateTransform3D.Identity, TranslateTransform3D.Identity);
                _mBuild.AddSphere(_pediPoint, 0.125d, 4, 4);
                var _matteMesh = _mBuild.ToMesh();
                _group.Children.Add(new GeometryModel3D(_matteMesh, _MatteMaterial));

                var _tileIdx = 0;
                foreach (var _material in _materials)
                {
                    // build icon quad
                    var _target = observer - (_look * (1d - (0.0025d * _tileIdx)));
                    var _iQuad = GetIconQuadPoints(_target, _side, _up, _materials.Count, _tileIdx, _scale).ToList();
                    var _iBuild = new MeshBuilder();
                    _iBuild.AddQuad(_iQuad[0], _iQuad[1], _iQuad[2], _iQuad[3], new Point(0, 1), new Point(1, 1), new Point(1, 0), new Point(0, 0));
                    var _markerMesh = _iBuild.ToMesh();
                    _group.Children.Add(new GeometryModel3D(_markerMesh, _material));
                    _tileIdx++;
                }


                // return model
                return _group;
            }
            return null;
        }
        #endregion

        #region public static Model3D GenerateMarker(IResolveIcon, List<VisualEffectValue>, List<List<string>>, Point3D, Point3D, AnchorFace, int)
        public static Model3D GenerateMarker(IResolveIcon resolver, List<VisualEffectValue> visualEffects,
            List<IconReferenceInfo> iconRefs, Point3D observer, Point3D target, AnchorFace baseFace, int heading)
        {
            var _base = visualEffects.FirstOrDefault(_ve => _ve.Type == typeof(SenseEffectExtension))?.Effect ?? VisualEffect.Skip;
            //Debug.WriteLine($@"ModelGenerator.GenerateMarker: obs=({observer}), tgt=({target}), face={baseFace}, hdg={heading}, zoom={zoom}");
            if (iconRefs.Any() && (_base != VisualEffect.Skip) && (_base != VisualEffect.Unseen))
            {
                #region adjustments for detail level
                var _detail = IconDetailLevel.Lowest;
                var _distance = (target - observer).Length;
                if (_distance < 120)
                {
                    _detail = IconDetailLevel.Low;
                    if (_distance < 60)
                    {
                        _detail = IconDetailLevel.Normal;
                        if (_distance < 30)
                        {
                            _detail = IconDetailLevel.High;
                            if (_distance < 15)
                            {
                                _detail = IconDetailLevel.Highest;
                            }
                        }
                    }
                }
                #endregion

                var _group = new Model3DGroup();
                var _mBuild = new MeshBuilder();
                _mBuild.AddSphere(target, 0.5d, 4, 4);
                _mBuild.AddCone(target, baseFace.GetNormalVector(), 0d, 0.075d, 2.5d, false, false, 5, TranslateTransform3D.Identity, TranslateTransform3D.Identity);

                var _matteMesh = _mBuild.ToMesh();
                _group.Children.Add(new GeometryModel3D(_matteMesh, _MatteMaterial));

                // ICONS
                var _materials = (from _iRef in iconRefs
                                  let _icon = (from _key in _iRef.Keys
                                               let _ico = resolver.ResolveIconMaterial(_key, _iRef, _detail)
                                               where _ico != null
                                               select _ico).FirstOrDefault()
                                  where _icon != null
                                  select _icon).Take(5).ToList();

                var _up = baseFace.ReverseFace().GetNormalVector();
                foreach (var _sideFace in baseFace.GetOrthoFaces())
                {
                    _mBuild = new MeshBuilder();
                    var _sideVector = _sideFace.GetNormalVector();
                    var _sideOff = Vector3D.CrossProduct(_sideVector, _up);
                    _sideOff.Normalize();
                    var _mattePoint = target + (_sideVector * 0.7);
                    var _mQuad = GetMatteQuadPoints(_mattePoint, _sideOff, _up, _materials.Count, 0.5).ToList();
                    _mBuild.AddQuad(_mQuad[0], _mQuad[1], _mQuad[2], _mQuad[3], new Point(0, 1), new Point(1, 1), new Point(1, 0), new Point(0, 0));

                    _matteMesh = _mBuild.ToMesh();
                    _group.Children.Add(new GeometryModel3D(_matteMesh, _MatteMaterial));

                    var _tileIdx = 0;
                    foreach (var _material in _materials)
                    {
                        // build icon quad
                        var _target = target + (_sideVector * (0.75d - (0.0025d * _tileIdx)));
                        var _iQuad = GetIconQuadPoints(_target, _sideOff, _up, _materials.Count, _tileIdx, 0.5).ToList();
                        var _iBuild = new MeshBuilder();
                        _iBuild.AddQuad(_iQuad[0], _iQuad[1], _iQuad[2], _iQuad[3], new Point(0, 1), new Point(1, 1), new Point(1, 0), new Point(0, 0));
                        var _markerMesh = _iBuild.ToMesh();
                        _group.Children.Add(new GeometryModel3D(_markerMesh, _material));
                        _tileIdx++;
                    }
                }

                // return model
                return _group;
            }
            return null;
        }
        #endregion

        public static Model3D GeneratePointer(Point3D observer, Vector3D vector, double range, double fade, Material material)
        {
            var _group = new Model3DGroup();
            var _mBuild = new MeshBuilder();
            if (range < 2.5d)
            {
                _mBuild.AddSphere(observer, 0.5d, 4, 4);
            }
            else if (range < 10)
            {
                _mBuild.AddCone(observer, vector, 0d, 0.25d * fade, range, false, true, 7, TranslateTransform3D.Identity, TranslateTransform3D.Identity);
            }
            else
            {
                _mBuild.AddCone(observer, vector, 0d, 0.375d * fade, range, false, true, 7, TranslateTransform3D.Identity, TranslateTransform3D.Identity);
            }

            var _matteMesh = _mBuild.ToMesh();
            _group.Children.Add(new GeometryModel3D(_matteMesh, material));
            return _group;
        }

        public static Model3D GenerateInfoMarker(Point3D observer, Point3D target, bool directionOnly, Material material)
        {
            var _group = new Model3DGroup();
            var _mBuild = new MeshBuilder();

            // vector
            var _vector = target - observer;
            var _range = directionOnly ? Math.Min(7.5d, _vector.Length) : _vector.Length;

            // mesh geometries
            _mBuild.AddCone(observer, _vector, 0, 0.5d, _range, false, true, 7, TranslateTransform3D.Identity, TranslateTransform3D.Identity);
            if (!directionOnly)
            {
                _mBuild.AddSphere(target, 1.25d, 4, 4);
            }

            var _matteMesh = _mBuild.ToMesh();
            _group.Children.Add(new GeometryModel3D(_matteMesh, material));
            return _group;
        }
    }
}
