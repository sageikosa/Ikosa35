using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Skills;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using System.IO;
using System.Runtime.Serialization;
using System.Diagnostics;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Senses
{
    public interface ISensorHost : ICoreObject, ITrackTime
    {
        string SensorHostName { get; }
        bool IsSensorHostActive { get; }
        SensorySet Senses { get; }
        AwarenessSet Awarenesses { get; }
        ExtraInfoSet ExtraInfoMarkers { get; }
        RoomAwarenessSet RoomAwarenesses { get; }
        SoundAwarenessSet SoundAwarenesses { get; }

        // TODO: allows some awareness sharing between sensor hosts controlled by same creature
        SkillSet Skills { get; }

        /// <summary>Heading as last used by client view camera</summary>
        int Heading { get; set; }

        /// <summary>Incline as last used by client view camera</summary>
        int Incline { get; set; }

        double ZOffset { get; }
        double YOffset { get; }
        double XOffset { get; }

        // aim point as tracked for movement and rotation re-syncing
        double AimPointRelativeLongitude { get; set; }
        double AimPointLatitude { get; set; }
        double AimPointDistance { get; set; }

        /// <summary>aim point as set by adjustment and reported by service</summary>
        Point3D AimPoint { get; set; }

        // third person perspective camera positioning
        int ThirdCameraRelativeHeading { get; set; }
        int ThirdCameraIncline { get; set; }
        double ThirdCameraDistance { get; }

        /// <summary>Third person perspective camera point</summary>
        Point3D ThirdCameraPoint { get; set; }
    }

    public static class ISensorHostHelper
    {
        /// <summary>
        /// ISensorHost carries no geometry region info, so needs to be given a geometric region boundary to find the aiming cell
        /// </summary>
        public static CellLocation GetAimCell(this ISensorHost sensors, IGeometricRegion region)
        {
            var _aimPt = sensors.AimPoint;
            return (from _cL in region.AllCellLocations()
                    let _cLoc = new CellLocation(_cL)
                    where _cLoc.Contains(_aimPt)
                    select _cLoc).FirstOrDefault();
        }

        #region public static ICellLocation GetTargetCell(this ISensorHost self, Locator sensorLocator = null, IInteract target = null)
        /// <summary>
        /// Get a target cell for the target.  Favors the SensorHost "facing-cell", then the closest in the target region.
        /// </summary>
        public static ICellLocation GetTargetCell(this ISensorHost self, Locator sensorLocator = null, IInteract target = null)
        {
            var _loc = sensorLocator ?? Locator.FindFirstLocator(self);
            if (_loc != null)
            {
                var _aimCell = self.GetAimCell(_loc.GeometricRegion);

                // no target supplied, or target inaccessible; so get facing cell
                var _faces = AnchorFaceHelper.MovementFaces(_loc.GetGravityFace(), self.Heading, self.Incline).ToArray();
                var _facingCell = CellPosition.GetAdjacentCellPosition(_aimCell, _faces);

                // target supplied? (but no cell...)
                if (target != null)
                {
                    // ... opportunistic attacks and cleaves may find themselves in this situation ...
                    var _tRgn = Locator.FindFirstLocator(target)?.GeometricRegion;
                    if (_tRgn != null)
                    {
                        // if facing cell is not in region
                        if (!_tRgn.ContainsCell(_facingCell))
                        {
                            // closest cell
                            return _tRgn.AllCellLocations().OrderBy(_cl => IGeometricHelper.Distance(_aimCell, _cl)).FirstOrDefault();
                        }
                    }
                }

                // no target, or target contains facing cell
                return _facingCell;
            }
            return null;
        }
        #endregion

        /// <summary>Provides a snapshot terrain visualizer built from current active senses</summary>
        public static TerrainVisualizer GetTerrainVisualizer(this ISensorHost sensors)
        {
            var _loc = sensors.GetLocated()?.Locator;
            return new TerrainVisualizer(sensors.Senses, _loc?.LightLevel ?? LightRange.OutOfRange, _loc.PlanarPresence);
        }

        #region public static SensorHostInfo ToSensorHostInfo(this ISensorHost sensorHost)
        public static SensorHostInfo ToSensorHostInfo(this ISensorHost sensorHost)
        {
            var _info = new SensorHostInfo()
            {
                SensorHostName = sensorHost.SensorHostName,
                ID = sensorHost.ID.ToString(),
                ForTerrain = sensorHost.Senses.AllSenses.Any(_s => _s.ForTerrain),
                ForTargeting = sensorHost.Senses.AllSenses.Any(_s => _s.ForTargeting),
                Heading = sensorHost.Heading,
                Incline = sensorHost.Incline,
                Offset = new TacticalPoint
                {
                    Z = sensorHost.ZOffset,
                    Y = sensorHost.YOffset,
                    X = sensorHost.XOffset
                },
                AimPoint = new TacticalPoint
                {
                    Z = sensorHost.AimPoint.Z,
                    Y = sensorHost.AimPoint.Y,
                    X = sensorHost.AimPoint.X
                },

                AimPointRelLongitude = sensorHost.AimPointRelativeLongitude,
                AimPointRelLatitude = sensorHost.AimPointLatitude,

                ThirdCameraPoint = new TacticalPoint
                {
                    Z = sensorHost.ThirdCameraPoint.Z,
                    Y = sensorHost.ThirdCameraPoint.Y,
                    X = sensorHost.ThirdCameraPoint.X
                },
                ThirdCameraRelativeHeading = sensorHost.ThirdCameraRelativeHeading,
                ThirdCameraIncline = sensorHost.ThirdCameraIncline
            };

            // get cell that contains aim point
            var _cLoc = (from _cl in _info.AllCellLocations()
                         let _c = new CellLocation(_cl)
                         where _c.Contains(sensorHost.AimPoint)
                         select _c).FirstOrDefault();
            if (_cLoc != null)
            {
                _info.AimCellZ = _cLoc.Z;
                _info.AimCellY = _cLoc.Y;
                _info.AimCellX = _cLoc.X;
            }
            else
            {
                _info.AimCellZ = null;
                _info.AimCellY = null;
                _info.AimCellX = null;
            }

            var _loc = sensorHost.GetLocated();
            if (_loc != null)
            {
                var _locator = _loc.Locator;
                _info.SetCubicInfo(_locator.GeometricRegion);
                _info.GravityFace = (int)_locator.BaseFace;

                var _rgn = _locator.GeometricRegion;
                var _zExt = (double)(_rgn.UpperZ - _rgn.LowerZ + 1) * 5d;
                var _yExt = (double)(_rgn.UpperY - _rgn.LowerY + 1) * 5d;
                var _xExt = (double)(_rgn.UpperX - _rgn.LowerX + 1) * 5d;
                var _max = 0.5d * Math.Sqrt((_zExt * _zExt) + (_yExt * _yExt) + (_xExt * _xExt));
                if (_max > 0)
                {
                    _info.AimPointRelDistance = sensorHost.AimPointDistance / _max;
                }

                // get stuff to build PointEffects
                var _map = _locator.Map;
                var _visualizer = sensorHost.GetTerrainVisualizer();
                if (_cLoc != null)
                {
                    // AimCell
                    _info.AimCellEffect = GetCellEffect(_map, _visualizer, _cLoc, sensorHost.AimPoint);
                }

                // effect for Center Cell...
                var _center = _info.GetPoint3D();
                _cLoc = (from _cl in _info.AllCellLocations()
                         let _c = new CellLocation(_cl)
                         where _c.Contains(_center)
                         select _c).FirstOrDefault();
                if (_cLoc != null)
                {
                    _info.CenterEffect = GetCellEffect(_map, _visualizer, _cLoc, _center);
                }

                // effect for Third Camera Cell...
                _cLoc = (from _cl in _info.AllCellLocations()
                         let _c = new CellLocation(_cl)
                         where _c.Contains(sensorHost.ThirdCameraPoint)
                         select _c).FirstOrDefault();
                if (_cLoc != null)
                {
                    _info.ThirdCameraEffect = GetCellEffect(_map, _visualizer, _cLoc,
                        sensorHost.ThirdCameraPoint);
                }
            }
            else
            {
                _info.GravityFace = (int)AnchorFace.ZLow;
                _info.AimPointRelDistance = 1;
                _info.AimCellEffect = null;
                _info.CenterEffect = null;
                _info.ThirdCameraEffect = null;
            }
            return _info;
        }
        #endregion

        private static PointEffect GetCellEffect(LocalMap map, TerrainVisualizer visualizer,
            CellLocation location, Point3D point)
        {
            // AimCell
            // TODO: ...also, may use monochrone if darkvision active...?
            // TODO: ... ... and, some visual effects should be determined by submersion in stuff ... ...
            var _effect = map.GetVisualEffect(location, location, visualizer);
            var (_collectionKey, _brushKey) = map[location].InnerBrushKeys(point);
            return new PointEffect
            {
                Effect = _effect,
                BrushKey = _brushKey,
                BrushSet = _collectionKey
            };
        }
    }
}
