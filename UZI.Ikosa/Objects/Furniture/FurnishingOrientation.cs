using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class FurnishingOrientation : INotifyPropertyChanged, IDeserializationCallback
    {
        public FurnishingOrientation(Furnishing furniture)
        {
            _Furnishing = furniture;
            _Twist = 0;
            _Heading = 0;
            _High = new bool[3];
            _Upright = Verticality.Upright;
            _Planar = new ConcurrentDictionary<AnchorFaceList, List<PlanarPoints>>();
        }

        #region data
        private Furnishing _Furnishing;
        private int _Twist;
        private int _Heading;
        private Verticality _Upright;
        private bool[] _High;

        // process caching
        [field: NonSerialized, JsonIgnore]
        private Transform3DGroup _Transform = null;
        [field: NonSerialized, JsonIgnore]
        private ConcurrentDictionary<AnchorFaceList, List<PlanarPoints>> _Planar = null;
        #endregion

        // INotifyPropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        public Furnishing Furnishing => _Furnishing;

        #region protected virtual void OnSituated(bool reorient)
        // TODO: this may need to be replaced with new process stream
        // TODO: ... or become augmented to handle everything ...

        /// <summary>Ensures snap to gravity face and heading face, may clear surface container if new top</summary>
        protected virtual void OnSituated(bool newTop)
        {
            // snap to gravity
            var _gravFace = GravityFace;
            switch (_gravFace.GetAxis())
            {
                case Axis.Z:
                    _High[0] = !_gravFace.IsLowFace();
                    break;

                case Axis.Y:
                    _High[1] = !_gravFace.IsLowFace();
                    break;

                default:
                    _High[2] = !_gravFace.IsLowFace();
                    break;
            }

            // ensure snapped to heading face, and not anti-heading face
            var _headFace = _gravFace.GetHeadingFaces(_Heading * 2)
                .ToAnchorFaces().FirstOrDefault();
            switch (_headFace.GetAxis())
            {
                case Axis.Z:
                    _High[0] = !_headFace.IsLowFace();
                    break;

                case Axis.Y:
                    _High[1] = !_headFace.IsLowFace();
                    break;

                case Axis.X:
                default:
                    _High[2] = !_headFace.IsLowFace();
                    break;
            }
            _Transform = null;
            _Planar.Clear();

            if (newTop)
            {
                var _surface = Furnishing.Adjuncts.OfType<SurfaceContainer>().FirstOrDefault(_sc => _sc.IsActive);
                if (_surface != null)
                {
                    // NOTE: once all the members are ejected, the group auto-ejects the master SurfaceContainer
                    foreach (var _member in _surface.Surface.Contained.ToList())
                    {
                        _member.Eject();
                    }
                }
            }
        }
        #endregion

        public int Twist => _Twist;

        /// <summary>Ortho heading in range of 0..3</summary>
        public int Heading => _Heading;

        /// <summary>
        /// Upright = 0 | OnSideTopOut = 2 | Inverted = 4 | OnSideBottomOut = 6
        /// </summary>
        public Verticality Upright => _Upright;

        #region public void SetOrientation(Verticality? upright, int? twist, int? heading)
        /// <summary>Attempt to set any or all orientation parameters in one validation sweep</summary>
        public void SetOrientation(Verticality? upright, int? twist, int? heading)
        {
            var _origHeading = Heading;
            var _origUpright = Upright;
            var _origTwist = Twist;
            var _origSnap = (bool[])_High.Clone();
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
            if (twist.HasValue)
            {
                // twist
                twist %= 4;
                if (twist < 0)
                {
                    twist += 4;
                }

                // twist only allows a single face to be the new front
                _newTop |= Upright switch
                {
                    Verticality.OnSideBottomOut => _Twist != twist,
                    Verticality.OnSideTopOut => _Twist != twist,
                    _ => false
                };
                _Twist = twist ?? 0;
            }
            #endregion

            #region heading
            if (heading.HasValue)
            {
                // new value
                _Heading = heading.Value % 4;
                if (_Heading < 0)
                {
                    _Heading += 4;
                }
            }
            #endregion

            // ensure appropriate snaps for gravity and heading
            OnSituated(_newTop);

            // test!
            var _vol = new FurnishingVolume(Furnishing);
            var _loc = Furnishing?.GetLocated()?.Locator;
            var _cube = _loc?.GeometricRegion as Cubic;
            var _size = SnappableSize;
            var (_fitCube, _fitOffset) = _vol.GetCubicFit(_cube, _size, ZHighSnap, YHighSnap, XHighSnap,
                _loc?.PlanarPresence ?? PlanarPresence.Material);
            if (_fitCube == null)
            {
                // revert!
                _Heading = _origHeading;
                _Upright = _origUpright;
                _Twist = _origTwist;
                _High[0] = _origSnap[0];
                _High[1] = _origSnap[1];
                _High[2] = _origSnap[2];
                return;
            }
            else
            {
                if (_loc != null)
                {
                    // set intra-model offset of locator
                    _loc.IntraModelOffset = _fitOffset;

                    // relocate if size changed
                    if (!_origSize.SameSize(_fitCube))
                    {
                        _loc.Relocate(_fitCube, _loc.PlanarPresence);
                    }
                }
            }

            // notify
            if (_origHeading != Heading)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Heading)));
            }

            if (_origUpright != Upright)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Upright)));
            }

            if (_origTwist != Twist)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Twist)));
            }
        }
        #endregion

        #region public void SetAxisSnap(Axis axis, bool isHigh)
        public void SetAxisSnap(Axis axis, bool isHigh)
        {
            switch (axis)
            {
                case Axis.Z:
                    _High[0] = isHigh;
                    break;

                case Axis.Y:
                    _High[1] = isHigh;
                    break;

                case Axis.X:
                default:
                    _High[2] = isHigh;
                    break;
            }

            OnSituated(false);

            // adjust offset (snap changes ***shouldn't*** change volume)
            var _vol = new FurnishingVolume(Furnishing);
            var _loc = Furnishing.GetLocated()?.Locator;
            var _cube = _loc?.GeometricRegion as Cubic;
            var (_fitCube, _fitOffset) = _vol.GetCubicFit(_cube, SnappableSize, ZHighSnap, YHighSnap, XHighSnap,
                _loc?.PlanarPresence ?? PlanarPresence.Material);
            if ((_fitCube != null) && (_loc != null))
            {
                // set intra-model offset of locator
                _loc.IntraModelOffset = _fitOffset;
            }
        }
        #endregion

        public bool ZHighSnap => _High[0];
        public bool YHighSnap => _High[1];
        public bool XHighSnap => _High[2];

        /// <summary>Get AxisSnap by Axis</summary>
        public AxisSnap GetAxisSnap(Axis axis)
            => axis == Axis.Z ? (_High[0] ? AxisSnap.High : AxisSnap.Low)
            : axis == Axis.Y ? (_High[1] ? AxisSnap.High : AxisSnap.Low)
            : (_High[2] ? AxisSnap.High : AxisSnap.Low);

        public AnchorFaceList GetAxisSnappedFaces()
            => AxisSnapHelper.ToAnchorFaceList(GetAxisSnap(Axis.Z), GetAxisSnap(Axis.Y), GetAxisSnap(Axis.X));

        /// <summary>Gravity face of locator for furnishing</summary>
        public AnchorFace GravityFace
            => Furnishing.GetLocated()?.Locator.GetGravityFace() ?? AnchorFace.ZLow;

        /// <summary>
        /// True if the face is snapped to the edge
        /// (must be parallel to a major planar face, and be either snapped, or fill the space)
        /// </summary>
        public bool IsFaceSnapped(AnchorFace face)
            => IsFaceSnapped(face, CoverageExtents);

        #region public bool IsFaceSnapped(AnchorFace face, Vector3D extents, IGeometricSize size)
        /// <summary>
        /// True if the face is snapped to the edge
        /// (must be parallel to a major planar face, and be either snapped, or fill the space)
        /// </summary>
        public bool IsFaceSnapped(AnchorFace face, Vector3D extents)
        {
            const double _precision = 0.041666;
            bool _close(double val)
            {
                var _rslt = Math.Abs(val) <= _precision;
                return _rslt;
            };

            switch (face)
            {
                case AnchorFace.XLow:
                    if (XHighSnap)
                    {
                        return _close(extents.X % 5);
                    }
                    return true;

                case AnchorFace.XHigh:
                    if (XHighSnap)
                    {
                        return true;
                    }
                    return _close(extents.X % 5);

                case AnchorFace.YLow:
                    if (YHighSnap)
                    {
                        return _close(extents.Y % 5);
                    }
                    return true;

                case AnchorFace.YHigh:
                    if (YHighSnap)
                    {
                        return true;
                    }
                    return _close(extents.Y % 5);

                case AnchorFace.ZLow:
                    if (ZHighSnap)
                    {
                        return _close(extents.Z % 5);
                    }
                    return true;

                case AnchorFace.ZHigh:
                    if (ZHighSnap)
                    {
                        return true;
                    }
                    return _close(extents.Z % 5);

                default:
                    return false;
            }
        }
        #endregion

        public bool IsHeadingTwisted
        {
            get
            {
                if ((Upright == Verticality.Upright)
                    || (Upright == Verticality.Inverted))
                {
                    var _grav = GravityFace;
                    var _headAxis = _grav.GetHeadingFaces(Heading * 2).ToAnchorFaces().FirstOrDefault().GetAxis();
                    var _crossAxis = _grav.GetHeadingFaces((Heading + 1) * 2).ToAnchorFaces().FirstOrDefault().GetAxis();
                    var _extents = SnappableExtents;
                    return _headAxis switch
                    {
                        Axis.X => _crossAxis switch
                        {
                            Axis.Y => _extents.X > _extents.Y,
                            _ => _extents.X > _extents.Z,
                        },
                        Axis.Y => _crossAxis switch
                        {
                            Axis.X => _extents.Y > _extents.X,
                            _ => _extents.Y > _extents.Z,
                        },
                        _ => _crossAxis switch
                        {
                            Axis.X => _extents.Z > _extents.X,
                            _ => _extents.Z > _extents.Y,
                        },
                    };
                }
                return false;
            }
        }

        #region public Vector3D SnappableExtents { get; }
        /// <summary>Physical extent in world units</summary>
        public Vector3D SnappableExtents
        {
            get
            {
                var _bottom = GravityFace;
                var _h = Furnishing.Height;
                var _w = Furnishing.Width;
                var _l = Furnishing.Length;

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
                var _offset = Furnishing.GetLocated()?.Locator.IntraModelOffset
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
                var _rgn = Furnishing?.GetLocated()?.Locator.GeometricRegion;
                if (_rgn != null)
                {
                    return new GeometricSize(_rgn);
                }

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
                var _locOffSet = Furnishing.GetLocated()?.Locator.IntraModelOffset
                    ?? new Vector3D();

                var (_z, _y, _x) = (0d, 0d, 0d);
                var _actual = ActualSize;
                if (_actual != null)
                {
                    var _extents = SnappableExtents;
                    var _gravity = GravityFace;

                    double _snap(double actual, double extent, AnchorFace hardSnap)
                    {
                        var _act = actual * 5;
                        if (_act > (2 * extent))
                        {
                            // extent is less than half the actual space
                            if ((hardSnap == _gravity) || Furnishing.IsHardSnap(hardSnap))
                            {
                                // hard snap
                                return (_act - extent) / 2d;
                            }

                            // buffer snap
                            return _act / 4d;
                        }
                        return (_act - extent) / 2d;
                    }

                    // furniture is always snapped to cell faces in this world
                    _z = (ZHighSnap)
                        ? _snap(_actual.ZExtent, _extents.Z, AnchorFace.ZHigh)
                        : 0 - _snap(_actual.ZExtent, _extents.Z, AnchorFace.ZLow);

                    _y = (YHighSnap)
                        ? _snap(_actual.YExtent, _extents.Y, AnchorFace.YHigh)
                        : 0 - _snap(_actual.YExtent, _extents.Y, AnchorFace.YLow);

                    _x = (XHighSnap)
                        ? _snap(_actual.XExtent, _extents.X, AnchorFace.XHigh)
                        : 0 - _snap(_actual.XExtent, _extents.X, AnchorFace.XLow);
                }
                return new System.Windows.Media.Media3D.Vector3D(_x, _y, _z) + _locOffSet;
            }
        }
        #endregion

        /// <summary>Get Pivot Angle for presentation and point cloud adjustment</summary>
        public double GetModelPivot()
            => Heading * 90;

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
                {
                    _group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), _pivot)));
                }
                #endregion

                #region Displacement
                // intra model offset
                var _offset = Displacement;
                if (_offset.Length > 0)
                {
                    _group.Children.Add(new TranslateTransform3D(_offset));
                }

                #endregion

                #region Location and Base Face
                var _rgn = Furnishing.GetLocated()?.Locator?.GeometricRegion;
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
                    {
                        _group.Children.Add(_baseTx);
                    }
                }
                #endregion

                _group.Freeze();
                _Transform = _group;
            }
            return _Transform;
        }
        #endregion

        #region protected CellLocation OffsetCell(Locator locator, AnchorFace? face)
        protected IEnumerable<CellLocation> OffsetCell(Locator locator, AnchorFace face)
        {
            var _rgn = locator.GeometricRegion;
            foreach (var _cell in _rgn.AllCellLocations())
            {
                if (_rgn.IsCellAtSurface(_cell, face))
                {
                    yield return CellLocation.GetAdjacentCellLocation(_cell, face);
                }
            }

            yield break;
        }
        #endregion

        #region protected static VisualEffect OffsetVisualEffect(IGeometricRegion location, IList<SensoryBase> filteredSenses, Locator locator, CellLocation cells)
        protected static VisualEffect OffsetVisualEffect(IGeometricRegion location,
            IList<SensoryBase> filteredSenses, Locator locator, IEnumerable<CellLocation> cells)
        {
            var _bestEffect = VisualEffect.Unseen;
            foreach (var _cell in cells)
            {
                var _distance = IGeometricHelper.Distance(location.GetPoint3D(), _cell);
                var _effect = VisualEffect.Unseen;

                #region magic dark
                // any magic darks on objects in this locator
                if (locator.Map.IsInMagicDarkness(_cell))
                {
                    // dim under magic dark
                    _effect = VisualEffect.DimTo50;

                    foreach (var _magicDarkPiercing in filteredSenses.Where(_s => _s.IgnoresVisualEffects
                        && (_distance <= _s.Range)))
                    {
                        if (!_magicDarkPiercing.UsesSenseTransit
                            || _magicDarkPiercing.CarrySenseInteraction(locator.Map, location, _cell,
                            ITacticalInquiryHelper.EmptyArray))
                        {
                            if (_magicDarkPiercing.UsesSight)
                            {
                                // obtained highest detail
                                _effect = VisualEffect.Normal;
                                break;
                            }
                            else
                            {
                                _effect = VisualEffect.FormOnly;
                            }
                        }
                    }
                }
                #endregion

                // still need to determine visual effect
                if (_effect == VisualEffect.Unseen)
                {
                    // in range visual senses
                    var _level = locator.Map.GetLightLevel(_cell);
                    // TODO: max over all locations
                    var _sight = filteredSenses.Where(_s => _s.UsesSight && _s.Range >= _distance).ToList();
                    _effect = VisualEffectHandler.GetSightedVisualEffect(_sight, _level, _distance);

                    // if not normal or monochrome, allow non-sighted sense a chance to affect visualization
                    if ((_effect != VisualEffect.Brighter)
                        && (_effect != VisualEffect.Normal)
                        && (_effect != VisualEffect.Monochrome))
                    {
                        foreach (var _formSense in filteredSenses.Where(_s => !_s.UsesSight
                            && (_distance <= _s.Range)))
                        {
                            if (!_formSense.UsesSenseTransit
                                || _formSense.CarrySenseInteraction(locator.Map, location, _cell,
                                ITacticalInquiryHelper.EmptyArray))
                            {
                                _effect = VisualEffect.FormOnly;
                            }
                        }
                    }
                }

                if (_bestEffect.GetEffectRank() < _effect.GetEffectRank())
                {
                    _bestEffect = _effect;
                }

                if (_bestEffect == VisualEffect.Brighter)
                {
                    break;
                }
            }
            return _bestEffect;
        }
        #endregion

        #region public AnchorFace GetAnchorFaceForSideIndex(SideIndex index)
        /// <summary>Yields the cartesian anchor face for the furnishing side based upon current orientation</summary>
        public AnchorFace GetAnchorFaceForSideIndex(SideIndex index)
        {
            var _ht = (Heading + Twist) % 4;
            return index switch
            {
                SideIndex.Top => Upright switch
                {
                    Verticality.Upright => GravityFace.ReverseFace(),
                    Verticality.Inverted => GravityFace,
                    Verticality.OnSideBottomOut => GravityFace.BackFace(Heading * 2).Value,
                    Verticality.OnSideTopOut => GravityFace.FrontFace(Heading * 2).Value,
                    _ => GravityFace.ReverseFace(),
                },
                SideIndex.Bottom => Upright switch
                {
                    Verticality.Upright => GravityFace,
                    Verticality.Inverted => GravityFace.ReverseFace(),
                    Verticality.OnSideBottomOut => GravityFace.FrontFace(Heading * 2).Value,
                    Verticality.OnSideTopOut => GravityFace.BackFace(Heading * 2).Value,
                    _ => GravityFace,
                },
                SideIndex.Front => Upright switch
                {
                    Verticality.Upright => GravityFace.FrontFace(_ht * 2).Value,
                    Verticality.Inverted => GravityFace.BackFace(_ht * 2).Value,
                    Verticality.OnSideBottomOut => Twist switch
                    {
                        0 => GravityFace.ReverseFace(),
                        1 => GravityFace.LeftFace(Heading * 2).Value,
                        2 => GravityFace,
                        3 => GravityFace.RightFace(Heading * 2).Value,
                        _ => GravityFace.ReverseFace(),
                    },
                    Verticality.OnSideTopOut => Twist switch
                    {
                        0 => GravityFace,
                        1 => GravityFace.LeftFace(Heading * 2).Value,
                        2 => GravityFace.ReverseFace(),
                        3 => GravityFace.RightFace(Heading * 2).Value,
                        _ => GravityFace,
                    },
                    _ => GravityFace.FrontFace(_ht * 2).Value,
                },
                SideIndex.Back => Upright switch
                {
                    Verticality.Upright => GravityFace.BackFace(_ht * 2).Value,
                    Verticality.Inverted => GravityFace.FrontFace(_ht * 2).Value,
                    Verticality.OnSideBottomOut => Twist switch
                    {
                        0 => GravityFace,
                        1 => GravityFace.RightFace(Heading * 2).Value,
                        2 => GravityFace.ReverseFace(),
                        3 => GravityFace.LeftFace(Heading * 2).Value,
                        _ => GravityFace,
                    },
                    Verticality.OnSideTopOut => Twist switch
                    {
                        0 => GravityFace.ReverseFace(),
                        1 => GravityFace.RightFace(Heading * 2).Value,
                        2 => GravityFace,
                        3 => GravityFace.LeftFace(Heading * 2).Value,
                        _ => GravityFace.ReverseFace(),
                    },
                    _ => GravityFace.BackFace((Heading + Twist) * 2).Value,
                },
                SideIndex.Left => Upright switch
                {
                    Verticality.Upright => GravityFace.LeftFace(_ht * 2).Value,
                    Verticality.Inverted => GravityFace.LeftFace(_ht * 2).Value,
                    Verticality.OnSideBottomOut => Twist switch
                    {
                        0 => GravityFace.LeftFace(Heading * 2).Value,
                        1 => GravityFace,
                        2 => GravityFace.RightFace(Heading * 2).Value,
                        3 => GravityFace.ReverseFace(),
                        _ => GravityFace.LeftFace(Heading * 2).Value,
                    },
                    Verticality.OnSideTopOut => Twist switch
                    {
                        0 => GravityFace.LeftFace(Heading * 2).Value,
                        1 => GravityFace.ReverseFace(),
                        2 => GravityFace.RightFace(Heading * 2).Value,
                        3 => GravityFace,
                        _ => GravityFace.LeftFace(Heading * 2).Value,
                    },
                    _ => GravityFace.LeftFace(_ht * 2).Value
                },
                SideIndex.Right => Upright switch
                {
                    Verticality.Upright => GravityFace.RightFace(_ht * 2).Value,
                    Verticality.Inverted => GravityFace.RightFace(_ht * 2).Value,
                    Verticality.OnSideBottomOut => Twist switch
                    {
                        0 => GravityFace.RightFace(Heading * 2).Value,
                        1 => GravityFace.ReverseFace(),
                        2 => GravityFace.LeftFace(Heading * 2).Value,
                        3 => GravityFace,
                        _ => GravityFace.RightFace(Heading * 2).Value,
                    },
                    Verticality.OnSideTopOut => Twist switch
                    {
                        0 => GravityFace.RightFace(Heading * 2).Value,
                        1 => GravityFace,
                        2 => GravityFace.LeftFace(Heading * 2).Value,
                        3 => GravityFace.ReverseFace(),
                        _ => GravityFace.RightFace(Heading * 2).Value,
                    },
                    _ => GravityFace.RightFace(_ht * 2).Value
                },
                _ => GravityFace.ReverseFace(),
            };
        }
        #endregion

        #region public IEnumerable<KeyValuePair<Type, VisualEffect>> VisualEffects(IGeometricRegion location, IList<SensoryBase> filteredSenses, VisualEffect standard)
        public IEnumerable<KeyValuePair<Type, VisualEffect>> VisualEffects(IGeometricRegion location, IList<SensoryBase> filteredSenses, VisualEffect standard)
        {
            if (!filteredSenses.Any(_s => _s.UsesSight))
            {
                yield return new KeyValuePair<Type, VisualEffect>(typeof(FrontSenseEffectExtension), standard);
                yield return new KeyValuePair<Type, VisualEffect>(typeof(BackSenseEffectExtension), standard);
                yield return new KeyValuePair<Type, VisualEffect>(typeof(TopSenseEffectExtension), standard);
                yield return new KeyValuePair<Type, VisualEffect>(typeof(BottomSenseEffectExtension), standard);
                yield return new KeyValuePair<Type, VisualEffect>(typeof(LeftSenseEffectExtension), standard);
                yield return new KeyValuePair<Type, VisualEffect>(typeof(RightSenseEffectExtension), standard);
            }
            else
            {
                var _locator = Furnishing.GetLocated().Locator;
                var _extents = CoverageExtents;

                #region VisualEffect _effect(AnchorFace? anchor) {...}
                VisualEffect _effect(AnchorFace? anchor)
                {
                    if (!anchor.HasValue)
                    {
                        return standard;
                    }

                    if (!IsFaceSnapped(anchor.Value, _extents))
                    {
                        return standard;
                    }

                    return OffsetVisualEffect(location, filteredSenses, _locator, OffsetCell(_locator, anchor.Value));
                };
                #endregion

                yield return new KeyValuePair<Type, VisualEffect>(typeof(TopSenseEffectExtension), _effect(GetAnchorFaceForSideIndex(SideIndex.Top)));
                yield return new KeyValuePair<Type, VisualEffect>(typeof(BottomSenseEffectExtension), _effect(GetAnchorFaceForSideIndex(SideIndex.Bottom)));
                yield return new KeyValuePair<Type, VisualEffect>(typeof(FrontSenseEffectExtension), _effect(GetAnchorFaceForSideIndex(SideIndex.Front)));
                yield return new KeyValuePair<Type, VisualEffect>(typeof(BackSenseEffectExtension), _effect(GetAnchorFaceForSideIndex(SideIndex.Back)));
                yield return new KeyValuePair<Type, VisualEffect>(typeof(LeftSenseEffectExtension), _effect(GetAnchorFaceForSideIndex(SideIndex.Left)));
                yield return new KeyValuePair<Type, VisualEffect>(typeof(RightSenseEffectExtension), _effect(GetAnchorFaceForSideIndex(SideIndex.Right)));
            }
            yield break;
        }
        #endregion

        #region public AnchorFaceList GetCellFaceIndex(ICellLocation location, Axis axis)
        /// <summary>Selects cell face index for the location</summary>
        public AnchorFaceList GetCellFaceIndex(ICellLocation location, Axis axis)
        {
            var _rgn = Furnishing?.GetLocated()?.Locator?.GeometricRegion;
            if (_rgn != null)
            {
                var _size = new GeometricSize(_rgn);
                AnchorFaceList _ord(long range, int pos, int upper, int lower, Axis tAxis)
                {
                    switch (range)
                    {
                        case 2:
                            // range of 2 can give lower OR upper
                            if (pos == upper)
                            {
                                return tAxis.GetHighFace().ToAnchorFaceList();
                            }
                            else
                            {
                                return tAxis.GetLowFace().ToAnchorFaceList();
                            }

                        default:
                            // range of 1 can only give lower AND upper
                            return tAxis.GetLowFace().ToAnchorFaceList().Add(tAxis.GetHighFace());
                    }
                };
                return axis switch
                {
                    Axis.Z => _ord(_size.XLength, location.X, _rgn.UpperX, _rgn.LowerX, Axis.X)
                            | _ord(_size.YLength, location.Y, _rgn.UpperY, _rgn.LowerY, Axis.Y),
                    Axis.Y => _ord(_size.XLength, location.X, _rgn.UpperX, _rgn.LowerX, Axis.X)
                            | _ord(_size.ZHeight, location.Z, _rgn.UpperZ, _rgn.LowerZ, Axis.Z),
                    _ => _ord(_size.YLength, location.Y, _rgn.UpperY, _rgn.LowerY, Axis.Y)
                            | _ord(_size.ZHeight, location.Z, _rgn.UpperZ, _rgn.LowerZ, Axis.Z),
                };
            }
            return AnchorFaceList.None;
        }
        #endregion

        #region public HedralGrip GetHedralCoverage(ICellLocation location, Axis axis)
        public HedralGrip GetHedralCoverage(ICellLocation location, Axis axis)
        {
            var _extents = CoverageExtents;
            var _loc = Furnishing?.GetLocated()?.Locator;
            var _rgn = _loc?.GeometricRegion;
            if (_rgn != null)
            {
                var _size = new GeometricSize(_rgn);

                // each dimension is 0..5d
                HedralGrip _grip(long range, int pos, int lower, bool highSnap, double extent, AnchorFace face)
                {
                    switch (range)
                    {
                        case 2:
                            if ((pos == lower) ^ highSnap)
                            {
                                return new HedralGrip(true);
                            }

                            return new HedralGrip(axis, face, extent - 5d);

                        default:
                            return new HedralGrip(axis, face, extent);
                    }
                };

                return axis switch
                {
                    Axis.Z => _grip(_size.XLength, location.X, _rgn.LowerX, XHighSnap, _extents.X, XHighSnap ? AnchorFace.XHigh : AnchorFace.XLow)
                        .Intersect(_grip(_size.YLength, location.Y, _rgn.LowerY, YHighSnap, _extents.Y, YHighSnap ? AnchorFace.YHigh : AnchorFace.YLow)),
                    Axis.Y => _grip(_size.XLength, location.X, _rgn.LowerX, XHighSnap, _extents.X, XHighSnap ? AnchorFace.XHigh : AnchorFace.XLow)
                        .Intersect(_grip(_size.ZHeight, location.Z, _rgn.LowerZ, ZHighSnap, _extents.Z, ZHighSnap ? AnchorFace.ZHigh : AnchorFace.ZLow)),
                    _ => _grip(_size.YLength, location.Y, _rgn.LowerY, YHighSnap, _extents.Y, YHighSnap ? AnchorFace.YHigh : AnchorFace.YLow)
                        .Intersect(_grip(_size.ZHeight, location.Z, _rgn.LowerZ, ZHighSnap, _extents.Z, ZHighSnap ? AnchorFace.ZHigh : AnchorFace.ZLow)),
                };
            }
            return new HedralGrip(false);
        }
        #endregion

        public double GetCoverageExtent(Axis axis)
            => axis switch
            {
                Axis.X => CoverageExtents.X,
                Axis.Y => CoverageExtents.Y,
                Axis.Z => CoverageExtents.Z,
                _ => CoverageExtents.Z,
            };

        private ConcurrentDictionary<AnchorFaceList, List<PlanarPoints>> GetPlanar()
            => _Planar;

        #region public List<PlanarPoints> GetPlanarPoints(AnchorFaceList faces)
        public List<PlanarPoints> GetPlanarPoints(AnchorFaceList faces)
        {
            return GetPlanar().GetOrAdd(faces, (gppFaces) =>
            {
                var _trans = TransformGroup();
                return gppFaces.ToAnchorFaces()
                    .Select(_f =>
                    {
                        var _corners = Furnishing.GetDimensionalCorners(_f);
                        if (_trans.Children.Any())
                        {
                            _trans.Transform(_corners);
                        }

                        return new PlanarPoints(_corners);
                    }).ToList();
            });
        }
        #endregion

        public void OnDeserialization(object sender)
        {
            _Planar = new ConcurrentDictionary<AnchorFaceList, List<PlanarPoints>>();
        }
    }
}
