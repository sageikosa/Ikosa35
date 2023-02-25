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
using System.Diagnostics;
using Uzi.Visualize.Contracts.Tactical;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class CornerPivotPortal : PortalBase, ITacticalInquiry, IAlterLocalLink, IMoveAlterer
    {
        #region Construction
        public CornerPivotPortal(string name, AnchorFace anchorClose, AnchorFace anchorOpen,
            PortalledObjectBase portObjA, PortalledObjectBase portObjB)
            : base(name, portObjA, portObjB)
        {
            AnchorClose = anchorClose;
            AnchorOpen = anchorOpen;
            _InvertOpener = false;
        }
        #endregion

        #region data
        private AnchorFace _Close;
        private AnchorFace _Open;

        /// <summary>When set to true, calculate opener angle by subtracting from 1, then multiplying by 90 degrees</summary>
        [NonSerialized, JsonIgnore]
        private bool _InvertOpener;

        private bool _InnerHinge = false;
        #endregion

        protected override void InitInteractionHandlers()
        {
            AddIInteractHandler(new CornerPivotPortalHandler());
            AddIInteractHandler(new CornerPivotPortalObserveHandler());
            base.InitInteractionHandlers();
        }

        #region public virtual AnchorFace AnchorClose { get; set; }
        public virtual AnchorFace AnchorClose
        {
            get => _Close;
            set
            {
                _Close = value;
                DoPropertyChanged(nameof(AnchorClose));
                Represent();
            }
        }
        #endregion

        #region public virtual AnchorFace AnchorOpen { get; set; }
        public virtual AnchorFace AnchorOpen
        {
            get => _Open;
            set
            {
                _Open = value;
                DoPropertyChanged(nameof(AnchorOpen));
                Represent();
            }
        }
        #endregion

        #region public bool InnerHinge { get; set; }
        public bool InnerHinge
        {
            get => _InnerHinge;
            set
            {
                _InnerHinge = value;
                DoPropertyChanged(nameof(InnerHinge));
                Represent();
            }
        }
        #endregion

        #region public bool AnchorMatch(AnchorFace anchor, ICellLocation cLoc)
        /// <summary>
        /// Determines if the cell location is one that contains specified anchor face
        /// </summary>
        /// <param name="cLoc"></param>
        /// <returns></returns>
        private bool AnchorMatch(AnchorFace anchor, ICellLocation cLoc)
        {
            var _region = MyPresenter.GeometricRegion;
            return anchor switch
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
        {
            if (actor == null)
                return true;
            bool _faceTest(AnchorFace face)
                => face switch
                {
                    AnchorFace.XHigh => inside
                        ? actor.LowerX <= portal.UpperX
                        : actor.UpperX > portal.UpperX,
                    AnchorFace.XLow => inside
                        ? actor.UpperX >= portal.LowerX
                        : actor.LowerX < portal.LowerX,
                    AnchorFace.YHigh => inside
                        ? actor.LowerY <= portal.UpperY
                        : actor.UpperY > portal.UpperY,
                    AnchorFace.YLow => inside
                        ? actor.UpperY >= portal.LowerY
                        : actor.LowerY < portal.LowerY,
                    AnchorFace.ZHigh => inside
                        ? actor.LowerZ <= portal.UpperZ
                        : actor.UpperZ > portal.UpperZ,
                    _ => inside
                        ? actor.UpperZ >= portal.LowerZ
                        : actor.LowerZ < portal.LowerZ,
                };

            if (OpenState.Value <= 0.25)
            {
                return _faceTest(AnchorClose);
            }
            else if (OpenState.Value >= 0.75)
            {
                inside = !inside;
                return _faceTest(AnchorOpen);
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region ITacticalInquiry Members

        // TODO: AnchorMatch and crossesFace for intermediate values
        private bool InPortal(TacticalInfo tacticalInfo, IGeometricRegion region)
            => region?.ContainsCell(tacticalInfo.TacticalCellRef) ?? false;

        #region public CoverLevel SuppliesCover(TacticalInfo tacticalInfo)
        public CoverLevel SuppliesCover(in TacticalInfo tacticalInfo)
        {
            if (OpenState.Value > 0.75d)
            {
                // TODO: bigger portals need to ensure cells at appropriate edge
                // TODO: planar points?
                if (AnchorMatch(AnchorOpen, tacticalInfo.TacticalCellRef) && tacticalInfo.CrossesFace(AnchorOpen))
                    return (PortalledObjectA.DoesSupplyCover > PortalledObjectB.DoesSupplyCover ? PortalledObjectA.DoesSupplyCover : PortalledObjectB.DoesSupplyCover);
            }
            else if (OpenState.Value < 0.25d)
            {
                // TODO: bigger portals need to track cells at appropriate edge
                // TODO: planar points?
                if (AnchorMatch(AnchorClose, tacticalInfo.TacticalCellRef) && tacticalInfo.CrossesFace(AnchorClose))
                    return (PortalledObjectA.DoesSupplyCover > PortalledObjectB.DoesSupplyCover ? PortalledObjectA.DoesSupplyCover : PortalledObjectB.DoesSupplyCover);
            }
            else
            {
                // TODO: use expanded info to see if line goes through open door
                if (InPortal(tacticalInfo, this.GetLocated()?.Locator.GeometricRegion))
                    return (PortalledObjectA.DoesSupplyCover > PortalledObjectB.DoesSupplyCover
                        ? PortalledObjectA.DoesSupplyCover
                        : PortalledObjectB.DoesSupplyCover);
            }
            return CoverLevel.None;
        }
        #endregion

        #region public bool SuppliesConcealment(TacticalInfo tacticalInfo)
        public bool SuppliesConcealment(in TacticalInfo tacticalInfo)
        {
            var _geom = new Lazy<IGeometricRegion>(() => this.GetLocated()?.Locator.GeometricRegion);
            if (OpenState.Value <= 0.1d)
                return (PortalledObjectA.DoesSupplyConcealment || PortalledObjectB.DoesSupplyConcealment)
                    && !_geom.Value.HasTargetAccess(AnchorClose, tacticalInfo.SourceRegion, tacticalInfo.TargetRegion)
                    && tacticalInfo.CrossesFace(AnchorClose)
                    && AnchorMatch(AnchorClose, tacticalInfo.TacticalCellRef);
            else if (OpenState.Value >= 0.9d)
                return (PortalledObjectA.DoesSupplyConcealment || PortalledObjectB.DoesSupplyConcealment)
                    && !_geom.Value.HasTargetAccess(AnchorOpen, tacticalInfo.SourceRegion, tacticalInfo.TargetRegion)
                    && tacticalInfo.CrossesFace(AnchorOpen)
                    && AnchorMatch(AnchorOpen, tacticalInfo.TacticalCellRef);
            else
            {
                // somewhere in-between
                return (PortalledObjectA.DoesSupplyConcealment || PortalledObjectB.DoesSupplyConcealment)
                    && tacticalInfo.CrossesFace(AnchorOpen)
                    && tacticalInfo.CrossesFace(AnchorClose)
                    && InPortal(tacticalInfo, _geom.Value);
            }
        }
        #endregion

        #region public bool SuppliesTotalConcealment(TacticalInfo tacticalInfo)
        public bool SuppliesTotalConcealment(in TacticalInfo tacticalInfo)
        {
            var _geom = new Lazy<IGeometricRegion>(() => this.GetLocated()?.Locator.GeometricRegion);
            if (OpenState.Value <= 0.1d)
                return (PortalledObjectA.DoesSupplyTotalConcealment || PortalledObjectB.DoesSupplyTotalConcealment)
                    && !_geom.Value.HasTargetAccess(AnchorClose, tacticalInfo.SourceRegion, tacticalInfo.TargetRegion)
                    && tacticalInfo.CrossesFace(AnchorClose)
                    && AnchorMatch(AnchorClose, tacticalInfo.TacticalCellRef);
            else if (OpenState.Value >= 0.9d)
                return (PortalledObjectA.DoesSupplyTotalConcealment || PortalledObjectB.DoesSupplyTotalConcealment)
                    && !_geom.Value.HasTargetAccess(AnchorOpen, tacticalInfo.SourceRegion, tacticalInfo.TargetRegion)
                    && tacticalInfo.CrossesFace(AnchorOpen)
                    && AnchorMatch(AnchorOpen, tacticalInfo.TacticalCellRef);
            else
            {
                // somewhere in-between
                return (PortalledObjectA.DoesSupplyTotalConcealment || PortalledObjectB.DoesSupplyTotalConcealment)
                    && tacticalInfo.CrossesFace(AnchorOpen)
                    && tacticalInfo.CrossesFace(AnchorClose)
                    && InPortal(tacticalInfo, _geom.Value);
            }
        }
        #endregion

        #region public bool BlocksLineOfEffect(TacticalInfo tacticalInfo)
        public bool BlocksLineOfEffect(in TacticalInfo tacticalInfo)
        {
            var _geom = new Lazy<IGeometricRegion>(() => this.GetLocated()?.Locator.GeometricRegion);
            if (OpenState.Value.CloseEnough(0, 0.025d))
                return (PortalledObjectA.DoesBlocksLineOfEffect || PortalledObjectB.DoesBlocksLineOfEffect)
                    && !_geom.Value.HasTargetAccess(AnchorClose, tacticalInfo.SourceRegion, tacticalInfo.TargetRegion)
                    && tacticalInfo.CrossesFace(AnchorClose)
                    && InPortal(tacticalInfo, _geom.Value);
            else if (OpenState.Value.CloseEnough(1, 0.025d))
                return (PortalledObjectA.DoesBlocksLineOfEffect || PortalledObjectB.DoesBlocksLineOfEffect)
                    && !_geom.Value.HasTargetAccess(AnchorOpen, tacticalInfo.SourceRegion, tacticalInfo.TargetRegion)
                    && tacticalInfo.CrossesFace(AnchorOpen)
                    && InPortal(tacticalInfo, _geom.Value);
            else
            {
                // somewhere in-between
                return (PortalledObjectA.DoesBlocksLineOfEffect || PortalledObjectB.DoesBlocksLineOfEffect)
                    && tacticalInfo.CrossesFace(AnchorOpen)
                    && tacticalInfo.CrossesFace(AnchorClose)
                    && InPortal(tacticalInfo, _geom.Value);
            }
        }
        #endregion

        #region public bool BlocksLineOfDetect(TacticalInfo tacticalInfo)
        public bool BlocksLineOfDetect(in TacticalInfo tacticalInfo)
        {
            var _geom = new Lazy<IGeometricRegion>(() => this.GetLocated()?.Locator.GeometricRegion);
            if (OpenState.Value.CloseEnough(0, 0.025d))
                return (PortalledObjectA.DoesBlocksLineOfDetect || PortalledObjectB.DoesBlocksLineOfDetect)
                    && !_geom.Value.HasTargetAccess(AnchorOpen, tacticalInfo.SourceRegion, tacticalInfo.TargetRegion)
                    && tacticalInfo.CrossesFace(AnchorClose)
                    && InPortal(tacticalInfo, _geom.Value);
            else if (OpenState.Value.CloseEnough(1, 0.025d))
                return (PortalledObjectA.DoesBlocksLineOfDetect || PortalledObjectB.DoesBlocksLineOfDetect)
                    && !_geom.Value.HasTargetAccess(AnchorOpen, tacticalInfo.SourceRegion, tacticalInfo.TargetRegion)
                    && tacticalInfo.CrossesFace(AnchorOpen)
                    && InPortal(tacticalInfo, _geom.Value);
            else
            {
                // somewhere in-between
                return (PortalledObjectA.DoesBlocksLineOfDetect || PortalledObjectB.DoesBlocksLineOfDetect)
                    && tacticalInfo.CrossesFace(AnchorOpen)
                    && tacticalInfo.CrossesFace(AnchorClose)
                    && InPortal(tacticalInfo, _geom.Value);
            }
        }
        #endregion

        #region private bool BlocksHedralTransit(MovementTacticalInfo moveTactical)
        private bool BlocksHedralTransit(MovementTacticalInfo moveTactical)
        {
            // up to 50% open blocks when coming through the closed face (and vice versa)
            if ((OpenState.Value <= 0.375d)
                && (moveTactical.TransitFaces.Contains(AnchorClose))
                && this.InLocator(moveTactical))
                return ((IMoveAlterer)PortalledObjectA).BlocksTransit(moveTactical)
                    || ((IMoveAlterer)PortalledObjectB).BlocksTransit(moveTactical);
            else if ((OpenState.Value >= 0.625d)
                && (moveTactical.TransitFaces.Contains(AnchorOpen))
                && this.InLocator(moveTactical))
                return ((IMoveAlterer)PortalledObjectA).BlocksTransit(moveTactical)
                    || ((IMoveAlterer)PortalledObjectB).BlocksTransit(moveTactical);
            return false;
        }
        #endregion

        #region public bool HindersHedralTransit(MovementBase movement, AnchorFace transitFace)
        public bool HindersHedralTransit(MovementTacticalInfo moveTactical)
        {
            // up to 87.5% open hinders when coming through the closed face (and vice versa)
            if ((OpenState.Value <= 0.875d)
                && (moveTactical.TransitFaces.Contains(AnchorClose))
                && this.InLocator(moveTactical))
                return ((IMoveAlterer)PortalledObjectA).HindersTransit(moveTactical.Movement, new CellLocation(moveTactical.SourceCell))
                    || ((IMoveAlterer)PortalledObjectB).HindersTransit(moveTactical.Movement, new CellLocation(moveTactical.SourceCell));
            else if ((OpenState.Value >= 0.125d)
                && (moveTactical.TransitFaces.Contains(AnchorOpen))
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
            if (OpenState.Value.CloseEnough(0, 0.025d))
                return (PortalledObjectA.DoesBlocksSpread || PortalledObjectB.DoesBlocksSpread) && (AnchorClose == transitFace);
            else if (OpenState.Value.CloseEnough(1, 0.025d))
                return (PortalledObjectA.DoesBlocksSpread || PortalledObjectB.DoesBlocksSpread) && (AnchorOpen == transitFace);
            else
                return false;
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

            if (OpenState.Value <= 0.25)
            {
                var _rgn = this.GetLocated()?.Locator.GeometricRegion;
                return _rgn.AllCellLocations().All(_c => region.IsCellUnboundAtFace(_c, AnchorClose));
            }
            else if (OpenState.Value >= 0.75)
            {
                var _rgn = this.GetLocated()?.Locator.GeometricRegion;
                return _rgn.AllCellLocations().All(_c => region.IsCellUnboundAtFace(_c, AnchorOpen));
            }

            // blocking the middle area
            return false;
        }

        /// <summary>Used for support against gravity and cell elevations</summary>
        public double HedralTransitBlocking(MovementTacticalInfo moveTactical)
            => BlocksHedralTransit(moveTactical) ? 1 : 0;

        /// <summary>True if movement is hindered moving from SourceCell to TargetCell</summary>
        public bool HindersTransit(MovementBase movement, IGeometricRegion region)
        {
            // if the material is immaterial to the movement, no hinderance
            if (movement.CanMoveThrough(PortalledObjectA.ObjectMaterial)
                    && movement.CanMoveThrough(PortalledObjectB.ObjectMaterial))
                return false;

            // open enough to hinder
            return (OpenState.Value >= 0.125d) && (OpenState.Value <= 0.875d)
                && this.InLocator(region);
        }

        public bool IsCostlyMovement(MovementBase movement, IGeometricRegion region)
            => (OpenState.Value > 0.10) && (OpenState.Value < 0.9)
                && this.InLocator(region)
                && (!movement.CanMoveThrough(PortalledObjectA.ObjectMaterial)
                    || !movement.CanMoveThrough(PortalledObjectB.ObjectMaterial));

        #region public IEnumerable<MovementOpening> OpensTowards(MovementBase movement, ICellLocation occupyCell, AnchorFace baseFace, ICoreObject testObj)
        /// <summary>Only uses Movement and SourceCell</summary>
        public IEnumerable<MovementOpening> OpensTowards(MovementBase movement, ICellLocation occupyCell, AnchorFace baseFace, ICoreObject testObj)
        {
            // only providing when completely blocking
            if (OpenState.Value == 0)
            {
                if (BlocksHedralTransit(new MovementTacticalInfo
                {
                    Movement = movement,
                    SourceCell = occupyCell,
                    TransitFaces = AnchorClose.ToEnumerable().ToArray()
                }))
                {
                    yield return new MovementOpening(AnchorClose.ReverseFace(),
                        5 - (PortalledObjectA.Thickness + PortalledObjectB.Thickness),
                        1 - OpenState.Value);
                }
            }
            else if (OpenState.Value == 1)
            {
                if (BlocksHedralTransit(new MovementTacticalInfo
                {
                    Movement = movement,
                    SourceCell = occupyCell,
                    TransitFaces = AnchorOpen.ToEnumerable().ToArray()
                }))
                {
                    yield return new MovementOpening(AnchorOpen.ReverseFace(),
                        5 - (PortalledObjectA.Thickness + PortalledObjectB.Thickness),
                        OpenState.Value);
                }
            }
            yield break;
        }
        #endregion

        #endregion

        #region protected void TransformOpenState(OpenStatus oldValue)
        private double OpenerAngle
            => _InvertOpener
            ? 90 - (90 * OpenState.Value)
            : (90 * OpenState.Value);
        #endregion

        private bool HasAnchor(AnchorFace anchor)
            => (AnchorOpen == anchor) || (AnchorClose == anchor);

        public override IGeometricSize GeometricSize
        {
            get
            {
                var _h = Math.Ceiling(PortalledObjectA.Height / 5d);
                var _w = Math.Ceiling(PortalledObjectA.Width / 5d);
                if (!HasAnchor(AnchorFace.ZLow) && !HasAnchor(AnchorFace.ZHigh))
                    return new GeometricSize(_h, _w, _w);
                else if (!HasAnchor(AnchorFace.YLow) && !HasAnchor(AnchorFace.YHigh))
                    return new GeometricSize(_w, _h, _w);
                return new GeometricSize(_w, _w, _h);
            }
        }

        public override Sizer Sizer
            => new ObjectSizer(Size.Medium, this);

        #region private bool NativeFlip { get; }
        /// <summary>Must flip the portal to align it's sides correctly</summary>
        private bool NativeFlip
        {
            get
            {
                if (
                    ((AnchorOpen == AnchorFace.YLow) && (AnchorClose == AnchorFace.XLow))
                    || ((AnchorOpen == AnchorFace.YLow) && (AnchorClose == AnchorFace.ZHigh))
                    || ((AnchorOpen == AnchorFace.YHigh) && (AnchorClose == AnchorFace.ZLow))
                    || ((AnchorOpen == AnchorFace.YHigh) && (AnchorClose == AnchorFace.XHigh))

                    || ((AnchorOpen == AnchorFace.XLow) && (AnchorClose == AnchorFace.YHigh))
                    || ((AnchorOpen == AnchorFace.XLow) && (AnchorClose == AnchorFace.ZLow))
                    || ((AnchorOpen == AnchorFace.XHigh) && (AnchorClose == AnchorFace.YLow))
                    || ((AnchorOpen == AnchorFace.XHigh) && (AnchorClose == AnchorFace.ZHigh))

                    || ((AnchorOpen == AnchorFace.ZLow) && (AnchorClose == AnchorFace.YLow))
                    || ((AnchorOpen == AnchorFace.ZLow) && (AnchorClose == AnchorFace.XHigh))
                    || ((AnchorOpen == AnchorFace.ZHigh) && (AnchorClose == AnchorFace.YHigh))
                    || ((AnchorOpen == AnchorFace.ZHigh) && (AnchorClose == AnchorFace.XLow))
                    )
                    return true;
                return false;
            }
        }
        #endregion

        #region public IEnumerable<KeyValuePair<Type, VisualEffect>> VisualEffects(IGeometricRegion location, IList<SensoryBase> filteredSenses, VisualEffect standard)
        public IEnumerable<KeyValuePair<Type, VisualEffect>> VisualEffects(IGeometricRegion observer, IList<SensoryBase> filteredSenses, VisualEffect standard)
        {
            var _effect = standard;

            // use offset cell for closed face
            var _locator = this.GetLocated().Locator;

            // side A
            var _closeType = Flip ? typeof(BackSenseEffectExtension) : typeof(FrontSenseEffectExtension);
            if (!IsSideAccessible(false, observer))
            {
                yield return new KeyValuePair<Type, VisualEffect>(_closeType, VisualEffect.Unseen);
            }
            else
            {
                var _closeEffect = OffsetVisualEffect(observer, filteredSenses, _locator, OffsetCell(_locator, AnchorClose));
                if (OpenState.Value >= 0.25)
                {
                    // more than 25% open, use the regular locator effect for A side
                    if ((_effect <= VisualEffect.Unseen)
                        && (((byte)_effect - (byte)_closeEffect) > 1)
                        && (OpenState.Value <= 0.75))
                    {
                        yield return new KeyValuePair<Type, VisualEffect>(_closeType, _effect - 1);
                    }
                    else
                    {
                        yield return new KeyValuePair<Type, VisualEffect>(_closeType, _effect);
                    }
                }
                else
                {
                    // use offset cell for closed face
                    yield return new KeyValuePair<Type, VisualEffect>(_closeType, _closeEffect);
                }
            }

            // side B
            var _openType = !Flip ? typeof(BackSenseEffectExtension) : typeof(FrontSenseEffectExtension);
            if (!IsSideAccessible(true, observer))
            {
                yield return new KeyValuePair<Type, VisualEffect>(_openType, VisualEffect.Unseen);
            }
            else
            {
                var _openEffect = OffsetVisualEffect(observer, filteredSenses, _locator, OffsetCell(_locator, AnchorOpen));
                if (OpenState.Value <= 0.75)
                {
                    // less than 75% open, use the regular locator effect for B side
                    if ((_effect <= VisualEffect.Unseen)
                        && (((byte)_effect - (byte)_openEffect) > 1)
                        && (OpenState.Value >= 0.25))
                    {
                        yield return new KeyValuePair<Type, VisualEffect>(_openType, _effect - 1);
                    }
                    else
                    {
                        yield return new KeyValuePair<Type, VisualEffect>(_openType, _effect);
                    }
                }
                else
                {
                    // use offset cell for open face
                    yield return new KeyValuePair<Type, VisualEffect>(_openType, _openEffect);
                }
            }
            yield return new KeyValuePair<Type, VisualEffect>(typeof(LeftSenseEffectExtension), _effect);
            yield return new KeyValuePair<Type, VisualEffect>(typeof(RightSenseEffectExtension), _effect);
            yield return new KeyValuePair<Type, VisualEffect>(typeof(TopSenseEffectExtension), _effect);
            yield return new KeyValuePair<Type, VisualEffect>(typeof(BottomSenseEffectExtension), _effect);
            yield break;
        }
        #endregion

        #region public List<Transform3DInfo> GetCustomTransformations(IGeometricRegion geom)
        public List<Transform3DInfo> GetCustomTransformations(IGeometricRegion geom)
        {
            var _custom = new List<Transform3DInfo>();
            var _widthBump = PortalledObjectA.Width / 2;
            var _thickBump = PortalledObjectA.Thickness / 2;
            var _heightBump = PortalledObjectA.Height / 2;

            var _xH = (geom.UpperX - geom.LowerX + 1) * 5d;
            var _yH = (geom.UpperY - geom.LowerY + 1) * 5d;
            var _zH = (geom.UpperZ - geom.LowerZ + 1) * 5d;

            if (HasAnchor(AnchorFace.XLow))
            {
                _InvertOpener = AnchorClose == AnchorFace.XLow;
                if (Flip ^ NativeFlip)
                {
                    // flip up and down
                    _custom.Add(
                        new AxisAngleRotate3DInfo
                        {
                            AxisX = 1,
                            AxisY = 0,
                            AxisZ = 0,
                            Angle = 180,
                            CenterX = 0,
                            CenterY = 0,
                            CenterZ = 0
                        });
                }
                if (HasAnchor(AnchorFace.YLow))
                {
                    _custom.Add(new Translate3DInfo
                    {
                        OffsetX = _widthBump,
                        OffsetY = _thickBump,
                        OffsetZ = _heightBump
                    });
                    _custom.Add(
                        new AxisAngleRotate3DInfo
                        {
                            AxisX = 0,
                            AxisY = 0,
                            AxisZ = 1,
                            Angle = OpenerAngle,
                            CenterX = _thickBump,
                            CenterY = _thickBump,
                            CenterZ = 0
                        });
                    if (InnerHinge && AnchorClose == AnchorFace.XLow)
                        _custom.Add(new Translate3DInfo
                        {
                            OffsetX = 0,
                            OffsetY = _yH - PortalledObjectA.Width,
                            OffsetZ = 0
                        });
                }
                else if (HasAnchor(AnchorFace.YHigh))
                {
                    _custom.Add(new Translate3DInfo
                    {
                        OffsetX = _widthBump,
                        OffsetY = _yH - _thickBump,
                        OffsetZ = _heightBump
                    });
                    _custom.Add(
                        new AxisAngleRotate3DInfo
                        {
                            AxisX = 0,
                            AxisY = 0,
                            AxisZ = -1,
                            Angle = OpenerAngle,
                            CenterX = _thickBump,
                            CenterY = _yH - _thickBump,
                            CenterZ = 0
                        });
                    if (InnerHinge && AnchorClose == AnchorFace.XLow)
                        _custom.Add(
                            new Translate3DInfo
                            {
                                OffsetX = 0,
                                OffsetY = PortalledObjectA.Width - _yH,
                                OffsetZ = 0
                            });
                }
                else if (HasAnchor(AnchorFace.ZLow))
                {
                    // (custom) tilt it down (since this has to be after the flip)
                    _custom.Add(new AxisAngleRotate3DInfo { AxisX = 1, AxisY = 0, AxisZ = 0, Angle = -90 });
                    _custom.Add(new Translate3DInfo
                    {
                        OffsetX = _widthBump,
                        OffsetY = _heightBump,
                        OffsetZ = _thickBump
                    });
                    _custom.Add(
                       new AxisAngleRotate3DInfo
                       {
                           AxisX = 0,
                           AxisY = -1,
                           AxisZ = 0,
                           Angle = OpenerAngle,
                           CenterX = _thickBump,
                           CenterY = 0,
                           CenterZ = _thickBump
                       });
                    if (InnerHinge && AnchorClose == AnchorFace.XLow)
                        _custom.Add(
                            new Translate3DInfo
                            {
                                OffsetX = 0,
                                OffsetY = 0,
                                OffsetZ = _zH - PortalledObjectA.Width
                            });
                }
                else
                {
                    // ZHigh
                    // (custom) tilt it down (since this has to be after the flip)
                    _custom.Add(
                        new AxisAngleRotate3DInfo
                        {
                            AxisX = 1,
                            AxisY = 0,
                            AxisZ = 0,
                            Angle = -90
                        });

                    // move into place
                    _custom.Add(new Translate3DInfo
                    {
                        OffsetX = _widthBump,
                        OffsetY = _heightBump,
                        OffsetZ = _zH - _thickBump
                    });

                    // ctrl pivot
                    _custom.Add(
                       new AxisAngleRotate3DInfo
                       {
                           AxisX = 0,
                           AxisY = 1,
                           AxisZ = 0,
                           Angle = OpenerAngle,
                           CenterX = _thickBump,
                           CenterY = 0,
                           CenterZ = _zH - _thickBump
                       });
                    if (InnerHinge && AnchorClose == AnchorFace.XLow)
                        _custom.Add(
                            new Translate3DInfo
                            {
                                OffsetX = 0,
                                OffsetY = 0,
                                OffsetZ = PortalledObjectA.Width - _zH
                            });
                }
                if (InnerHinge && AnchorOpen == AnchorFace.XLow)
                    _custom.Add(
                        new Translate3DInfo
                        {
                            OffsetX = _xH - PortalledObjectA.Width,
                            OffsetY = 0,
                            OffsetZ = 0
                        });
            }
            else if (HasAnchor(AnchorFace.XHigh))
            {
                // get the hinge side facing the corner
                _InvertOpener = AnchorClose == AnchorFace.XHigh;

                // Flip halfway up
                if (Flip ^ NativeFlip)
                    _custom.Add(
                        new AxisAngleRotate3DInfo
                        {
                            AxisX = 1,
                            AxisY = 0,
                            AxisZ = 0,
                            Angle = 180
                        });

                if (HasAnchor(AnchorFace.YLow))
                {
                    // move into place
                    _custom.Add(new Translate3DInfo
                    {
                        OffsetX = _xH - _widthBump,
                        OffsetY = _thickBump,
                        OffsetZ = _heightBump
                    });

                    // ctrl pivot
                    _custom.Add(new AxisAngleRotate3DInfo
                    {
                        AxisX = 0,
                        AxisY = 0,
                        AxisZ = -1,
                        Angle = OpenerAngle,
                        CenterX = _xH - _thickBump,
                        CenterY = _thickBump,
                        CenterZ = 0
                    });
                    if (InnerHinge && AnchorClose == AnchorFace.XHigh)
                        _custom.Add(
                            new Translate3DInfo
                            {
                                OffsetX = 0,
                                OffsetY = _yH - PortalledObjectA.Width,
                                OffsetZ = 0
                            });
                }
                else if (HasAnchor(AnchorFace.YHigh))
                {
                    // move into place
                    _custom.Add(new Translate3DInfo
                    {
                        OffsetX = _xH - _widthBump,
                        OffsetY = _yH - _thickBump,
                        OffsetZ = _heightBump
                    });

                    // ctrl pivot
                    _custom.Add(new AxisAngleRotate3DInfo
                    {
                        AxisX = 0,
                        AxisY = 0,
                        AxisZ = 1,
                        Angle = OpenerAngle,
                        CenterX = _xH - _thickBump,
                        CenterY = _yH - _thickBump,
                        CenterZ = 0
                    });

                    if (InnerHinge && AnchorClose == AnchorFace.XHigh)
                        _custom.Add(
                            new Translate3DInfo
                            {
                                OffsetX = 0,
                                OffsetY = PortalledObjectA.Width - _yH,
                                OffsetZ = 0
                            });
                }
                else if (HasAnchor(AnchorFace.ZLow))
                {
                    // move into place
                    _custom.Add(new Translate3DInfo
                    {
                        OffsetX = _xH - _widthBump,
                        OffsetY = _heightBump,
                        OffsetZ = _thickBump
                    });

                    // ctrl pivot
                    _custom.Add(new AxisAngleRotate3DInfo
                    {
                        AxisX = 0,
                        AxisY = 1,
                        AxisZ = 0,
                        Angle = OpenerAngle,
                        CenterX = _xH - _thickBump,
                        CenterY = 0,
                        CenterZ = _thickBump
                    });
                    if (InnerHinge && AnchorClose == AnchorFace.XHigh)
                        _custom.Add(
                            new Translate3DInfo
                            {
                                OffsetX = 0,
                                OffsetY = 0,
                                OffsetZ = _zH - PortalledObjectA.Width
                            });
                }
                else // ZHigh
                {
                    // move into place
                    _custom.Add(new Translate3DInfo
                    {
                        OffsetX = _xH - _widthBump,
                        OffsetY = _heightBump,
                        OffsetZ = _zH - _thickBump
                    });

                    // ctrl pivot
                    _custom.Add(new AxisAngleRotate3DInfo
                    {
                        AxisX = 0,
                        AxisY = -1,
                        AxisZ = 0,
                        Angle = OpenerAngle,
                        CenterX = _xH - _thickBump,
                        CenterY = 0,
                        CenterZ = _zH - _thickBump
                    });
                    if (InnerHinge && AnchorClose == AnchorFace.XHigh)
                        _custom.Add(
                            new Translate3DInfo
                            {
                                OffsetX = 0,
                                OffsetY = 0,
                                OffsetZ = PortalledObjectA.Width - _zH
                            });
                }
                if (InnerHinge && AnchorOpen == AnchorFace.XHigh)
                    _custom.Add(
                       new Translate3DInfo
                       {
                           OffsetX = PortalledObjectA.Width - _xH,
                           OffsetY = 0,
                           OffsetZ = 0
                       });
            }
            else if (HasAnchor(AnchorFace.YLow))
            {
                _InvertOpener = AnchorClose == AnchorFace.YLow;

                // flip around Y
                if (Flip ^ NativeFlip)
                    _custom.Add(
                        new AxisAngleRotate3DInfo
                        {
                            AxisX = 0,
                            AxisY = 1,
                            AxisZ = 0,
                            Angle = 180
                        });

                if (HasAnchor(AnchorFace.ZLow))
                {
                    // put in place
                    _custom.Add(new Translate3DInfo
                    {
                        OffsetX = _heightBump,
                        OffsetY = _widthBump,
                        OffsetZ = _thickBump
                    });

                    // ctrl pivot
                    _custom.Add(new AxisAngleRotate3DInfo
                    {
                        AxisX = 1,
                        AxisY = 0,
                        AxisZ = 0,
                        Angle = OpenerAngle,
                        CenterX = 0,
                        CenterY = _thickBump,
                        CenterZ = _thickBump
                    });
                    if (InnerHinge && AnchorClose == AnchorFace.YLow)
                        _custom.Add(
                            new Translate3DInfo
                            {
                                OffsetX = 0,
                                OffsetY = 0,
                                OffsetZ = _zH - PortalledObjectA.Width
                            });
                }
                else // ZHigh
                {
                    // put in place
                    _custom.Add(new Translate3DInfo
                    {
                        OffsetX = _heightBump,
                        OffsetY = _widthBump,
                        OffsetZ = _zH - _thickBump
                    });

                    // ctrl pivot
                    _custom.Add(new AxisAngleRotate3DInfo
                    {
                        AxisX = -1,
                        AxisY = 0,
                        AxisZ = 0,
                        Angle = OpenerAngle,
                        CenterX = 0,
                        CenterY = _thickBump,
                        CenterZ = _zH - _thickBump
                    });
                    if (InnerHinge && AnchorClose == AnchorFace.YLow)
                        _custom.Add(
                            new Translate3DInfo
                            {
                                OffsetX = 0,
                                OffsetY = 0,
                                OffsetZ = PortalledObjectA.Width - _zH
                            });
                }
                if (InnerHinge && AnchorOpen == AnchorFace.YLow)
                    _custom.Add(
                        new Translate3DInfo
                        {
                            OffsetX = 0,
                            OffsetY = _yH - PortalledObjectA.Width,
                            OffsetZ = 0
                        });
            }
            else
            {
                _InvertOpener = AnchorClose == AnchorFace.YHigh;

                // flip around Y
                if (Flip ^ NativeFlip)
                    _custom.Add(
                        new AxisAngleRotate3DInfo
                        {
                            AxisX = 0,
                            AxisY = 1,
                            AxisZ = 0,
                            Angle = 180
                        });

                // YHigh
                if (HasAnchor(AnchorFace.ZLow))
                {
                    // put in place
                    _custom.Add(new Translate3DInfo
                    {
                        OffsetX = _heightBump,
                        OffsetY = _yH - _widthBump,
                        OffsetZ = _thickBump
                    });

                    // ctrl pivot
                    _custom.Add(new AxisAngleRotate3DInfo
                    {
                        AxisX = -1,
                        AxisY = 0,
                        AxisZ = 0,
                        Angle = OpenerAngle,
                        CenterX = 0,
                        CenterY = _yH - _thickBump,
                        CenterZ = _thickBump
                    });
                    if (InnerHinge && AnchorClose == AnchorFace.YHigh)
                        _custom.Add(
                            new Translate3DInfo
                            {
                                OffsetX = 0,
                                OffsetY = 0,
                                OffsetZ = _zH - PortalledObjectA.Width
                            });
                }
                else // ZHigh
                {
                    // put in place
                    _custom.Add(new Translate3DInfo
                    {
                        OffsetX = _heightBump,
                        OffsetY = _yH - _widthBump,
                        OffsetZ = _zH - _thickBump
                    });

                    // ctrl pivot
                    _custom.Add(new AxisAngleRotate3DInfo
                    {
                        AxisX = 1,
                        AxisY = 0,
                        AxisZ = 0,
                        Angle = OpenerAngle,
                        CenterX = 0,
                        CenterY = _yH - _thickBump,
                        CenterZ = _zH - _thickBump
                    });
                    if (InnerHinge && AnchorClose == AnchorFace.YHigh)
                        _custom.Add(
                            new Translate3DInfo
                            {
                                OffsetX = 0,
                                OffsetY = 0,
                                OffsetZ = PortalledObjectA.Width - _zH
                            });
                }
                if (InnerHinge && AnchorOpen == AnchorFace.YHigh)
                    _custom.Add(
                        new Translate3DInfo
                        {
                            OffsetX = 0,
                            OffsetY = PortalledObjectA.Width - _yH,
                            OffsetZ = 0
                        });
            }
            return _custom;
        }
        #endregion

        #region public double OverridePivot()
        public double OverridePivot()
        {
            if (HasAnchor(AnchorFace.XLow))
            {
                // ensure these are reset to 0
                return 0;
            }
            else if (HasAnchor(AnchorFace.XHigh))
            {
                // get the hinge side facing the corner
                return 180;
            }
            else if (HasAnchor(AnchorFace.YLow))
            {
                return 90;
            }
            else
            {
                return -90;
            }
        }
        #endregion

        #region public double OverrideTilt()
        public double OverrideTilt()
        {
            if (HasAnchor(AnchorFace.XLow))
            {
                // ensure these are reset to 0
                return 0;
            }
            else if (HasAnchor(AnchorFace.XHigh))
            {
                if (HasAnchor(AnchorFace.YLow))
                {
                    return 0;
                }
                else if (HasAnchor(AnchorFace.YHigh))
                {
                    return 0;
                }
                else if (HasAnchor(AnchorFace.ZLow))
                {
                    return 90;
                }
                else // ZHigh
                {
                    return 90;
                }
            }
            else if (HasAnchor(AnchorFace.YLow))
            {
                return 90;
            }
            else
            {
                return -90;
            }
        }
        #endregion

        #region IAlterLocalLink Members

        #region public bool CanAlterLink(LocalLink link)
        public bool CanAlterLink(LocalLink link)
        {
            // NOTE: this can affect two links, one that is compatible on the close face, and one on the open face
            var _face = link.GetAnchorFaceForRegion(this.GetLocated().Locator.GeometricRegion);
            if (AnchorClose == _face)
            {
                // close face is the link's anchor face
                return OpenState.Value < 0.75d;
            }
            else if (AnchorOpen == _face)
            {
                // open face is the link's anchor face
                return OpenState.Value > 0.25d;
            }
            return false;
        }
        #endregion

        private double ClosedState(LocalLink link)
        {
            var _closeState = 1 - OpenState.Value;
            if ((link.AnchorFaceInA == AnchorOpen) || link.AnchorFaceInA.IsOppositeTo(AnchorOpen))
            {
                // however, if the link's anchorFace is compatible with portal's open face, then an open door is maximum blockage
                _closeState = OpenState.Value;
            }
            return _closeState;
        }

        public double AllowLightFactor(LocalLink link)
        {
            // OS=1 :: block=0
            // OS=0 :: block=opacity
            var _opacity = Math.Max(PortalledObjectA.Opacity, PortalledObjectB.Opacity);
            var _block = ClosedState(link) * _opacity;

            // when light is in a deep shadows room, reduce it through the link (indicating less reflecting glow)
            if (this.GetLocated()?.Locator.GetLocalCellGroups().FirstOrDefault()?.DeepShadows ?? false)
                return (1 - _block) / 8d;
            else
                return (1 - _block);
        }

        public int GetExtraSoundDifficulty(LocalLink link)
            => Convert.ToInt32(ClosedState(link) * Math.Max(PortalledObjectA.ExtraSoundDifficulty.EffectiveValue, PortalledObjectB.ExtraSoundDifficulty.EffectiveValue));

        #endregion
    }
}