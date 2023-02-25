using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Uzi.Core;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using Uzi.Visualize;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    // TODO: populate sound reference collection
    public class LocalLink : ILightTarget, ISerializable
    {
        #region construction
        public LocalLink(AnchorFace anchorFaceA, LocalCellGroup groupA, LocalCellGroup groupB, ICellLocation locationA, ICellLocation locationB)
        {
            _AnchorFaceA = anchorFaceA;
            if (groupA != null)
                _GroupA = groupA;
            if (groupB != null)
                _GroupB = groupB;

            _Cubic = anchorFaceA switch
            {
                AnchorFace.XHigh => new Cubic(locationA, 1, 1, 2),
                AnchorFace.XLow => new Cubic(locationB, 1, 1, 2),
                AnchorFace.YHigh => new Cubic(locationA, 1, 2, 1),
                AnchorFace.YLow => new Cubic(locationB, 1, 2, 1),
                AnchorFace.ZHigh => new Cubic(locationA, 2, 1, 1),
                _ => new Cubic(locationB, 2, 1, 1),
            };
            _LightA = new LocalLinkLight(this, true);
            _LightB = new LocalLinkLight(this, false);
            _LightFactor = 1;
            _Holder = new HolderObject();

            // sounds
            _ChannelsA = new ConcurrentDictionary<Guid, SoundChannel>();
            _ChannelsB = new ConcurrentDictionary<Guid, SoundChannel>();
        }

        protected LocalLink(SerializationInfo info, StreamingContext context)
        {
            _Cubic = (Cubic)info.GetValue(nameof(_Cubic), typeof(Cubic));
            _GroupA = (LocalCellGroup)info.GetValue(nameof(_GroupA), typeof(LocalCellGroup));
            _GroupB = (LocalCellGroup)info.GetValue(nameof(_GroupB), typeof(LocalCellGroup));
            _AnchorFaceA = (AnchorFace)info.GetValue(nameof(_AnchorFaceA), typeof(AnchorFace));
            _Holder = (HolderObject)info.GetValue(nameof(_Holder), typeof(HolderObject));

            ConcurrentDictionary<Guid, SoundChannel> _fetch(string key)
            {
                var _deserialized = (Dictionary<Guid, SoundChannel>)info.GetValue(key, typeof(Dictionary<Guid, SoundChannel>));
                return new ConcurrentDictionary<Guid, SoundChannel>(_deserialized);
            }
            foreach (var _p in info)
            {
                if (_p.Name.Equals(@"_SoundChannelsA"))
                {
                    _ChannelsA = _fetch(@"_SoundChannelsA");
                }
                if (_p.Name.Equals(@"_SoundChannelsB"))
                {
                    _ChannelsB = _fetch(@"_SoundChannelsB");
                }
                if ((_ChannelsA != null) && (_ChannelsB != null))
                    break;
            }
            _ChannelsA ??= new();
            _ChannelsB ??= new();

            _LightFactor = info.GetDouble(nameof(_LightFactor));
            _SoundDifficulty = info.GetInt32(nameof(_SoundDifficulty));
            _LightA = (LocalLinkLight)info.GetValue(nameof(_LightA), typeof(LocalLinkLight));
            _LightB = (LocalLinkLight)info.GetValue(nameof(_LightB), typeof(LocalLinkLight));
        }
        #endregion

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(_Cubic), _Cubic);
            info.AddValue(nameof(_GroupA), _GroupA);
            info.AddValue(nameof(_GroupB), _GroupB);
            info.AddValue(nameof(_AnchorFaceA), _AnchorFaceA);
            info.AddValue(nameof(_Holder), _Holder);
            info.AddValue(@"_SoundChannelsA", _ChannelsA.ToDictionary(_x => _x.Key, _x => _x.Value));
            info.AddValue(@"_SoundChannelsB", _ChannelsB.ToDictionary(_x => _x.Key, _x => _x.Value));
            info.AddValue(nameof(_LightFactor), _LightFactor);
            info.AddValue(nameof(_SoundDifficulty), _SoundDifficulty);
            info.AddValue(nameof(_LightA), _LightA);
            info.AddValue(nameof(_LightB), _LightB);
        }

        #region state
        private Cubic _Cubic;
        private LocalCellGroup _GroupA;
        private LocalCellGroup _GroupB;
        private AnchorFace _AnchorFaceA;
        private HolderObject _Holder;
        private ConcurrentDictionary<Guid, SoundChannel> _ChannelsA;
        private ConcurrentDictionary<Guid, SoundChannel> _ChannelsB;
        private double _LightFactor;
        private int _SoundDifficulty = 0;
        private LocalLinkLight _LightA;
        private LocalLinkLight _LightB;
        #endregion

        /// <summary>Cubic volume of the link (in both local cell groups)</summary>
        public Cubic LinkCube => _Cubic;
        public LocalCellGroup GroupA => _GroupA;
        public LocalCellGroup GroupB => _GroupB;
        public LocalLinkLight LightA => _LightA;
        public LocalLinkLight LightB => _LightB;
        public double AllowLightFactor => _LightFactor;
        public int ExtraSoundDifficulty => _SoundDifficulty;
        public AnchorFace AnchorFaceInA => _AnchorFaceA;
        public ConcurrentDictionary<Guid, SoundChannel> SoundChannelsToA => _ChannelsA;
        public ConcurrentDictionary<Guid, SoundChannel> SoundChannelsToB => _ChannelsB;
        public CoreObject Holder => _Holder;

        /// <summary>Returns the interaction cell for the other group in the link, given a specific group</summary>
        public CellLocation OutsideInteractionCell(LocalCellGroup group)
            => group == GroupA ? InteractionCell(GroupB) : InteractionCell(GroupA);

        /// <summary>Returns the other group, given a specific group</summary>
        public LocalCellGroup OutsideGroup(LocalCellGroup group)
            => Groups.FirstOrDefault(_g => _g != group);

        /// <summary>Channels to the specified group</summary>
        public ConcurrentDictionary<Guid, SoundChannel> ChannelsTo(LocalCellGroup group)
            => NextGroup(group) == GroupA
            ? _ChannelsA
            : _ChannelsB;

        public AnchorFace GetAnchorFaceInGroup(LocalCellGroup group)
            => group == GroupA
            ? AnchorFaceInA
            : AnchorFaceInA.ReverseFace();

        public AnchorFace GetAnchorFaceForRegion(IGeometricRegion region)
            => GroupA.ContainsGeometricRegion(region)
            ? AnchorFaceInA
            : AnchorFaceInA.ReverseFace();

        /// <summary>
        /// Returns the next group, given a specific group.  
        /// Accounts for background making links from different groups
        /// </summary>
        public LocalCellGroup NextGroup(LocalCellGroup group)
             => group.IsPartOfBackground
             ? Groups.FirstOrDefault(_g => !_g.IsPartOfBackground)
             : Groups.FirstOrDefault(_g => _g.IsPartOfBackground) ?? Groups.FirstOrDefault(_g => _g != group);

        public string Name
            => $@"{GroupA.Name}|{GroupB.Name}";

        public ShadowModel ShadowModel
            => GroupA.DeepShadows && GroupB.DeepShadows ? ShadowModel.Deep
            : GroupA.DeepShadows || GroupB.DeepShadows ? ShadowModel.Mixed
            : ShadowModel.Normal;

        public PlanarPresence PlanarPresence => PlanarPresence.Material;

        /// <summary>Yields both groups for the link</summary>
        public IEnumerable<LocalCellGroup> Groups { get { yield return _GroupA; yield return _GroupB; yield break; } }

        #region public IEnumerable<IIllumination> GetLights(LocalCellGroup target, bool linking)
        public IEnumerable<IIllumination> GetLights(LocalCellGroup target, bool linking)
        {
            if (target == GroupA)
            {
                if (LightA != null)
                {
                    yield return LightA;
                    if (!linking)
                    {
                        foreach (var _rl in LightA.ReferencedLights)
                            yield return _rl;
                    }
                }
            }
            else if (target == GroupB)
            {
                if (LightB != null)
                {
                    yield return LightB;
                    if (!linking)
                    {
                        foreach (var _rl in LightB.ReferencedLights)
                            yield return _rl;
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public void RecalculateSoundDifficulty()
        public void RecalculateSoundDifficulty()
        {
            var _extra = 0;
            foreach (var _linkAlterer in from _loc in MapContext.LocatorsInRegion(LinkCube, PlanarPresence.Material)
                                         from _obj in _loc.AllConnectedOf<IAlterLocalLink>()
                                         let _alter = _obj as IAlterLocalLink
                                         where _alter.CanAlterLink(this)
                                         select _alter)
            {
                _extra += _linkAlterer.GetExtraSoundDifficulty(this);
            }
            _SoundDifficulty = _extra;
        }
        #endregion

        #region public void RecalculateLightFactor()
        public void RecalculateLightFactor()
        {
            double _factor = 1;
            foreach (var _linkAlterer in from _loc in MapContext.LocatorsInRegion(LinkCube, PlanarPresence.Material)
                                         from _obj in _loc.AllConnectedOf<IAlterLocalLink>()
                                         let _alter = _obj as IAlterLocalLink
                                         where _alter.CanAlterLink(this)
                                         select _alter)
            {
                _factor *= _linkAlterer.AllowLightFactor(this);
            }
            _LightFactor = _factor;
        }
        #endregion

        #region public LocalCellGroup NotifyLight(LocalCellGroup homeGroup, IEnumerable<IIllumination> sourceLights)
        /// <summary>Updates links lighting to a connected cell group and returns the group if it needs to be notified</summary>
        public LocalCellGroup NotifyLight(LocalCellGroup homeGroup, IEnumerable<IIllumination> sourceLights)
        {
            RecalculateLightFactor();
            if (homeGroup == GroupA)
            {
                if (LightB.NotifyLights(sourceLights))
                    return GroupB;
            }
            else if (homeGroup == GroupB)
            {
                if (LightA.NotifyLights(sourceLights))
                    return GroupA;
            }
            return null;
        }
        #endregion

        #region ICore Members

        public Guid ID => Guid.Empty;
        public CoreSetting CurrentSetting { get => Groups.FirstOrDefault().Map; set {/* NOP */ } }

        #endregion

        #region public CellLocation InteractionCell(LocalCellGroup group)
        /// <summary>Cell location for interaction within the specified group</summary>
        public CellLocation InteractionCell(LocalCellGroup group)
        {
            if (group == GroupB)
            {
                return AnchorFaceInA switch
                {
                    AnchorFace.ZHigh => new CellLocation(LinkCube.Z + 1, LinkCube.Y, LinkCube.X),
                    AnchorFace.YHigh => new CellLocation(LinkCube.Z, LinkCube.Y + 1, LinkCube.X),
                    AnchorFace.XHigh => new CellLocation(LinkCube.Z, LinkCube.Y, LinkCube.X + 1),
                    _ => new CellLocation(LinkCube),
                };
            }
            else // GroupA
            {
                return AnchorFaceInA switch
                {
                    AnchorFace.ZLow => new CellLocation(LinkCube.Z + 1, LinkCube.Y, LinkCube.X),
                    AnchorFace.YLow => new CellLocation(LinkCube.Z, LinkCube.Y + 1, LinkCube.X),
                    AnchorFace.XLow => new CellLocation(LinkCube.Z, LinkCube.Y, LinkCube.X + 1),
                    _ => new CellLocation(LinkCube),
                };
            }
        }
        #endregion

        #region IGeometricContext Members

        public MapContext MapContext => _GroupA.Map.MapContext;
        public IGeometricRegion GeometricRegion => LinkCube;

        public IEnumerable<LocalCellGroup> GetLocalCellGroups()
        {
            yield return GroupA;
            yield return GroupB;
            yield break;
        }

        #endregion
    }
}
