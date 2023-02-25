using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Senses;
using Uzi.Visualize;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Actions;
using Uzi.Visualize.Contracts.Tactical;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class SlidingPortal : PortalBase, ITacticalInquiry, IAlterLocalLink, IMoveAlterer
    {
        #region Construction
        public SlidingPortal(string name, AnchorFace anchorFace, Axis slidingAxis, double distance, double zOff, double yOff, double xOff,
            PortalledObjectBase portObjA, PortalledObjectBase portObjB)
            : base(name, portObjA, portObjB)
        {
            AnchorFace = anchorFace;
            SlidingAxis = slidingAxis;
            Distance = distance;
            _XOff = xOff;
            _YOff = yOff;
            _ZOff = zOff;
        }
        #endregion

        #region state
        private Axis _SlideAxis;
        private double _Distance;
        private AnchorFace _Face;
        private double _XOff;
        private double _YOff;
        private double _ZOff;
        #endregion

        protected override void InitInteractionHandlers()
        {
            AddIInteractHandler(new SlidingPortalHandler());
            AddIInteractHandler(new SlidingPortalObserveHandler());
            base.InitInteractionHandlers();
        }

        #region public override IGeometricSize GeometricSize { get; }
        public override IGeometricSize GeometricSize
        {
            get
            {
                var _h = Math.Ceiling(PortalledObjectA.Height / 5d);
                var _w = Math.Ceiling(PortalledObjectA.Width / 5d);
                return (AnchorFace.GetAxis()) switch
                {
                    Axis.Z => SlidingAxis switch
                    {
                        Axis.Y => new GeometricSize(1, _w, _h),
                        _ => new GeometricSize(1, _h, _w),
                    },
                    Axis.Y => SlidingAxis switch
                    {
                        Axis.Z => new GeometricSize(_w, 1, _h),
                        _ => new GeometricSize(_h, 1, _w),
                    },
                    _ => SlidingAxis switch
                    {
                        Axis.Z => new GeometricSize(_w, _h, 1),
                        _ => new GeometricSize(_h, _w, 1),
                    },
                };
            }
        }
        #endregion

        public override Sizer Sizer
            => new ObjectSizer(Size.Medium, this);

        #region public Axis SlidingAxis { get; set; }
        public Axis SlidingAxis
        {
            get => _SlideAxis;
            set
            {
                _SlideAxis = value;
                DoPropertyChanged(nameof(SlidingAxis));
                Represent();
            }
        }
        #endregion

        public AnchorFace SlidesTowards
            => SlidingAxis switch
            {
                Axis.X => Distance > 0 ? AnchorFace.XHigh : AnchorFace.XLow,
                Axis.Y => Distance > 0 ? AnchorFace.YHigh : AnchorFace.YLow,
                Axis.Z => Distance > 0 ? AnchorFace.ZHigh : AnchorFace.ZLow,
                _ => Distance > 0 ? AnchorFace.ZHigh : AnchorFace.ZLow,
            };

        #region public double OffsetX { get; set; }
        /// <summary>Used to position the model</summary>
        /// <remarks>AnchorFace is not used to calulate offsets, since the portal may be set-back</remarks>
        public double OffsetX
        {
            get => _XOff;
            set
            {
                _XOff = value;
                DoPropertyChanged(nameof(OffsetX));
                Represent();
            }
        }
        #endregion

        #region public double OffsetY { get; set; }
        /// <summary>Used to position the model</summary>
        /// <remarks>AnchorFace is not used to calulate offsets, since the portal may be set-back</remarks>
        public double OffsetY
        {
            get => _YOff;
            set
            {
                _YOff = value;
                DoPropertyChanged(nameof(OffsetY));
                Represent();
            }
        }
        #endregion

        #region public double OffsetZ { get; set; }
        /// <summary>Used to position the model</summary>
        /// <remarks>AnchorFace is not used to calulate offsets, since the portal may be set-back</remarks>
        public double OffsetZ
        {
            get => _ZOff;
            set
            {
                _ZOff = value;
                DoPropertyChanged(nameof(OffsetZ));
                Represent();
            }
        }
        #endregion

        #region public double Distance { get; set; }
        /// <summary>total distance moved by portal object when openeing</summary>
        public double Distance
        {
            get => _Distance;
            set
            {
                _Distance = value;
                DoPropertyChanged(nameof(Distance));
                Represent();
            }
        }
        #endregion

        #region public AnchorFace AnchorFace { get; set; }
        /// <summary>Used to deteremine tactical inquiries, and general axial use of footprint and slide offset</summary>
        public AnchorFace AnchorFace
        {
            get => _Face;
            set
            {
                _Face = value;
                DoPropertyChanged(nameof(AnchorFace));
                Represent();
            }
        }
        #endregion

        #region public bool AnchorCell(ICellLocation cLoc)
        /// <summary>Determines if the cell location is one that contains the portalled object</summary>
        public bool AnchorCell(ICellLocation cLoc)
        {
            var _region = MyPresenter.GeometricRegion;
            return AnchorFace switch
            {
                AnchorFace.XHigh => _region.UpperX == cLoc.X && _region.ContainsCell(cLoc),
                AnchorFace.XLow => _region.LowerX == cLoc.X && _region.ContainsCell(cLoc),
                AnchorFace.YHigh => _region.UpperY == cLoc.Y && _region.ContainsCell(cLoc),
                AnchorFace.YLow => _region.LowerY == cLoc.Y && _region.ContainsCell(cLoc),
                AnchorFace.ZHigh => _region.UpperZ == cLoc.Z && _region.ContainsCell(cLoc),
                AnchorFace.ZLow => _region.LowerZ == cLoc.Z && _region.ContainsCell(cLoc),
                _ => false,
            };
        }
        #endregion

        #region protected override bool IsSideAccessible(bool inside, IGeometricRegion actor, IGeometricRegion portal)
        protected override bool IsSideAccessible(bool inside, IGeometricRegion actor, IGeometricRegion portal)
            => actor==null ||
            AnchorFace switch
            {
                AnchorFace.XHigh => 
                    inside ? actor.LowerX <= portal.UpperX : actor.UpperX > portal.UpperX,
                AnchorFace.XLow => 
                    inside ? actor.UpperX >= portal.LowerX : actor.LowerX < portal.LowerX,
                AnchorFace.YHigh => 
                    inside ? actor.LowerY <= portal.UpperY : actor.UpperY > portal.UpperY,
                AnchorFace.YLow => 
                    inside ? actor.UpperY >= portal.LowerY : actor.LowerY < portal.LowerY,
                AnchorFace.ZHigh => 
                    inside ? actor.LowerZ <= portal.UpperZ : actor.UpperZ > portal.UpperZ,
                _ => 
                    inside ? actor.UpperZ >= portal.LowerZ : actor.LowerZ < portal.LowerZ,
            };
        #endregion

        #region ITacticalInquiry Members

        #region public CoverLevel SuppliesCover(TacticalInfo tacticalInfo)
        public CoverLevel SuppliesCover(in TacticalInfo tacticalInfo)
        {
            if (AnchorCell(tacticalInfo.TacticalCellRef))
            {
                if (OpenState.Value == 0)
                {
                    if (tacticalInfo.CrossesFace(AnchorFace))
                        return (PortalledObjectA.DoesSupplyCover > PortalledObjectB.DoesSupplyCover ? PortalledObjectA.DoesSupplyCover : PortalledObjectB.DoesSupplyCover);
                }
                else if (OpenState.Value < 0.75)
                {
                    if (tacticalInfo.CrossesFace(AnchorFace))
                    {
                        if ((PortalledObjectA.DoesSupplyCover == CoverLevel.Improved) || (PortalledObjectB.DoesSupplyCover == CoverLevel.Improved))
                        {
                            return CoverLevel.Hard;
                        }
                        return (PortalledObjectA.DoesSupplyCover > PortalledObjectB.DoesSupplyCover ? PortalledObjectA.DoesSupplyCover : PortalledObjectB.DoesSupplyCover);
                    }
                }
            }
            return CoverLevel.None;
        }
        #endregion

        #region public bool SuppliesConcealment(TacticalInfo tacticalInfo)
        public bool SuppliesConcealment(in TacticalInfo tacticalInfo)
        {
            if (AnchorCell(tacticalInfo.TacticalCellRef))
                if (OpenState.Value < 0.1)
                {
                    if (tacticalInfo.CrossesFace(AnchorFace))
                        return PortalledObjectA.DoesSupplyConcealment || PortalledObjectB.DoesSupplyConcealment;
                }
            return false;
        }
        #endregion

        #region public bool SuppliesTotalConcealment(TacticalInfo tacticalInfo)
        public bool SuppliesTotalConcealment(in TacticalInfo tacticalInfo)
        {
            if (AnchorCell(tacticalInfo.TacticalCellRef))
                if (OpenState.Value < 0.1)
                {
                    if (tacticalInfo.CrossesFace(AnchorFace))
                        return PortalledObjectA.DoesSupplyTotalConcealment || PortalledObjectB.DoesSupplyTotalConcealment;
                }
            return false;
        }
        #endregion

        #region public bool BlocksLineOfEffect(TacticalInfo tacticalInfo)
        public bool BlocksLineOfEffect(in TacticalInfo tacticalInfo)
        {
            if ((OpenState.Value == 0) && AnchorCell(tacticalInfo.TacticalCellRef))
            {
                if (tacticalInfo.CrossesFace(AnchorFace))
                    return PortalledObjectA.DoesBlocksLineOfEffect || PortalledObjectB.DoesBlocksLineOfEffect;
            }
            return false;
        }
        #endregion

        #region public bool BlocksLineOfDetect(TacticalInfo tacticalInfo)
        public bool BlocksLineOfDetect(in TacticalInfo tacticalInfo)
        {
            if ((OpenState.Value == 0) && AnchorCell(tacticalInfo.TacticalCellRef))
            {
                if (tacticalInfo.CrossesFace(AnchorFace))
                    return PortalledObjectA.DoesBlocksLineOfDetect || PortalledObjectB.DoesBlocksLineOfDetect;
            }
            return false;
        }
        #endregion

        #region private bool BlocksHedralTransit(MovementTacticalInfo moveTactical)
        private bool BlocksHedralTransit(MovementTacticalInfo moveTactical)
        {
            var _threshold = 0.5d;
            if (moveTactical.Movement.CoreObject is Creature _critter)
            {
                if (_critter.Sizer.Size.Order < 0)
                    _threshold = _critter.Sizer.Size.CubeSize().XExtent / 2;
            }
            if ((moveTactical.TransitFaces.Contains(AnchorFace))
                && OpenState.Value.CloseEnough(0, _threshold)
                && this.InLocator(moveTactical))
                return ((IMoveAlterer)PortalledObjectA).BlocksTransit(moveTactical)
                    || ((IMoveAlterer)PortalledObjectB).BlocksTransit(moveTactical);
            return false;
        }
        #endregion

        #region public bool HindersHedralTransit(MovementTacticalInfo moveTactical)
        public bool HindersHedralTransit(MovementTacticalInfo moveTactical)
        {
            var _threshold = 0.25d;
            if (moveTactical.Movement.CoreObject is Creature _critter)
            {
                if (_critter.Sizer.Size.Order < 0)
                    _threshold = _critter.Sizer.Size.CubeSize().XExtent / 4;
            }
            if ((moveTactical.TransitFaces.Contains(AnchorFace))
                && !OpenState.Value.CloseEnough(1, _threshold)
                && this.InLocator(moveTactical))
                return ((IMoveAlterer)PortalledObjectA).HindersTransit(moveTactical.Movement, new CellLocation(moveTactical.SourceCell))
                    || ((IMoveAlterer)PortalledObjectB).HindersTransit(moveTactical.Movement, new CellLocation(moveTactical.SourceCell));
            return false;
        }
        #endregion

        #region public bool BlocksSpread(AnchorFace transitFace, ICellLocation sourceCell, ICellLocation targetCell)
        public bool BlocksSpread(AnchorFace transitFace, ICellLocation sourceCell, ICellLocation targetCell)
        {
            // TODO: source or target cell must be in locator
            return ((transitFace == AnchorFace) && OpenState.Value.CloseEnough(0, 0.025d)
                && (PortalledObjectA.DoesBlocksSpread || PortalledObjectB.DoesBlocksSpread));
        }
        #endregion

        public bool CanBlockSpread
            => PortalledObjectA.DoesBlocksSpread || PortalledObjectB.DoesBlocksSpread;

        #endregion

        #region IMoveAlterer Members

        /// <summary>True if movement from SourceCell to TargetCell is legal for the movement</summary>
        public bool BlocksTransit(MovementTacticalInfo moveTactical)
            => BlocksHedralTransit(moveTactical);

        /// <summary>True if the affecter allows the region when using the given movement</summary>
        public bool CanOccupy(MovementBase movement, IGeometricRegion region, ICoreObject testObj)
        {
            // if the material is immaterial to the movement, can-occupy
            if (movement.CanMoveThrough(PortalledObjectA.ObjectMaterial)
                    && movement.CanMoveThrough(PortalledObjectB.ObjectMaterial))
                return true;

            // don't really touch this cell?
            if (!this.InLocator(region))
                return true;

            var _rgn = this.GetLocated()?.Locator.GeometricRegion;
            return (OpenState.Value >= 0.875)
                || _rgn.AllCellLocations().All(_c => region.IsCellUnboundAtFace(_c, AnchorFace));
        }

        /// <summary>Used for support against gravity and cell elevations</summary>
        public double HedralTransitBlocking(MovementTacticalInfo moveTactical)
            => BlocksHedralTransit(moveTactical) ? 1 : 0;

        /// <summary>True if movement is hindered moving from SourceCell to TargetCell</summary>
        public bool HindersTransit(MovementBase movement, IGeometricRegion region)
            => false;

        public bool IsCostlyMovement(MovementBase movement, IGeometricRegion region)
            => false;

        #region public IEnumerable<MovementOpening> OpensTowards(MovementBase movement, ICellLocation occupyCell, AnchorFace baseFace, ICoreObject testObj)
        /// <summary>Only uses Movement and SourceCell</summary>
        public IEnumerable<MovementOpening> OpensTowards(MovementBase movement, ICellLocation occupyCell, AnchorFace baseFace, ICoreObject testObj)
        {
            if (HindersHedralTransit(new MovementTacticalInfo
            {
                Movement = movement,
                SourceCell = occupyCell,
                TransitFaces = new AnchorFace[] { AnchorFace }
            }))
            {
                yield return new MovementOpening(AnchorFace.ReverseFace(),
                    5 - (PortalledObjectA.Thickness + PortalledObjectB.Thickness),
                    1 - OpenState.Value);
            }
            yield break;
        }
        #endregion

        #endregion

        #region IControlPresentation Members

        public IEnumerable<KeyValuePair<Type, VisualEffect>> VisualEffects(IGeometricRegion observer,
            IList<SensoryBase> filteredSenses, VisualEffect standard)
        {
            var _aType = Flip ? typeof(BackSenseEffectExtension) : typeof(FrontSenseEffectExtension);
            var _bType = !Flip ? typeof(BackSenseEffectExtension) : typeof(FrontSenseEffectExtension);

            #region visual effect for A side
            if (!IsSideAccessible(false, observer))
            {
                yield return new KeyValuePair<Type, VisualEffect>(_aType, VisualEffect.Unseen);
            }
            else
            {
                if (!filteredSenses.Any(_s => _s.UsesSight))
                {
                    // form-only
                    yield return new KeyValuePair<Type, VisualEffect>(_aType, VisualEffect.FormOnly);
                }
                else
                {
                    // TODO: get best(?!?) of all at edge
                    var _locator = this.GetLocated().Locator;
                    CellLocation _location = OffsetCell(_locator, AnchorFace);
                    yield return new KeyValuePair<Type, VisualEffect>(_aType, OffsetVisualEffect(observer, filteredSenses, _locator, _location));
                }
            }
            #endregion

            if (!IsSideAccessible(false, observer))
            {
                yield return new KeyValuePair<Type, VisualEffect>(_bType, VisualEffect.Unseen);
            }
            else
            {
                yield return new KeyValuePair<Type, VisualEffect>(_bType, standard);
            }
            yield return new KeyValuePair<Type, VisualEffect>(typeof(LeftSenseEffectExtension), standard);
            yield return new KeyValuePair<Type, VisualEffect>(typeof(RightSenseEffectExtension), standard);
            yield return new KeyValuePair<Type, VisualEffect>(typeof(TopSenseEffectExtension), standard);
            yield return new KeyValuePair<Type, VisualEffect>(typeof(BottomSenseEffectExtension), standard);
            yield break;
        }

        #region public List<Transform3DInfo> GetCustomTransformations(IGeometricRegion geom)
        public List<Transform3DInfo> GetCustomTransformations(IGeometricRegion geom)
        {
            Translate3DInfo _footStart = null;
            var _custom = new List<Transform3DInfo>();
            var _widthBump = PortalledObjectA.Width / 2;
            var _thickBump = PortalledObjectA.Thickness / 2;
            var _heightBump = PortalledObjectA.Height / 2;
            switch (AnchorFace)
            {
                case AnchorFace.XLow:
                    _footStart = new Translate3DInfo
                    {
                        OffsetX = _thickBump + OffsetX,
                        OffsetY = _widthBump + OffsetY,
                        OffsetZ = _heightBump + OffsetZ
                    };
                    break;

                case AnchorFace.XHigh:
                    var _xl = (geom.UpperX - geom.LowerX + 1);
                    _footStart = new Translate3DInfo
                    {
                        OffsetX = _xl * 5d - _thickBump + OffsetX,
                        OffsetY = _widthBump + OffsetY,
                        OffsetZ = _heightBump + OffsetZ
                    };
                    break;

                case AnchorFace.YLow:
                    _footStart = new Translate3DInfo
                    {
                        OffsetX = _widthBump + OffsetX,
                        OffsetY = _thickBump + OffsetY,
                        OffsetZ = _heightBump + OffsetZ
                    };
                    break;

                case AnchorFace.YHigh:
                    var _yl = (geom.UpperY - geom.LowerY + 1);
                    _footStart = new Translate3DInfo
                    {
                        OffsetX = _widthBump + OffsetX,
                        OffsetY = _yl * 5d - _thickBump + OffsetY,
                        OffsetZ = _heightBump + OffsetZ
                    };
                    break;

                case AnchorFace.ZLow:
                    if (Flip)
                    {
                        _custom.Add(new AxisAngleRotate3DInfo
                        {
                            AxisX = 0,
                            AxisY = 1,
                            AxisZ = 0,
                            Angle = 180
                        });
                    }
                    _footStart = new Translate3DInfo
                    {
                        OffsetX = _widthBump + OffsetX,
                        OffsetY = _heightBump + OffsetY,
                        OffsetZ = _thickBump + OffsetZ
                    };
                    break;

                case AnchorFace.ZHigh:
                    if (Flip)
                    {
                        _custom.Add(new AxisAngleRotate3DInfo
                        {
                            AxisX = 0,
                            AxisY = 1,
                            AxisZ = 0,
                            Angle = 180
                        });
                    }
                    var _zl = (geom.UpperZ - geom.LowerZ + 1);
                    _footStart = new Translate3DInfo
                    {
                        OffsetX = _widthBump + OffsetX,
                        OffsetY = _heightBump + OffsetY,
                        OffsetZ = _zl * 5d - _thickBump + OffsetZ
                    };
                    break;
            }

            // add and adjust by open state
            _custom.Add(_footStart);
            return _custom;
        }
        #endregion

        #region public double OverridePivot(double pivot)
        public double OverridePivot(double pivot)
        {
            switch (AnchorFace)
            {
                case AnchorFace.XLow:
                case AnchorFace.XHigh:
                    return (Flip ? -90 : 90);

                case AnchorFace.YLow:
                case AnchorFace.YHigh:
                    return (Flip ? 180 : 0);

                default:
                    return pivot;
            }
        }
        #endregion

        #region public double OverrideTilt(double tilt)
        public double OverrideTilt(double tilt)
        {
            switch (AnchorFace)
            {
                case AnchorFace.ZLow:
                case AnchorFace.ZHigh:
                    return -90;

                default:
                    return 0;
            }
        }
        #endregion

        #endregion

        #region IAlterLocalLink Members

        #region public bool CanAlterLink(LocalLink link)
        public bool CanAlterLink(LocalLink link)
        {
            IGeometricRegion _selfLocator = this.GetLocated().Locator.GeometricRegion;
            if (link.GroupA.ContainsGeometricRegion(_selfLocator))
            {
                if (AnchorFace == link.AnchorFaceInA)
                {
                    // portal in link's A-Group, and faces match
                    return OpenState.Value < 1;
                }
            }
            else if (AnchorFace.IsOppositeTo(link.AnchorFaceInA))
            {
                // otherwise, portal in B-Group, and faces opposed
                return OpenState.Value < 1;
            }
            return false;
        }
        #endregion

        public double AllowLightFactor(LocalLink link)
        {
            // OS=1 :: block=0
            // OS=0 :: block=opacity
            var _opacity = Math.Max(PortalledObjectA.Opacity, PortalledObjectB.Opacity);
            var _block = (1 - OpenState.Value) * _opacity;

            // when light is in a deep shadows room, reduce it through the link (indicating less reflecting glow)
            if (this.GetLocated().Locator.GetLocalCellGroups().FirstOrDefault().DeepShadows)
                return (1 - _block) * 0.8d;
            else
                return (1 - _block);
        }

        public int GetExtraSoundDifficulty(LocalLink link)
            => Convert.ToInt32((1 - OpenState.Value) * Math.Max(PortalledObjectA.ExtraSoundDifficulty.EffectiveValue, PortalledObjectB.ExtraSoundDifficulty.EffectiveValue));

        #endregion
    }
}
