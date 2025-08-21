using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Interactions;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Adjunct that makes a CoreObject respond to illuminate interactions (that is, shed light) in all directions</summary>
    [Serializable]
    public class Illumination : Adjunct, IIllumination, IPathDependent, ICoreSettingNotify, IMonitorChange<IGeometricRegion>
    {
        #region Construction
        /// <summary>Adjunct that makes a CoreObject respond to illuminate interactions (that is, shed light) in all directions</summary>
        /// <param name="source"></param>
        /// <param name="brightRange">distance for the normal vision bright range</param>
        /// <param name="shadowyRange">distance for the normal vision shadowy range</param>
        /// <param name="veryBright">flag to indicate the bright range is very bright for creatures that are dazzled in such conditions</param>
        public Illumination(object source, double brightRange, double shadowyRange, bool veryBright)
            : base(source)
        {
            _Bright = brightRange;
            _Shadow = shadowyRange;
            _VBright = veryBright;
            _Handle = new IlluminateHandler();
        }
        #endregion

        #region data
        private bool _VBright;
        private double _Bright;
        private double _Shadow;
        private double _Z;
        private double _Y;
        private double _X;
        private IInteractHandler _Handle;
        protected PlanarPresence _Planar;
        protected Locator _Locator;

        [NonSerialized, JsonIgnore]
        protected IList<LocalCellGroup> _LastGroups = null;
        #endregion

        #region void OnActivate()
        /// <summary>Aggregates an illumination handler into an object, so it can shed light</summary>
        protected override void OnActivate(object source)
        {
            // when active, use a handler
            var _obj = Anchor as CoreObject;
            _obj?.AddIInteractHandler(_Handle);
            RecalcGroupLights();
            base.OnActivate(source);
        }
        #endregion

        #region void OnDeactivate()
        /// <summary>Removes an illumination handler from an object, so that it stops shedding light</summary>
        protected override void OnDeactivate(object source)
        {
            var _obj = Anchor as CoreObject;
            _obj?.RemoveIInteractHandler(_Handle);
            RecalcGroupLights();
            base.OnDeactivate(source);
        }
        #endregion

        public virtual bool IsVeryBright { get => _VBright; set => _VBright = value; }
        /// <summary>Bright Range or 0</summary>
        public virtual double VeryBrightRange => _VBright ? _Bright : 0;
        public virtual double BrightRange { get => _Bright; set => _Bright = value; }
        public virtual double ShadowyRange { get => _Shadow; set => _Shadow = value; }
        /// <summary>Double the shadowy range</summary>
        public virtual double FarShadowyRange => _Shadow * 2;

        public double ZOffset { get => _Z; set => _Z = value; }
        public double YOffset { get => _Y; set => _Y = value; }
        public double XOffset { get => _X; set => _X = value; }

        public IInteract LightHandler
            => Anchor as IInteract;

        public PlanarPresence PlanarPresence => _Planar;

        public bool IsUsable
            => IsActive && (Locator.FindFirstLocator(LightHandler) != null);

        public IGeometricRegion SourceGeometry(IGeometricRegion target)
            // NOTE: don't care about target, this illumination is bound to a source object
            => Locator.FindFirstLocator(LightHandler).GeometricRegion;

        #region public Point3D InteractionPoint3D { get; }
        public Point3D InteractionPoint3D(IGeometricRegion target)
        {
            var _pt = SourceGeometry(target).GetPoint3D();
            _pt.Offset(XOffset, YOffset, ZOffset);
            return _pt;
        }
        #endregion

        #region public Point3D InteractionPoint3D { get; }
        public Point3D InteractionPoint3D(Point3D target)
        {
            var _pt = SourceGeometry(null).GetPoint3D();
            _pt.Offset(XOffset, YOffset, ZOffset);
            return _pt;
        }
        #endregion

        public override object Clone()
            => new Illumination(Source, BrightRange, ShadowyRange, IsVeryBright);

        #region public LightLevel MaximumLight { get; }
        public LightRange MaximumLight
        {
            get
            {
                if (VeryBrightRange > 0)
                {
                    return LightRange.VeryBright;
                }
                else if (BrightRange > 0)
                {
                    return LightRange.Bright;
                }
                else if (ShadowyRange > 0)
                {
                    return LightRange.NearShadow;
                }
                else if (FarShadowyRange > 0)
                {
                    return LightRange.FarShadow;
                }

                return LightRange.OutOfRange;
            }
        }
        #endregion

        #region private void RecalcGroupLights()
        private void RecalcGroupLights()
        {
            // see if we are still locatable
            var _loc = Anchor.GetLocated()?.Locator;
            if (_loc != _Locator)
            {
                // switching locators
                _Locator?.RemoveChangeMonitor(this);
                _Locator = _loc;
                _Locator?.AddChangeMonitor(this);
            }

            // currently locater?
            if ((_Locator != null) && _Locator.PlanarPresence.HasMaterialPresence())
            {
                // light on ethereal locators contribute nothing
                _Planar = _Locator.PlanarPresence;

                // new groups
                var _newGroups = _Locator.GetLocalCellGroups().ToList();
                var _groups = _newGroups.Select(_g => _g).ToList();
                if (_LastGroups != null)
                {
                    // old groups
                    _groups = _groups.Union(_LastGroups).Distinct().ToList();
                }

                // notify all
                var _notifiers = _groups
                    .SelectMany(_g => _g.NotifyLighting())
                    .Distinct()
                    .ToList();
                // new last locator
                _LastGroups = _newGroups;

                AwarenessSet.RecalculateAllSensors(_Locator.Map, _notifiers, false);
            }
            else if (_LastGroups != null)
            {
                // notify lighting
                var _notifiers = _LastGroups
                    .SelectMany(_g => _g.NotifyLighting())
                    .Distinct()
                    .ToList();
                var _map = _LastGroups.FirstOrDefault()?.Map;

                // clear last light
                _LastGroups = null;

                AwarenessSet.RecalculateAllSensors(_map, _notifiers, false);
            }
        }
        #endregion

        public virtual void PathChanged(Pathed source)
            => RecalcGroupLights();

        // ICoreSettingNotify Members
        public void OnCoreSetting()
            => RecalcGroupLights();

        // NOTE: currently normal illumination does not provide solar powered, only ambient can do this
        public double SolarLeft(ICellLocation location) => 0d;
        public double VeryBrightLeft(ICellLocation location) => VeryBrightRange;
        public double BrightLeft(ICellLocation location) => BrightRange;
        public double ShadowyLeft(ICellLocation location) => ShadowyRange;
        public double FarShadowyLeft(ICellLocation location) => FarShadowyRange;

        public double NearBoostLeft(ICellLocation location)
            => (BrightRange > 0)
            ? (ShadowyRange + BrightRange) / 2
            : 0;

        public double FarBoostLeft(ICellLocation location)
            => (ShadowyRange > 0)
            ? ShadowyRange + 5
            : 0;

        public double ExtentBoostLeft(ICellLocation location)
            => (FarShadowyRange > 0)
            ? FarShadowyRange + 5
            : 0;

        public double SolarLeft(Point3D location) => 0d;
        public double VeryBrightLeft(Point3D location) => VeryBrightRange;
        public double BrightLeft(Point3D location) => BrightRange;
        public double ShadowyLeft(Point3D location) => ShadowyRange;
        public double FarShadowyLeft(Point3D location) => FarShadowyRange;

        public double NearBoostLeft(Point3D location)
            => (BrightRange > 0)
            ? (ShadowyRange + BrightRange) / 2
            : 0;

        public double FarBoostLeft(Point3D location)
            => (ShadowyRange > 0)
            ? ShadowyRange + 5
            : 0;

        public double ExtentBoostLeft(Point3D location)
            => (FarShadowyRange > 0)
            ? FarShadowyRange + 5
            : 0;

        public void PreTestChange(object sender, AbortableChangeEventArgs<IGeometricRegion> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args)
        {
            RecalcGroupLights();
        }
    }
}
