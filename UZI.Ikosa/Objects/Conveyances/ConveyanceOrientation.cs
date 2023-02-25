using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class ConveyanceOrientation : INotifyPropertyChanged
    {
        public ConveyanceOrientation(Conveyance conveyance)
        {
            _Conveyance = conveyance;
            _Twist = 0;
            _Heading = 0;
            _Upright = Verticality.Upright;
        }

        #region constants
        private const double _SNAP = 1.0d;
        private const double _OFF_IGNORE = 1.0d;
        #endregion

        #region data
        private Conveyance _Conveyance;
        private int _Twist;
        private int _Heading;
        private Verticality _Upright;

        // process caching
        [field: NonSerialized, JsonIgnore]
        private Transform3DGroup _Transform = null;
        #endregion

        // INotifyPropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        public Conveyance Conveyance => _Conveyance;

        #region protected void OnSituated(bool reorient)
        protected void OnSituated(bool newTop)
        {
            _Transform = null;

            if (newTop)
            {
                var _surface = Conveyance.Adjuncts.OfType<SurfaceContainer>().FirstOrDefault(_sc => _sc.IsActive);
                if (_surface != null)
                {
                    // NOTE: once all the members are ejected, the group auto-ejects the master SurfaceContainer
                    foreach (var _member in _surface.Surface.Contained.ToList())
                        _member.Eject();
                }
            }
        }
        #endregion

        /// <summary>Returns 0 if upright or inverted, otherwise returns the proper ortho twist 0..3</summary>
        public int Twist
            => (Upright > Verticality.Upright) && (Upright < Verticality.Inverted) ? _Twist : 0;

        /// <summary>Ortho heading in range of 0..7</summary>
        public int Heading => _Heading;

        public Verticality Upright => _Upright;

        #region public void SetOrientation(Verticality? upright, int? twist, int? heading)
        /// <summary>Attempt to set any or all orientation parameters in one validation sweep</summary>
        public void SetOrientation(Verticality? upright, int? twist, int? heading)
        {
            var _origHeading = Heading;
            var _origUpright = Upright;
            var _origTwist = Twist;
            var _newTop = false;
            var _origSize = SnappableSize;

            #region upright
            if (upright.HasValue)
            {
                // new top and value
                _newTop |= (Upright != upright);
                _Upright = upright.Value;
            }
            #endregion

            #region twist
            // twist
            if (twist.HasValue)
            {
                twist %= 4;
                if (twist < 0)
                    twist += 4;

                // twist only allows a single face to be the new front
                _newTop = (_Twist != twist)
                    && (Upright > Verticality.Upright) && (Upright < Verticality.Inverted);
                _Twist = twist ?? 0;
            }
            #endregion

            // masking twist?
            if (Twist != _Twist)
            {
                // convert twist into heading alteration and remove twist
                heading = (heading ?? Heading) + (_Twist * ((Upright == Verticality.Inverted) ? -1 : 1));
                _Twist = 0;
            }

            #region heading
            if (heading.HasValue)
            {
                // new value
                _Heading = heading.Value % 8;
                if (_Heading < 0)
                    _Heading += 8;
            }
            #endregion

            OnSituated(_newTop);

            // test!
            var _vol = new ConveyanceVolume(Conveyance);
            var _loc = Conveyance?.GetLocated()?.Locator;
            var _cube = _loc?.GeometricRegion as Cubic;
            var _size = SnappableSize;
            var (_fitCube, _fitOffset) = _vol.GetCubicFit(_cube, _size);
            if (_fitCube == null)
            {
                // revert!
                _Heading = _origHeading;
                _Upright = _origUpright;
                _Twist = _origTwist;
                return;
            }
            else
            {
                // set intra-model offset of locator
                if (_loc != null)
                {
                    _loc.IntraModelOffset = _fitOffset;

                    // relocate if size changed
                    if (!_origSize.SameSize(_fitCube))
                        _loc.Relocate(_fitCube, _loc.PlanarPresence);
                }
            }

            // notify
            if (_origHeading != Heading)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Heading)));
            if (_origUpright != Upright)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Upright)));
            if (_origTwist != Twist)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Twist)));
        }
        #endregion

        public AnchorFace GravityFace
            => Conveyance.GetLocated()?.Locator.GetGravityFace() ?? AnchorFace.ZLow;

        #region public bool IsFaceSnapped(AnchorFace face, Vector3D extents, IGeometricSize size)
        /// <summary>
        /// True if the face is snapped to the edge
        /// (must be parallel to a major planar face, and be either snapped, or fill the space)
        /// </summary>
        public bool IsFaceSnapped(AnchorFace face, Vector3D extents)
        {
            // must not be diagonal
            if (Heading % 2 == 1)
                return false;

            const double _precision = 0.041666;
            bool _close(double val)
            {
                var _rslt = Math.Abs(val) <= _precision;
                return _rslt;
            };

            switch (face)
            {
                case AnchorFace.XLow:
                case AnchorFace.XHigh:
                    return _close(extents.X % 5);

                case AnchorFace.YLow:
                case AnchorFace.YHigh:
                    return _close(extents.Y % 5);

                case AnchorFace.ZLow:
                case AnchorFace.ZHigh:
                    return _close(extents.Z % 5);

                default:
                    return false;
            }
        }
        #endregion

        #region public Vector3D SnappableExtents { get; }
        /// <summary>Physical extent in world units</summary>
        public Vector3D SnappableExtents
        {
            get
            {
                var _bottom = GravityFace;
                var _h = Conveyance.Height;
                var _w = Conveyance.Width;
                var _l = Conveyance.Length;

                #region Twist
                switch (Twist)
                {
                    case 0:
                    case 2:
                        // stay...
                        break;

                    case 1:
                    case 3:
                        {
                            // swap length and width if twisted...
                            var _tmp = _l;
                            _l = _w;
                            _w = _tmp;
                        }
                        break;
                }
                #endregion

                #region Not Upright
                switch (Upright)
                {
                    case Verticality.OnSideBottomOut:
                    case Verticality.OnSideTopOut:
                        {
                            // swap height and length if lateral...
                            var _tmp = _h;
                            _h = _l;
                            _l = _tmp;
                        }
                        break;
                }
                #endregion

                var _z = 0d;
                var _y = 0d;
                var _x = 0d;

                #region Heading
                switch (Heading)
                {
                    case 0:
                    case 2:
                        // stay...
                        break;

                    case 1:
                    case 3:
                        {
                            // swap length and width if heading...
                            var _tmp = _l;
                            _l = _w;
                            _w = _tmp;
                        }
                        break;
                }
                #endregion

                #region Base Align
                switch (_bottom.GetAxis())
                {
                    case Axis.Z:
                        _z = _h;
                        _x = _l;
                        _y = _w;
                        break;

                    case Axis.Y:
                        _y = _h;
                        _z = _l;
                        _x = _w;
                        break;

                    default: // X
                        _x = _h;
                        _y = _l;
                        _z = _w;
                        break;
                }
                #endregion

                // NOTE: unrotated only!
                return new Vector3D(_x, _y, _z);
            }
        }
        #endregion

        #region public Vector3D CoverageExtents { get; }
        /// <summary>Physical extent in world units for coverage processes (accounts for IntraModelOffset)</summary>
        public Vector3D CoverageExtents
        {
            get
            {
                // standard terrain hug information
                var _offset = Conveyance.GetLocated()?.Locator.IntraModelOffset
                    ?? new Vector3D();
                var _ext = SnappableExtents;
                return new Vector3D(
                    _ext.X + Math.Abs(_offset.X),
                    _ext.Y + Math.Abs(_offset.Y),
                    _ext.Z + Math.Abs(_offset.Z));
            }
        }
        #endregion

        /// <summary>Size in cell units (rounded up)</summary>
        public IGeometricSize SnappableSize
            => (from _ext in SnappableExtents.ToEnumerable()
                select new GeometricSize(Math.Ceiling(_ext.Z / 5d), Math.Ceiling(_ext.Y / 5d), Math.Ceiling(_ext.X / 5d)))
            .FirstOrDefault();

        #region public IGeometricSize ActualSize{ get; set; }
        public IGeometricSize ActualSize
        {
            get
            {
                var _rgn = Conveyance?.GetLocated()?.Locator.GeometricRegion;
                if (_rgn != null)
                    return new GeometricSize(_rgn);
                return null;
            }
        }
        #endregion

        #region public Vector3D Displacement { get; }
        /// <summary>Combines axis align and locator-based terrain hugging offsets</summary>
        public Vector3D Displacement
        {
            get
            {
                if (Conveyance?.GetLocated()?.Locator is Locator _loc)
                {
                    var _locOffSet = _loc.IntraModelOffset;
                    var _extents = SnappableExtents;
                    var _z = 0d;
                    var _y = 0d;
                    var _x = 0d;
                    var _actual = ActualSize;
                    if (_actual != null)
                    {
                        switch (_loc.GetGravityFace())
                        {
                            case AnchorFace.XHigh:
                                _x = ((_actual.XExtent * 5) - _extents.X) / 2;
                                break;
                            case AnchorFace.XLow:
                                _x = 0 - (((_actual.XExtent * 5) - _extents.X) / 2);
                                break;

                            case AnchorFace.YHigh:
                                _y = ((_actual.YExtent * 5) - _extents.Y) / 2;
                                break;
                            case AnchorFace.YLow:
                                _y = 0 - (((_actual.YExtent * 5) - _extents.Y) / 2);
                                break;

                            case AnchorFace.ZHigh:
                                _z = ((_actual.ZExtent * 5) - _extents.Z) / 2;
                                break;
                            case AnchorFace.ZLow:
                            default:
                                _z = 0 - (((_actual.ZExtent * 5) - _extents.Z) / 2);
                                break;
                        }
                    }
                    return new Vector3D(_x, _y, _z) + _locOffSet;
                }
                return new Vector3D();
            }
        }
        #endregion

        /// <summary>Get Pivot Angle for presentation and point cloud adjustment</summary>
        public double GetModelPivot()
            => Heading * 45;

        #region public Transform3DGroup TransformGroup()
        /// <summary>Used to transform logical point clouds to tactically useful coordinates</summary>
        public Transform3DGroup TransformGroup()
        {
            if (_Transform == null)
            {
                var _group = new Transform3DGroup();

                #region Twist
                // twist
                switch (Twist)
                {
                    case 0:
                        // stay...
                        break;

                    default:
                        // rotate around Z
                        _group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), Twist * 90d)));
                        break;
                }
                #endregion

                #region Not Upright
                if (Upright != Verticality.Upright)
                {
                    // rotate around Y (90 for None, 180 for Low)
                    _group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), -45d * (int)Upright)));
                }
                #endregion

                #region Pivot
                // rotate around Z
                var _pivot = GetModelPivot();// + 90d;
                if (_pivot != 0)
                    _group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), _pivot)));
                #endregion

                #region Displacement
                // intra model offset
                var _offset = Displacement;
                if (_offset.Length > 0)
                    _group.Children.Add(new TranslateTransform3D(_offset));
                #endregion

                #region Location and Base Face
                var _rgn = Conveyance.GetLocated()?.Locator?.GeometricRegion;
                if (_rgn != null)
                {
                    var _size = new GeometricSize(_rgn);

                    // geometric region (from locator?)
                    var _world = new Vector3D(
                        (_rgn.LowerX + (_size.XLength / 2d)) * 5d,
                        (_rgn.LowerY + (_size.YLength / 2d)) * 5d,
                        (_rgn.LowerZ + (_size.ZHeight / 2d)) * 5d);
                    _group.Children.Add(new TranslateTransform3D(_world));

                    // base face
                    var _baseTx = ModelGenerator.GetBaseFaceTransform(GravityFace, _rgn.GetPoint3D());
                    if (_baseTx != null)
                        _group.Children.Add(_baseTx);
                }
                #endregion

                _group.Freeze();
                _Transform = _group;
            }
            return _Transform;
        }
        #endregion

        #region public AnchorFaceList GetAnchorFacesForSideIndex(SideIndex index)
        public AnchorFaceList GetAnchorFacesForSideIndex(SideIndex index)
            => index switch
            {
                // TODO: consider twist may affect this as per furnishing orientation
                SideIndex.Top => Upright switch
                {
                    Verticality.Upright => GravityFace.ReverseFace().ToAnchorFaceList(),
                    Verticality.Inverted => GravityFace.ToAnchorFaceList(),
                    Verticality.OnSideBottomOut => GravityFace.GetHeadingFaces(Heading).Invert(),
                    Verticality.OnSideTopOut => GravityFace.GetHeadingFaces(Heading),
                    _ => GravityFace.ReverseFace().ToAnchorFaceList(),
                },
                SideIndex.Bottom => Upright switch
                {
                    Verticality.Upright => GravityFace.ToAnchorFaceList(),
                    Verticality.Inverted => GravityFace.ReverseFace().ToAnchorFaceList(),
                    Verticality.OnSideBottomOut => GravityFace.GetHeadingFaces(Heading),
                    Verticality.OnSideTopOut => GravityFace.GetHeadingFaces(Heading).Invert(),
                    _ => GravityFace.ToAnchorFaceList(),
                },
                SideIndex.Front => Upright switch
                {
                    Verticality.Upright => GravityFace.GetHeadingFaces(Heading),
                    Verticality.Inverted => GravityFace.GetHeadingFaces(Heading).Invert(),
                    Verticality.OnSideBottomOut => Twist switch
                    {
                        0 => GravityFace.ToAnchorFaceList().Invert(),
                        1 => GravityFace.GetHeadingFaces(Heading + 2),
                        2 => GravityFace.ToAnchorFaceList(),
                        3 => GravityFace.GetHeadingFaces(Heading + 6),
                        _ => GravityFace.ToAnchorFaceList().Invert(),
                    },
                    Verticality.OnSideTopOut => Twist switch
                    {
                        0 => GravityFace.ToAnchorFaceList(),
                        1 => GravityFace.GetHeadingFaces(Heading + 2),
                        2 => GravityFace.ToAnchorFaceList().Invert(),
                        3 => GravityFace.GetHeadingFaces(Heading + 6),
                        _ => GravityFace.ToAnchorFaceList(),
                    },
                    _ => GravityFace.GetHeadingFaces(Heading),
                },
                SideIndex.Back => Upright switch
                {
                    Verticality.Upright => GravityFace.GetHeadingFaces(Heading).Invert(),
                    Verticality.Inverted => GravityFace.GetHeadingFaces(Heading),
                    Verticality.OnSideBottomOut => Twist switch
                    {
                        0 => GravityFace.ToAnchorFaceList(),
                        1 => GravityFace.GetHeadingFaces(Heading + 6),
                        2 => GravityFace.ToAnchorFaceList().Invert(),
                        3 => GravityFace.GetHeadingFaces(Heading + 2),
                        _ => GravityFace.ToAnchorFaceList(),
                    },
                    Verticality.OnSideTopOut => Twist switch
                    {
                        0 => GravityFace.ToAnchorFaceList().Invert(),
                        1 => GravityFace.GetHeadingFaces(Heading + 6),
                        2 => GravityFace.ToAnchorFaceList(),
                        3 => GravityFace.GetHeadingFaces(Heading + 2),
                        _ => GravityFace.ToAnchorFaceList().Invert(),
                    },
                    _ => GravityFace.GetHeadingFaces(Heading).Invert(),
                },
                SideIndex.Left => Upright switch
                {
                    Verticality.Upright => GravityFace.GetHeadingFaces(Heading + 2),
                    Verticality.Inverted => GravityFace.GetHeadingFaces(Heading + 2),
                    Verticality.OnSideBottomOut => Twist switch
                    {
                        0 => GravityFace.GetHeadingFaces(Heading + 2),
                        1 => GravityFace.ToAnchorFaceList(),
                        2 => GravityFace.GetHeadingFaces(Heading + 6),
                        3 => GravityFace.ToAnchorFaceList().Invert(),
                        _ => GravityFace.GetHeadingFaces(Heading + 2),
                    },
                    Verticality.OnSideTopOut => Twist switch
                    {
                        0 => GravityFace.GetHeadingFaces(Heading + 2),
                        1 => GravityFace.ToAnchorFaceList().Invert(),
                        2 => GravityFace.GetHeadingFaces(Heading + 6),
                        3 => GravityFace.ToAnchorFaceList(),
                        _ => GravityFace.GetHeadingFaces(Heading + 2),
                    },
                    _ => GravityFace.GetHeadingFaces(Heading + 2),
                },
                SideIndex.Right => Upright switch
                {
                    Verticality.Upright => GravityFace.GetHeadingFaces(Heading + 6),
                    Verticality.Inverted => GravityFace.GetHeadingFaces(Heading + 6),
                    Verticality.OnSideBottomOut => Twist switch
                    {
                        0 => GravityFace.GetHeadingFaces(Heading + 6),
                        1 => GravityFace.ToAnchorFaceList().Invert(),
                        2 => GravityFace.GetHeadingFaces(Heading + 2),
                        3 => GravityFace.ToAnchorFaceList(),
                        _ => GravityFace.GetHeadingFaces(Heading + 6),
                    },
                    Verticality.OnSideTopOut => Twist switch
                    {
                        0 => GravityFace.GetHeadingFaces(Heading + 6),
                        1 => GravityFace.ToAnchorFaceList(),
                        2 => GravityFace.GetHeadingFaces(Heading + 2),
                        3 => GravityFace.ToAnchorFaceList().Invert(),
                        _ => GravityFace.GetHeadingFaces(Heading + 6),
                    },
                    _ => GravityFace.GetHeadingFaces(Heading + 6),
                },
                _ => GravityFace.ToAnchorFaceList().Invert(),
            };
        #endregion

        #region public IEnumerable<KeyValuePair<Type, VisualEffect>> VisualEffects(IGeometricRegion location, IList<SensoryBase> filteredSenses, VisualEffect standard)
        public IEnumerable<KeyValuePair<Type, VisualEffect>> VisualEffects(IGeometricRegion location, IList<SensoryBase> filteredSenses, VisualEffect standard)
        {
            yield return new KeyValuePair<Type, VisualEffect>(typeof(FrontSenseEffectExtension), standard);
            yield return new KeyValuePair<Type, VisualEffect>(typeof(BackSenseEffectExtension), standard);
            yield return new KeyValuePair<Type, VisualEffect>(typeof(TopSenseEffectExtension), standard);
            yield return new KeyValuePair<Type, VisualEffect>(typeof(BottomSenseEffectExtension), standard);
            yield return new KeyValuePair<Type, VisualEffect>(typeof(LeftSenseEffectExtension), standard);
            yield return new KeyValuePair<Type, VisualEffect>(typeof(RightSenseEffectExtension), standard);
            yield break;
        }
        #endregion
    }
}
