using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class ShadingZoneSet : IEnumerable<ShadingZone>
    {
        public ShadingZoneSet(LocalMap map)
        {
            _Map = map;
            _Zones = new Collection<ShadingZone>();
        }

        #region private data
        private LocalMap _Map;
        private Collection<ShadingZone> _Zones;

        private static Vector3D _ZDrop;
        private static Vector3D _ZLift;
        private static Vector3D _YDrop;
        private static Vector3D _YLift;
        private static Vector3D _XDrop;
        private static Vector3D _XLift;
        #endregion

        #region static construction
        static ShadingZoneSet()
        {
            _ZDrop = new Vector3D(0, 0, -5);
            _ZLift = new Vector3D(0, 0, 5);
            _YDrop = new Vector3D(0, -5, 0);
            _YLift = new Vector3D(0, 5, 0);
            _XDrop = new Vector3D(-5, 0, 0);
            _XLift = new Vector3D(5, 0, 0);
        }
        #endregion

        public LocalMap Map => _Map;

        private IEnumerable<ShadingZone> Containers(ICellLocation location)
            => _Zones.Where(_z => _z.Cube.ContainsCell(location));

        public bool IsSkipped(ICellLocation location)
            => Containers(location).Any(_z => _z[location] == LightShadeLevel.Skip);

        public bool IsMagicDark(ICellLocation location)
            => Containers(location).Any(_z => _z[location] == LightShadeLevel.MagicDark);

        #region public LightRange GetLightLevel(ICellLocation location)
        public LightRange GetLightLevel(ICellLocation location, LightRange fallback)
        {
            switch (Containers(location).Max(_z => (LightShadeLevel?)_z[location]))
            {
                case LightShadeLevel.VeryBright:
                    return LightRange.VeryBright; // TODO: solar?
                case LightShadeLevel.Bright:
                    return LightRange.Bright;
                case LightShadeLevel.NearBoost:
                    return LightRange.NearBoost;
                case LightShadeLevel.NearShadow:
                    return LightRange.NearShadow;
                case LightShadeLevel.FarBoost:
                    return LightRange.FarBoost;
                case LightShadeLevel.FarShadow:
                    return LightRange.FarShadow;
                case LightShadeLevel.ExtentBoost:
                    return LightRange.ExtentBoost;
            }
            return fallback;
        }
        #endregion

        #region public void ReshadeBackground()
        /// <summary>Recalculates shading across all shading zones in background</summary>
        public void ReshadeBackground()
        {
            // TODO: parallelize getting lights and darks

            #region magic dark, magic light and regular light lists
            // all magic dark on the map
            var _mDarks = (from _cap in Map.MapContext.LocatorZones.AllCaptures()
                           where _cap.Capturer is MagicDark
                           let _md = _cap.Capturer as MagicDark
                           let _mpe = _md.Source as MagicPowerEffect
                           orderby _mpe.PowerLevel descending
                           select new { Capture = _cap, MagicEffect = _mpe, Dark = _md }).ToList();

            // all magic light (Bright capture zone) on the map (to attempt to counter magic dark)
            var _mLights = (from _cap in Map.MapContext.LocatorZones.AllCaptures()
                            where _cap.Capturer is MagicLight
                            let _ml = _cap.Capturer as MagicLight
                            let _mpe = _ml.Source as MagicPowerEffect
                            orderby _mpe.PowerLevel descending
                            select new { Capture = _cap, MagicEffect = _mpe, Light = _ml }).ToList();

            // find lights needed to illuminate (lights in background, UNIONed with linked-in)
            var _bgLights = (from _ill in Map.MapContext.ListEffectsInBackground<Illumination>()
                             where _ill.IsActive
                             && (_ill.MaximumLight > LightRange.OutOfRange)
                             && _ill.PlanarPresence.HasMaterialPresence()
                             select _ill as IIllumination).Union(from _bg in Map.Backgrounds.All()
                                                                 from _lnk in _bg.Links.All
                                                                 from _lnkLight in _lnk.GetLights(_bg, false)
                                                                 where (_lnkLight != null)
                                                                 && (_lnkLight.MaximumLight > LightRange.OutOfRange)
                                                                 select _lnkLight).ToList();
            #endregion

            // step through zones, clearing all levels...
            Parallel.ForEach(_Zones, (zone) => zone.Wipe());

            // step through zones, apply calculated cell level to each zone containing cell...
            // NOTE: each zone is done serially, since they might overlap
            foreach (var _shade in _Zones)
            {
                // NOTE: each partition within a zone is parallelizable
                Parallel.ForEach(_shade.Cube.AllCubicPartitions(), (cells) =>
                {
                    foreach (var _loc in cells)
                    {
                        // if already calculated (via an overlapping boundary), we can skip...
                        // any cell in a room is handled by room lighting, so skip
                        if (Map.RoomIndex.GetRoom(_loc) != null)
                        {
                            _shade[_loc] = LightShadeLevel.Skip;
                        }
                        else
                        {
                            ref readonly var _cell = ref Map[_loc];
                            var _doCalc = true;

                            #region check solid cells and gasesous surrounding to skip calcs for this cell
                            // if cell borders only invisible uniform gases, no point in checking shading...
                            if (!_cell.IsShadeable)
                            {
                                // since it isn't natively shadeable, check if cannot get light, or doesn't need shade calcs
                                if (_cell.Template.GetType() == typeof(CellSpace))
                                {
                                    var _uniform = _cell.Template as CellSpace;
                                    if (_uniform.CellMaterial is SolidCellMaterial)
                                    {
                                        // NO LIGHT in a solid uniform cell
                                        _doCalc = false;
                                    }
                                    else
                                    {
                                        // see if it is surrounded by invisible gases
                                        if (Map[_loc, AnchorFace.ZLow, null].IsInvisibleGasCell()
                                            && Map[_loc, AnchorFace.ZHigh, null].IsInvisibleGasCell()
                                            && Map[_loc, AnchorFace.YLow, null].IsInvisibleGasCell()
                                            && Map[_loc, AnchorFace.YHigh, null].IsInvisibleGasCell()
                                            && Map[_loc, AnchorFace.XLow, null].IsInvisibleGasCell()
                                            && Map[_loc, AnchorFace.XHigh, null].IsInvisibleGasCell())
                                        {
                                            // surrounding cells have no visible parts, so don't need to calc
                                            _doCalc = false;
                                        }
                                    }
                                }
                            }
                            #endregion

                            if (_doCalc)
                            {
                                // Lights to Use
                                var _useLights =
                                    Map.Backgrounds.GetBackgroundCellGroup(_loc)?.GetAmbientIlluminators().Union(_bgLights).ToList()
                                    ?? _bgLights;

                                #region magic dark and magic light
                                // get the best darkness
                                var _bestDark = _mDarks.FirstOrDefault(_d => _d.Capture.ContainsCell(_loc, null, PlanarPresence.Material));
                                if (_bestDark != null)
                                {
                                    var _bestMLight = _mLights.FirstOrDefault(_l => _l.Capture.ContainsCell(_loc, null, PlanarPresence.Material));
                                    if (_bestMLight != null)
                                    {
                                        if (_bestDark.MagicEffect.PowerLevel > _bestMLight.MagicEffect.PowerLevel)
                                        {
                                            _shade[_loc] = LightShadeLevel.MagicDark;
                                            _doCalc = false;
                                        }
                                        else if (_bestDark.MagicEffect.PowerLevel < _bestMLight.MagicEffect.PowerLevel)
                                        {
                                            _shade[_loc] = _bestMLight.Light.IsVeryBright ? LightShadeLevel.VeryBright : LightShadeLevel.Bright;
                                            _doCalc = false;
                                        }
                                        else
                                        {
                                            // cancellation (no magic lighting to check shading)
                                            _useLights = (from _l in _useLights
                                                          where !(_l is MagicLight)
                                                          select _l).ToList();
                                        }
                                    }
                                }
                                #endregion

                                // still calculations
                                if (_doCalc)
                                {
                                    #region gather light range
                                    // this should pick up ambient levels as well ...
                                    var _currentRange = LightRange.OutOfRange;
                                    var _max = _useLights.Any()
                                        ? _useLights.Max(_l => _l.MaximumLight)
                                        : LightRange.OutOfRange;
                                    foreach (var _l in _useLights)
                                    {
                                        var _lp = _l.InteractionPoint3D(_loc);
                                        var _dist = IGeometricHelper.Distance(_lp, _loc);
                                        var _cellLeft = new CellLightRanger(_l, _loc);
                                        var _newRange = _cellLeft.CurrentRange;
                                        if (_newRange > _currentRange)
                                        {
                                            if (Map.HasLineOfEffect(_lp, _l.SourceGeometry(_loc), _loc, PlanarPresence.Material))
                                            {
                                                _currentRange = _newRange;
                                            }
                                            else
                                            {
                                                if (_newRange > LightRange.ExtentBoost)
                                                {
                                                    _newRange = _newRange - 2;
                                                    if (_newRange > _currentRange)
                                                    {
                                                        _currentRange = _newRange;
                                                    }
                                                }
                                            }
                                        }
                                        if (_currentRange >= _max)
                                            break;
                                    }
                                    #endregion

                                    #region setCellShadeFlags()
                                    // flag as processed to minimize overlap processing
                                    switch (_currentRange)
                                    {
                                        case LightRange.ExtentBoost:
                                            _shade[_loc] = LightShadeLevel.ExtentBoost;
                                            break;
                                        case LightRange.FarShadow:
                                            _shade[_loc] = LightShadeLevel.FarShadow;
                                            break;
                                        case LightRange.FarBoost:
                                            _shade[_loc] = LightShadeLevel.FarBoost;
                                            break;
                                        case LightRange.NearShadow:
                                            _shade[_loc] = LightShadeLevel.NearShadow;
                                            break;
                                        case LightRange.NearBoost:
                                            _shade[_loc] = LightShadeLevel.NearBoost;
                                            break;
                                        case LightRange.Bright:
                                            _shade[_loc] = LightShadeLevel.Bright;
                                            break;
                                        case LightRange.VeryBright:
                                        case LightRange.Solar:
                                            _shade[_loc] = LightShadeLevel.VeryBright;
                                            break;
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                });
            }

        }
        #endregion

        #region public void RecacheShadingZones()
        /// <summary>
        /// Builds shading zones for all external or internal boundaries to local cell groups in the viewport
        /// </summary>
        public void RecacheShadingZones()
        {
            _Zones.Clear();
            var _viewCube = Map.BackgroundViewport;

            // get edges of backgrounds within viewport (insides of air, outsides of solids and gases)
            foreach (var _grp in from _g in Map.Backgrounds.All()
                                 where _g.IsOverlapping(_viewCube)
                                 select _g)
            {
                // NOTE: assumes backgrounds are uniform cellSpaces
                var _internal = (_grp.TemplateCell.CellSpace as ICellSpace).IsInvisible;
                var _offset = _internal ? 0 : 1;

                // ZLow zone
                if (_grp.Z >= _viewCube.Z)
                {
                    _Zones.Add(new ShadingZone(_internal ? _grp.ID : Guid.Empty, new Cubic(
                        _grp.Z - _offset, Math.Max(_grp.Y, _viewCube.Y), Math.Max(_grp.X, _viewCube.X),
                        _grp.Z - _offset, Math.Min(_grp.UpperY, _viewCube.UpperY), Math.Min(_grp.UpperX, _viewCube.UpperX)),
                        _internal ? AnchorFace.ZHigh : AnchorFace.ZLow));
                }
                // YLow zone
                if (_grp.Y >= _viewCube.Y)
                {
                    _Zones.Add(new ShadingZone(_internal ? _grp.ID : Guid.Empty, new Cubic(
                        Math.Max(_grp.Z, _viewCube.Z), _grp.Y - _offset, Math.Max(_grp.X, _viewCube.X),
                        Math.Min(_grp.UpperZ, _viewCube.UpperZ), _grp.Y - _offset, Math.Min(_grp.UpperX, _viewCube.UpperX)),
                        _internal ? AnchorFace.YHigh : AnchorFace.YLow));
                }
                // XLow zone
                if (_grp.X >= _viewCube.X)
                {
                    _Zones.Add(new ShadingZone(_internal ? _grp.ID : Guid.Empty, new Cubic(
                        Math.Max(_grp.Z, _viewCube.Z), Math.Max(_grp.Y, _viewCube.Y), _grp.X - _offset,
                        Math.Min(_grp.UpperZ, _viewCube.UpperZ), Math.Min(_grp.UpperY, _viewCube.UpperY), _grp.X - _offset),
                        _internal ? AnchorFace.XHigh : AnchorFace.XLow));
                }
                // ZHigh zone
                if (_grp.UpperZ <= _viewCube.UpperZ)
                {
                    _Zones.Add(new ShadingZone(_internal ? _grp.ID : Guid.Empty, new Cubic(
                        _grp.UpperZ + _offset, Math.Max(_grp.Y, _viewCube.Y), Math.Max(_grp.X, _viewCube.X),
                        _grp.UpperZ + _offset, Math.Min(_grp.UpperY, _viewCube.UpperY), Math.Min(_grp.UpperX, _viewCube.UpperX)),
                        _internal ? AnchorFace.ZLow : AnchorFace.ZHigh));
                }
                // YHigh zone
                if (_grp.UpperY <= _viewCube.UpperY)
                {
                    _Zones.Add(new ShadingZone(_internal ? _grp.ID : Guid.Empty, new Cubic(
                        Math.Max(_grp.Z, _viewCube.Z), _grp.UpperY + _offset, Math.Max(_grp.X, _viewCube.X),
                        Math.Min(_grp.UpperZ, _viewCube.UpperZ), _grp.UpperY + _offset, Math.Min(_grp.UpperX, _viewCube.UpperX)),
                        _internal ? AnchorFace.YLow : AnchorFace.YHigh));
                }
                // XHigh zone
                if (_grp.UpperX <= _viewCube.UpperX)
                {
                    _Zones.Add(new ShadingZone(_internal ? _grp.ID : Guid.Empty, new Cubic(
                        Math.Max(_grp.Z, _viewCube.Z), Math.Max(_grp.Y, _viewCube.Y), _grp.UpperX + _offset,
                        Math.Min(_grp.UpperZ, _viewCube.UpperZ), Math.Min(_grp.UpperY, _viewCube.UpperY), _grp.UpperX + _offset),
                        _internal ? AnchorFace.XLow : AnchorFace.XHigh));
                }
            }

            foreach (var _room in from _r in Map.Rooms
                                  where _r.IsOverlapping(_viewCube)
                                  select _r)
            {
                // ZLow zone
                if (_room.Z >= _viewCube.Z)
                {
                    _Zones.Add(new ShadingZone(Guid.Empty, new Cubic(
                        _room.Z - 1, Math.Max(_room.Y, _viewCube.Y), Math.Max(_room.X, _viewCube.X),
                        _room.Z - 1, Math.Min(_room.UpperY, _viewCube.UpperY), Math.Min(_room.UpperX, _viewCube.UpperX)),
                        AnchorFace.ZLow));
                }
                // YLow zone
                if (_room.Y >= _viewCube.Y)
                {
                    _Zones.Add(new ShadingZone(Guid.Empty, new Cubic(
                        Math.Max(_room.Z, _viewCube.Z), _room.Y - 1, Math.Max(_room.X, _viewCube.X),
                        Math.Min(_room.UpperZ, _viewCube.UpperZ), _room.Y - 1, Math.Min(_room.UpperX, _viewCube.UpperX)),
                        AnchorFace.YLow));
                }
                // XLow zone
                if (_room.X >= _viewCube.X)
                {
                    _Zones.Add(new ShadingZone(Guid.Empty, new Cubic(
                        Math.Max(_room.Z, _viewCube.Z), Math.Max(_room.Y, _viewCube.Y), _room.X - 1,
                        Math.Min(_room.UpperZ, _viewCube.UpperZ), Math.Min(_room.UpperY, _viewCube.UpperY), _room.X - 1),
                        AnchorFace.XLow));
                }
                // ZHigh zone
                if (_room.UpperZ <= _viewCube.UpperZ)
                {
                    _Zones.Add(new ShadingZone(Guid.Empty, new Cubic(
                        _room.UpperZ + 1, Math.Max(_room.Y, _viewCube.Y), Math.Max(_room.X, _viewCube.X),
                        _room.UpperZ + 1, Math.Min(_room.UpperY, _viewCube.UpperY), Math.Min(_room.UpperX, _viewCube.UpperX)),
                        AnchorFace.ZHigh));
                }
                // YHigh zone
                if (_room.UpperY <= _viewCube.UpperY)
                {
                    _Zones.Add(new ShadingZone(Guid.Empty, new Cubic(
                        Math.Max(_room.Z, _viewCube.Z), _room.UpperY + 1, Math.Max(_room.X, _viewCube.X),
                        Math.Min(_room.UpperZ, _viewCube.UpperZ), _room.UpperY + 1, Math.Min(_room.UpperX, _viewCube.UpperX)),
                        AnchorFace.YHigh));
                }
                // XHigh zone
                if (_room.UpperX <= _viewCube.UpperX)
                {
                    _Zones.Add(new ShadingZone(Guid.Empty, new Cubic(
                        Math.Max(_room.Z, _viewCube.Z), Math.Max(_room.Y, _viewCube.Y), _room.UpperX + 1,
                        Math.Min(_room.UpperZ, _viewCube.UpperZ), Math.Min(_room.UpperY, _viewCube.UpperY), _room.UpperX + 1),
                        AnchorFace.XHigh));
                }
            }
        }
        #endregion

        #region public IEnumerable<BuildableGroup> RenderShadingZones(ISensorHost sensorHost, bool alphaChannel)
        public IEnumerable<BuildableGroup> RenderShadingZones(ISensorHost sensorHost, bool alphaChannel)
        {
            var _loc = sensorHost.GetLocated();
            if ((_loc != null) && (sensorHost != null))
            {
                return RenderShadingZones(_loc.Locator.GeometricRegion, alphaChannel, sensorHost.GetTerrainVisualizer());
            }
            return null;
        }
        #endregion

        #region public IEnumerable<BuildableGroup> RenderShadingZones(IGeometricRegion location, bool alphaChannel, TerrainVisualizer visualizer)
        public IEnumerable<BuildableGroup> RenderShadingZones(IGeometricRegion location, bool alphaChannel, TerrainVisualizer visualizer)
        {
            foreach (var _shadeZone in YieldShadeZoneEffects(location, alphaChannel, visualizer))
            {
                var _localOpaque = new Model3DGroup();
                var _localAlpha = new Model3DGroup();
                var _globalContext = new BuildableContext();

                // loop over region
                var _z = 0;
                var _y = 0;
                var _x = 0;
                foreach (var _effect in _shadeZone.Effects)
                {
                    if (_effect != VisualEffect.Skip)
                    {
                        var _cLoc = new CellPosition(_shadeZone.Cube.Z + _z, _shadeZone.Cube.Y + _y, _shadeZone.Cube.X + _x);

                        // render faces
                        var _cellOpaque = new Model3DGroup();
                        var _cellAlpha = new Model3DGroup();
                        var _group = new BuildableGroup { Context = _globalContext, Opaque = _cellOpaque, Alpha = _cellAlpha };
                        switch (_shadeZone.Face.ToAnchorFace())
                        {
                            case AnchorFace.XLow:
                                _Map[_cLoc.Z, _cLoc.Y, _cLoc.X + 1, null].AddOuterSurface(_group, _cLoc.Z, _cLoc.Y, _cLoc.X + 1, AnchorFace.XLow, _effect, _XLift, null);
                                break;
                            case AnchorFace.XHigh:
                                _Map[_cLoc.Z, _cLoc.Y, _cLoc.X - 1, null].AddOuterSurface(_group, _cLoc.Z, _cLoc.Y, _cLoc.X - 1, AnchorFace.XHigh, _effect, _XDrop, null);
                                break;
                            case AnchorFace.YLow:
                                _Map[_cLoc.Z, _cLoc.Y + 1, _cLoc.X, null].AddOuterSurface(_group, _cLoc.Z, _cLoc.Y + 1, _cLoc.X, AnchorFace.YLow, _effect, _YLift, null);
                                break;
                            case AnchorFace.YHigh:
                                _Map[_cLoc.Z, _cLoc.Y - 1, _cLoc.X, null].AddOuterSurface(_group, _cLoc.Z, _cLoc.Y - 1, _cLoc.X, AnchorFace.YHigh, _effect, _YDrop, null);
                                break;
                            case AnchorFace.ZLow:
                                _Map[_cLoc.Z + 1, _cLoc.Y, _cLoc.X, null].AddOuterSurface(_group, _cLoc.Z + 1, _cLoc.Y, _cLoc.X, AnchorFace.ZLow, _effect, _ZLift, null);
                                break;
                            case AnchorFace.ZHigh:
                            default:
                                _Map[_cLoc.Z - 1, _cLoc.Y, _cLoc.X, null].AddOuterSurface(_group, _cLoc.Z - 1, _cLoc.Y, _cLoc.X, AnchorFace.ZHigh, _effect, _ZDrop, null);
                                break;
                        }
                        _Map[_cLoc].AddInnerStructures(_group, _cLoc.Z, _cLoc.Y, _cLoc.X, _effect);

                        // merge buildables
                        if (_cellOpaque.Children.Count > 0)
                        {
                            _cellOpaque.Transform = new TranslateTransform3D(_x * 5d, _y * 5d, _z * 5d);
                            _cellOpaque.Transform.Freeze();
                            _localOpaque.Children.Add(_cellOpaque);
                        }
                        if (_cellAlpha.Children.Count > 0)
                        {
                            _cellAlpha.Transform = new TranslateTransform3D(_x * 5d, _y * 5d, _z * 5d);
                            _cellAlpha.Freeze();
                            _localAlpha.Children.Add(_cellAlpha);
                        }
                    }

                    #region cell coordinates
                    // end of loop counter increments
                    _x++;
                    if (_x >= _shadeZone.Cube.XLength)
                    {
                        _x = 0;
                        _y++;
                        if (_y >= _shadeZone.Cube.YLength)
                        {
                            _y = 0;
                            _z++;

                            // NOTE: this is a failsafe, shouldn't have more cells than effects
                            if (_z >= _shadeZone.Cube.ZHeight)
                                break;
                        }
                    }
                    #endregion
                }

                // merge buildable context
                var _move = new TranslateTransform3D(_shadeZone.Cube.Vector3D());
                Model3DGroup _getFinal(bool alpha, Model3DGroup gathered)
                {
                    var _final = new Model3DGroup();
                    foreach (var _m in _globalContext.GetModel3D(alpha))
                        _final.Children.Add(_m);
                    if (gathered.Children.Count > 0)
                    {
                        gathered.Transform = _move;
                        _final.Children.Add(gathered);
                    }
                    if (_final.Children.Count > 0)
                    {
                        _final.Freeze();
                        return _final;
                    }
                    return null;
                };

                // return
                yield return new BuildableGroup
                {
                    Alpha = _getFinal(true, _localAlpha),
                    Opaque = _getFinal(false, _localOpaque)
                };
            }
            yield break;
        }
        #endregion

        #region public BuildableGroup RenderShadingZoneEffects(ShadeZoneEffects shadeZone, IGeometricRegion location, bool alphaChannel, TerrainVisualizer visualizer)
        public BuildableGroup RenderShadingZoneEffects(ShadeZoneEffects shadeZone, IGeometricRegion location, bool alphaChannel, TerrainVisualizer visualizer)
        {
            var _localOpaque = new Model3DGroup();
            var _localAlpha = new Model3DGroup();
            var _globalContext = new BuildableContext();

            // loop over region
            var _z = 0;
            var _y = 0;
            var _x = 0;
            foreach (var _effect in shadeZone.Effects)
            {
                if (_effect != VisualEffect.Skip)
                {
                    var _cLoc = new CellPosition(shadeZone.Cube.Z + _z, shadeZone.Cube.Y + _y, shadeZone.Cube.X + _x);

                    // render faces
                    var _cellOpaque = new Model3DGroup();
                    var _cellAlpha = new Model3DGroup();
                    var _group = new BuildableGroup { Context = _globalContext, Opaque = _cellOpaque, Alpha = _cellAlpha };
                    switch (shadeZone.Face.ToAnchorFace())
                    {
                        case AnchorFace.XLow:
                            _Map[_cLoc.Z, _cLoc.Y, _cLoc.X + 1, null].AddOuterSurface(_group, _cLoc.Z, _cLoc.Y, _cLoc.X + 1, AnchorFace.XLow, _effect, _XLift, null);
                            break;
                        case AnchorFace.XHigh:
                            _Map[_cLoc.Z, _cLoc.Y, _cLoc.X - 1, null].AddOuterSurface(_group, _cLoc.Z, _cLoc.Y, _cLoc.X - 1, AnchorFace.XHigh, _effect, _XDrop, null);
                            break;
                        case AnchorFace.YLow:
                            _Map[_cLoc.Z, _cLoc.Y + 1, _cLoc.X, null].AddOuterSurface(_group, _cLoc.Z, _cLoc.Y + 1, _cLoc.X, AnchorFace.YLow, _effect, _YLift, null);
                            break;
                        case AnchorFace.YHigh:
                            _Map[_cLoc.Z, _cLoc.Y - 1, _cLoc.X, null].AddOuterSurface(_group, _cLoc.Z, _cLoc.Y - 1, _cLoc.X, AnchorFace.YHigh, _effect, _YDrop, null);
                            break;
                        case AnchorFace.ZLow:
                            _Map[_cLoc.Z + 1, _cLoc.Y, _cLoc.X, null].AddOuterSurface(_group, _cLoc.Z + 1, _cLoc.Y, _cLoc.X, AnchorFace.ZLow, _effect, _ZLift, null);
                            break;
                        case AnchorFace.ZHigh:
                        default:
                            _Map[_cLoc.Z - 1, _cLoc.Y, _cLoc.X, null].AddOuterSurface(_group, _cLoc.Z - 1, _cLoc.Y, _cLoc.X, AnchorFace.ZHigh, _effect, _ZDrop, null);
                            break;
                    }
                    _Map[_cLoc].AddInnerStructures(_group, _cLoc.Z, _cLoc.Y, _cLoc.X, _effect);

                    // merge buildables
                    if (_cellOpaque.Children.Count > 0)
                    {
                        _cellOpaque.Transform = new TranslateTransform3D(_x * 5d, _y * 5d, _z * 5d);
                        _cellOpaque.Freeze();
                        _localOpaque.Children.Add(_cellOpaque);
                    }
                    if (_cellAlpha.Children.Count > 0)
                    {
                        _cellAlpha.Transform = new TranslateTransform3D(_x * 5d, _y * 5d, _z * 5d);
                        _cellAlpha.Freeze();
                        _localAlpha.Children.Add(_cellAlpha);
                    }
                }

                #region cell coordinates
                // end of loop counter increments
                _x++;
                if (_x >= shadeZone.Cube.XLength)
                {
                    _x = 0;
                    _y++;
                    if (_y >= shadeZone.Cube.YLength)
                    {
                        _y = 0;
                        _z++;

                        // NOTE: this is a failsafe, shouldn't have more cells than effects
                        if (_z >= shadeZone.Cube.ZHeight)
                            break;
                    }
                }
                #endregion
            }

            // merge buildable context
            var _move = new TranslateTransform3D(shadeZone.Cube.Vector3D());
            Model3DGroup _getFinal(bool alpha, Model3DGroup gathered)
            {
                var _final = new Model3DGroup();
                foreach (var _m in _globalContext.GetModel3D(alpha))
                    _final.Children.Add(_m);
                if (gathered.Children.Count > 0)
                {
                    gathered.Transform = _move;
                    _final.Children.Add(gathered);
                }
                if (_final.Children.Count > 0)
                {
                    _final.Freeze();
                    return _final;
                }
                return null;
            };

            // return
            return new BuildableGroup
            {
                Alpha = _getFinal(true, _localAlpha),
                Opaque = _getFinal(false, _localOpaque)
            };
        }
        #endregion

        #region public VisualEffect CellEffect(IGeometricRegion location, int z, int y, int x, TerrainVisualizer visualizer)
        public VisualEffect CellEffect(IGeometricRegion location, int z, int y, int x, TerrainVisualizer visualizer)
        {
            var _effect = VisualEffect.Unseen;
            var _cLoc = new CellLocation(z, y, x);
            var _fallback = location.ContainsCell(_cLoc) ? visualizer.HostLightLevel : LightRange.OutOfRange;

            // if the cell should be handled by shading zones, do so
            if (!IsSkipped(_cLoc))
            {
                var _distance = location.NearDistanceToCell((ICellLocation)_cLoc);

                #region determine visual effect
                // if sight is not usable, check in-range (and line of effect)
                if (visualizer.NoSight)
                {
                    #region blindsight only
                    // only terrain visualization senses with no sight requirements
                    var _sightless = visualizer.FilteredSenses
                        .Where(_s => _distance <= _s.Range)
                        .OrderByDescending(_s => _s.Range);
                    var _noTrans = _sightless.FirstOrDefault(_s => !_s.UsesSenseTransit);
                    var _useTrans = _sightless.FirstOrDefault(_s => _s.UsesSenseTransit);
                    if ((_useTrans != null)
                        && ((_noTrans == null) || (_noTrans.Range < _useTrans.Range))
                        && _useTrans.CarrySenseInteraction(Map, location, _cLoc, ITacticalInquiryHelper.EmptyArray))
                        _effect = VisualEffectProcessor.GetFormOnlyLevel(_distance, _useTrans.Range);
                    else if (_noTrans != null)
                        _effect = VisualEffectProcessor.GetFormOnlyLevel(_distance, _noTrans.Range);
                    #endregion
                }
                else
                {
                    // should be at least one terrain visualizing sense that uses sight...
                    if (IsMagicDark(_cLoc))
                    {
                        // default to 50%
                        _effect = VisualEffect.DimTo50;

                        #region any sense that ignores visual effects
                        // there is at least one sense that ignores visual effects
                        if (visualizer.IgnoreVisual)
                        {
                            // so see if any allow terrain visualization (may actually be 
                            foreach (var _magicDarkPiercing in visualizer.IgnoresVisualEffects
                                .Where(_s => _distance <= _s.Range))
                            {
                                if (!_magicDarkPiercing.UsesSenseTransit
                                    || _magicDarkPiercing.CarrySenseInteraction(Map, location, _cLoc, ITacticalInquiryHelper.EmptyArray))
                                {
                                    if (_magicDarkPiercing.UsesSight)
                                    {
                                        // obtained highest detail
                                        _effect = VisualEffect.Normal;
                                        break;
                                    }
                                    else
                                    {
                                        var _eff = VisualEffectProcessor.GetFormOnlyLevel(_distance, _magicDarkPiercing.Range);
                                        if (_eff > _effect)
                                            _effect = _eff;
                                    }
                                }
                            }
                        }
                        #endregion
                    }

                    // still need to probe for visualization effect?
                    // NOTE: since we've gotten past magic darkness, only concerned with light
                    if (_effect == VisualEffect.Unseen)
                    {
                        var _level = GetLightLevel(_cLoc, _fallback);
                        var _sight = visualizer.UsesSight.Where(_s => _s.Range >= _distance);
                        if (_sight.Any())
                        {
                            if (_sight.Any(_ir => _ir.IgnoresVisualEffects))
                            {
                                // sighted sense ignoring visual effects doesn't care about light levels
                                _effect = VisualEffect.Normal;
                            }
                            else if (_level >= LightRange.Bright)
                            {
                                #region brightly lit
                                if (_sight.Any(_ir => _ir.UsesLight))
                                {
                                    if (_level >= LightRange.VeryBright)
                                        _effect = VisualEffect.Brighter;
                                    else
                                        _effect = VisualEffect.Normal;
                                }
                                else
                                    // dark vision (and true seeing...)
                                    _effect = VisualEffectProcessor.GetMonochromeLevel(_distance, _sight.Where(_ir => !_ir.UsesLight).Max(_ir => _ir.Range));
                                #endregion
                            }
                            else if (_level >= LightRange.NearShadow)
                            {
                                #region regular shadowy
                                if (_sight.Any(_ir => _ir.UsesLight && _ir.LowLight))
                                {
                                    // low light vision and true seeing
                                    _effect = VisualEffect.Normal;
                                }
                                else if (_sight.Any(_ir => !_ir.UsesLight))
                                {
                                    if (_sight.Any(_ir => _ir.UsesLight))
                                    {
                                        // dark vision (but sensor also uses normal vision)
                                        _effect = VisualEffect.Normal;
                                    }
                                    else
                                    {
                                        // dark vision only
                                        _effect = VisualEffectProcessor.GetMonochromeLevel(_distance, _sight.Where(_ir => !_ir.UsesLight).Max(_ir => _ir.Range));
                                    }
                                }
                                else
                                {
                                    if (_level == LightRange.NearBoost)
                                        _effect = VisualEffect.DimTo75;
                                    else
                                        _effect = VisualEffect.DimTo50;
                                }
                                #endregion
                            }
                            else if (_sight.Any(_ir => !_ir.UsesLight))
                            {
                                _effect = VisualEffectProcessor.GetMonochromeLevel(_distance, _sight.Where(_ir => !_ir.UsesLight).Max(_ir => _ir.Range));
                            }
                            else if (_level >= LightRange.FarShadow)
                            {
                                if (_sight.Any(_ir => _ir.UsesLight && _ir.LowLight))
                                {
                                    // low light vision is all that's left if we haven't found darkvision
                                    if (_level == LightRange.FarBoost)
                                        _effect = VisualEffect.DimTo75;
                                    else
                                        _effect = VisualEffect.DimTo50;
                                }
                                else
                                {
                                    // far shade but on fringe for near shade
                                    if (_level == LightRange.FarBoost)
                                        _effect = VisualEffect.DimTo25;
                                }
                            }
                            else
                            {
                                // beyond far shade
                                if ((_level == LightRange.ExtentBoost)
                                    && _sight.Any(_ir => _ir.UsesLight && _ir.LowLight))
                                    _effect = VisualEffect.DimTo25;
                            }
                        }

                        // if not normal or monochrome, allow non-sighted sense a chance to affect visualization
                        if ((_effect != VisualEffect.Normal)
                            && (_effect != VisualEffect.Brighter)
                            && (!_effect.IsMonochrome()))
                        {
                            // so see if any allow terrain visualization 
                            foreach (var _formSense in visualizer.NotUsesSight
                                .Where(_s => _distance <= _s.Range))
                            {
                                if (!_formSense.UsesSenseTransit
                                    || _formSense.CarrySenseInteraction(Map, location, _cLoc, ITacticalInquiryHelper.EmptyArray))
                                {
                                    var _eff = VisualEffectProcessor.GetFormOnlyLevel(_distance, _formSense.Range);
                                    if (_eff > _effect)
                                        _effect = _eff;
                                }
                            }
                        }
                    }
                }
                #endregion

                return _effect;
            }
            else
            {
                // indication not to draw anything
                return VisualEffect.Skip;
            }
        }
        #endregion

        #region private IEnumerable<VisualEffect> YieldEffects(ShadingZone zone, IGeometricRegion location, bool alphaChannel, TerrainVisualizer visualizer)
        private IEnumerable<VisualEffect> YieldEffects(ShadingZone zone, IGeometricRegion location,
            bool alphaChannel, TerrainVisualizer visualizer)
        {
            for (var _z = 0; _z < zone.Cube.ZHeight; _z++)
                for (var _y = 0; _y < zone.Cube.YLength; _y++)
                    for (var _x = 0; _x < zone.Cube.XLength; _x++)
                    {
                        yield return CellEffect(location, zone.Cube.Z + _z, zone.Cube.Y + _y, zone.Cube.X + _x, visualizer);
                    }

            // done yielding
            yield break;
        }
        #endregion

        #region public IEnumerable<ShadeZoneEffects> YieldShadeZoneEffects(IGeometricRegion location, bool alphaChannel, TerrainVisualizer visualizer)
        public IEnumerable<ShadeZoneEffects> YieldShadeZoneEffects(IGeometricRegion location, bool alphaChannel, TerrainVisualizer visualizer)
        {
            // if no active senses perform terrain visualization, no point in generating models
            if (visualizer.FilteredSenses.Count != 0)
            {
                // step through zones
                foreach (var _zone in _Zones)
                {
                    // TODO: panelShadings for localLinks containing panel producing portals
                    yield return new ShadeZoneEffects(_zone.ID, _zone.Cube, _zone.Face.ToOptionalAnchorFace(),
                        YieldEffects(_zone, location, alphaChannel, visualizer), null);
                }
            }
            yield break;
        }
        #endregion

        #region IEnumerable<ShadingZone> Members

        public IEnumerator<ShadingZone> GetEnumerator()
        {
            return _Zones.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _Zones.GetEnumerator();
        }

        #endregion
    }
}
