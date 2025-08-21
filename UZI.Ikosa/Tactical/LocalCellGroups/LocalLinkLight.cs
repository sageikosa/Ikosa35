using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Senses;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using System.Diagnostics;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    /// <summary>Used to reproject all light received from one side of a local link to the other side</summary>
    public class LocalLinkLight : IIllumination, IInteract
    {
        #region construction
        public LocalLinkLight(LocalLink link, bool isA)
        {
            _Link = link;
            _IsA = isA;
            double _flip = isA ? 1 : -1;
            var _grp = isA ? link.GroupA : link.GroupB;
            _Cell = link.InteractionCell(_grp);
            _Normal = _Link.AnchorFaceInA switch
            {
                AnchorFace.ZLow => new Vector3D(0, 0, 1 * _flip),
                AnchorFace.ZHigh => new Vector3D(0, 0, -1 * _flip),
                AnchorFace.YLow => new Vector3D(0, 1 * _flip, 0),
                AnchorFace.YHigh => new Vector3D(0, -1 * _flip, 0),
                AnchorFace.XLow => new Vector3D(1 * _flip, 0, 0),
                _ => new Vector3D(-1 * _flip, 0, 0),
            };
            _Refs = [];
            _Displace = _grp.Map[_Cell.Z, _Cell.Y, _Cell.X, _grp].InteractionOffset3D();
        }
        #endregion

        #region state
        private LocalLink _Link;
        private double _Solar;
        private double _VBright;
        private double _Bright;
        private double _Shadow;
        private double _FarShadow;
        private bool _IsA;
        private ICellLocation _Cell;

        /// <summary>Represents the normal vector for the link transitory face</summary>
        private Vector3D _Normal;

        private Vector3D _Displace;
        private List<LightReference> _Refs;
        #endregion

        #region public bool NotifyLights(IEnumerable<IIllumination> sourceLights)
        /// <summary>Calculates the light passing through and indicates whether it changed</summary>
        public bool NotifyLights(IEnumerable<IIllumination> sourceLights)
        {
            var _oldSolar = _Solar;
            var _oldVBright = _VBright;
            var _oldBright = _Bright;
            var _oldShadow = _Shadow;
            var _oldFar = _FarShadow;
            var _pt = Link.LinkCube.GetPoint3D(); 

            // get lights that are either non-links, or that only share one group with our link
            // ... this prevents links between the same groups from feeding each other
            _Refs = (from _light in sourceLights
                     let _lnk = _light as LocalLinkLight
                     where ((_lnk == null) || (_lnk.Link.Groups.Intersect(_Link.Groups).Count() == 1))
                     let _lRef = new LightReference(_light, _Link, _pt, _Normal)
                     where _lRef.IsProvidingLight
                     select _lRef).ToList();

            double _snapUp(double source, double snap)
                => Math.Ceiling(source / snap) * snap;

            // find best lights
            _Solar = _snapUp((from _l in _Refs
                              let _attenuate = Attenuation(_l.LinkAngle, Sharpness.Direct)
                              let _sol = _l.SolarRange * _attenuate
                              where (_sol > 0)
                              orderby _sol descending
                              select _l.VeryBrightRange).FirstOrDefault(), 5d);
            _VBright = _snapUp((from _l in _Refs
                                let _attenuate = Attenuation(_l.LinkAngle, Sharpness.Tight)
                                let _vbr = _l.VeryBrightRange * _attenuate
                                where (_vbr > 0)
                                orderby _vbr descending
                                select _l.VeryBrightRange).FirstOrDefault(), 5d);
            _Bright = _snapUp((from _l in _Refs
                               let _attenuate = Attenuation(_l.LinkAngle, Sharpness.Moderate)
                               let _br = _l.BrightRange * _attenuate
                               where _br > 0
                               orderby _br descending
                               select _l.BrightRange).FirstOrDefault(), 5d);
            _Shadow = _snapUp((from _l in _Refs
                               let _attenuate = Attenuation(_l.LinkAngle, Sharpness.Loose)
                               let _sh = _l.ShadowyRange * _attenuate
                               where _sh > 0
                               orderby _l.ShadowyRange descending
                               select _l.ShadowyRange).FirstOrDefault(), 1d);
            _FarShadow = _snapUp((from _l in _Refs
                                  let _attenuate = Attenuation(_l.LinkAngle, Sharpness.Lax)
                                  let _fs = _l.FarShadowRange * _attenuate
                                  where _fs > 0
                                  orderby _fs descending
                                  select _l.FarShadowRange).FirstOrDefault(), 1d);

            // TODO: may be some cases where notification isn't propogated when it should be...
            return (_oldSolar != _Solar)
                || (_oldVBright != _VBright)
                || (_oldBright != _Bright)
                || (_oldShadow != _Shadow)
                || (_oldFar != _FarShadow);
        }
        #endregion

        public LocalLink Link => _Link;
        public IInteract LightHandler => this;
        public double Solar => _Solar * Link.AllowLightFactor;
        public double VeryBrightRange => _VBright * Link.AllowLightFactor;
        public double BrightRange => _Bright * Link.AllowLightFactor;
        public double ShadowyRange => _Shadow * Link.AllowLightFactor;
        public double FarShadowyRange => _FarShadow * Link.AllowLightFactor;
        public object Source => this;
        public IGeometricRegion SourceGeometry(IGeometricRegion target) => _Link.LinkCube;
        public Point3D InteractionPoint3D(IGeometricRegion target) => Link.LinkCube.GetPoint3D() + _Displace;
        public Point3D InteractionPoint3D(Point3D target) => Link.LinkCube.GetPoint3D() + _Displace;
        public Guid ID => _Link.ID;
        public Vector3D Normal => _Normal;
        public PlanarPresence PlanarPresence => PlanarPresence.Material;

        public bool IsUsable
            => IsActive;

        public bool IsActive
            => (Solar > 0) || (VeryBrightRange > 0) || (BrightRange > 0)
            || (ShadowyRange > 0) || (FarShadowyRange > 0);

        public IEnumerable<IIllumination> ReferencedLights
            => _Refs.Select(_r => _r.IIlumination).Where(_i => _i?.IsUsable ?? false);

        #region public LightLevel MaximumLight { get; }
        public LightRange MaximumLight
        {
            get
            {
                if (Solar > 0)
                {
                    return LightRange.Solar;
                }
                else if (VeryBrightRange > 0)
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

        #region IInteract Members

        public void HandleInteraction(Interaction interact)
        {
            // illuminate is a request to provide light to a target
            if (interact.InteractData as Illuminate != null)
            {
                if ((interact.InteractData as Illuminate).LightTarget == _Link)
                {
                    // we do not light up our link (otherwise we could light up our own holding object)
                    return;
                }
                else if (IsActive)
                {
                    // ... then the link can provide light
                    var _handler = new IlluminateHandler();
                    _handler.HandleInteraction(interact);
                    // NOTE: otherwise, the target is in the same "room" as the light, and doesn't need the link
                }
            }
        }

        #endregion

        #region light attenuation
        private enum Sharpness { Direct, Tight, Moderate, Loose, Lax };

        private double Attenuation(in Vector3D refVector, in Vector3D targetVector, Sharpness sharp)
        {
            var _factor = Math.Min(Math.Max(targetVector.Length - 5d, 0d) / 15d, 1d);
            return Attenuation(_factor * Math.Min(Vector3D.AngleBetween(refVector, targetVector), 90), sharp);
        }

        private double Attenuation(double angle, Sharpness sharp)
        {
            switch (Link.ShadowModel)
            {
                case ShadowModel.Deep:
                    switch (sharp)
                    {
                        case Sharpness.Direct:
                            return Math.Max(Math.Min(1d, (7.5d - angle) / 6d), 0d);

                        case Sharpness.Tight:
                            return Math.Max(Math.Min(1d, (12.5d - angle) / 10d), 0d);

                        case Sharpness.Moderate:
                            return Math.Max(Math.Min(1d, (20d - angle) / 15d), 0d);

                        case Sharpness.Loose:
                            return Math.Max(Math.Min(1d, (40d - angle) / 20d), 0d);

                        case Sharpness.Lax:
                        default:
                            return Math.Max(Math.Min(1d, (60d - angle) / 30d), 0d);
                    }
                case ShadowModel.Mixed:
                    switch (sharp)
                    {
                        case Sharpness.Direct:
                            return Math.Max(Math.Min(1d, (10d - angle) / 8d), 0d);

                        case Sharpness.Tight:
                            return Math.Max(Math.Min(1d, (16d - angle) / 12.5d), 0d);

                        case Sharpness.Moderate:
                            return Math.Max(Math.Min(1d, (30d - angle) / 17.5d), 0d);

                        case Sharpness.Loose:
                            return Math.Max(Math.Min(1d, (50d - angle) / 25d), 0d);

                        case Sharpness.Lax:
                        default:
                            return Math.Max(Math.Min(1d, (70d - angle) / 35d), 0d);
                    }

                case ShadowModel.Normal:
                default:
                    switch (sharp)
                    {
                        case Sharpness.Direct:
                            return Math.Max(Math.Min(1d, (12.5d - angle) / 10d), 0d);

                        case Sharpness.Tight:
                            return Math.Max(Math.Min(1d, (20d - angle) / 15d), 0d);

                        case Sharpness.Moderate:
                            return Math.Max(Math.Min(1d, (40d - angle) / 20d), 0d);

                        case Sharpness.Loose:
                            return Math.Max(Math.Min(1d, (60d - angle) / 30d), 0d);

                        case Sharpness.Lax:
                        default:
                            return Math.Max(Math.Min(1d, (80d - angle) / 40d), 0d);
                    }
            }
        }
        #endregion

        #region IIllumination Members

        private double LevelLeft(ICellLocation location, Sharpness sharpness, double maxLevel, Func<LightReference, double> refLevel)
        {
            if (!location.IsCellEqual(_Cell) && (_Refs?.Count > 0))
            {
                var _vector = (location.GetPoint() - _Link.LinkCube.GetPoint3D());
                return (from _lr in _Refs
                        let _attenuate = Attenuation(_lr.Vector, _vector, sharpness)
                        select refLevel(_lr) * _attenuate).Max() * Link.AllowLightFactor;
            }
            else
            {
                return maxLevel * Link.AllowLightFactor;
            }
        }

        public double SolarLeft(ICellLocation location)
            => LevelLeft(location, Sharpness.Direct, _Solar, (lr) => lr.SolarRange);

        public double VeryBrightLeft(ICellLocation location)
            => LevelLeft(location, Sharpness.Tight, _VBright, (lr) => lr.VeryBrightRange);

        public double BrightLeft(ICellLocation location)
            => LevelLeft(location, Sharpness.Moderate, _Bright, (lr) => lr.BrightRange);

        public double ShadowyLeft(ICellLocation location)
            => LevelLeft(location, Sharpness.Loose, _Shadow, (lr) => lr.ShadowyRange);

        public double FarShadowyLeft(ICellLocation location)
            => LevelLeft(location, Sharpness.Lax, _FarShadow, (lr) => lr.FarShadowRange);

        public double NearBoostLeft(ICellLocation location)
            => (BrightRange > 0) ? (BrightLeft(location) + ShadowyLeft(location)) / 2 : 0;

        public double FarBoostLeft(ICellLocation location)
            => (ShadowyRange > 0) ? ShadowyLeft(location) + 5 : 0;

        public double ExtentBoostLeft(ICellLocation location)
            => (FarShadowyRange > 0) ? FarShadowyLeft(location) + 5 : 0;

        private double LevelLeft(Point3D location, Sharpness sharpness, double maxLevel, Func<LightReference, double> refLevel)
        {
            var _vector = (location - _Link.LinkCube.GetPoint3D());
            var _full = _vector.Length <= 5d;
            if (!_full && (_Refs?.Count > 0))
            {
                return (from _lr in _Refs
                        let _attenuate = Attenuation(_lr.Vector, _vector, sharpness)
                        select refLevel(_lr) * _attenuate).Max() * Link.AllowLightFactor;
            }
            else
            {
                return maxLevel * Link.AllowLightFactor;
            }
        }

        public double SolarLeft(Point3D location)
            => LevelLeft(location, Sharpness.Direct, _Solar, (lr) => lr.SolarRange);

        public double VeryBrightLeft(Point3D location)
            => LevelLeft(location, Sharpness.Tight, _VBright, (lr) => lr.VeryBrightRange);

        public double BrightLeft(Point3D location)
            => LevelLeft(location, Sharpness.Moderate, _Bright, (lr) => lr.BrightRange);

        public double ShadowyLeft(Point3D location)
            => LevelLeft(location, Sharpness.Loose, _Shadow, (lr) => lr.ShadowyRange);

        public double FarShadowyLeft(Point3D location)
            => LevelLeft(location, Sharpness.Lax, _FarShadow, (lr) => lr.FarShadowRange);

        public double NearBoostLeft(Point3D location)
            => (BrightRange > 0) ? (BrightLeft(location) + ShadowyLeft(location)) / 2 : 0;

        public double FarBoostLeft(Point3D location)
            => (ShadowyRange > 0) ? ShadowyLeft(location) + 5 : 0;

        public double ExtentBoostLeft(Point3D location)
            => (FarShadowyRange > 0) ? FarShadowyLeft(location) + 5 : 0;
        #endregion
    }
}