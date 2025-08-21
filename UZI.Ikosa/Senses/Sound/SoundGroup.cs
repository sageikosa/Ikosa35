using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class SoundGroup : AdjunctGroup, ISerializable
    {
        public SoundGroup(object source, SoundRef soundRef)
            : base(source, soundRef.Audible.SoundGroupID)
        {
            _Channels = new ConcurrentDictionary<(LocalLink link, LocalCellGroup project), SoundChannel>();
            _SoundRef = soundRef;
            _LastGroups = [];
            _Presence = PlanarPresence.None;
        }

        protected SoundGroup(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _Channels = new ConcurrentDictionary<(LocalLink link, LocalCellGroup project), SoundChannel>(
                (Dictionary<(LocalLink link, LocalCellGroup project), SoundChannel>)info.GetValue(nameof(_Channels), typeof(Dictionary<(LocalLink link, LocalCellGroup project), SoundChannel>)));
            _SoundRef = (SoundRef)info.GetValue(nameof(_SoundRef), typeof(SoundRef));
            _LastGroups = (List<LocalCellGroup>)info.GetValue(nameof(_LastGroups), typeof(List<LocalCellGroup>));
            _Presence = (PlanarPresence)info.GetValue(nameof(_Presence), typeof(PlanarPresence));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(_Channels), _Channels.ToDictionary(_x => _x.Key, _x => _x.Value));
            info.AddValue(nameof(_SoundRef), _SoundRef);
            info.AddValue(nameof(_LastGroups), _LastGroups);
            info.AddValue(nameof(_Presence), _Presence);
        }

        #region state
        private ConcurrentDictionary<(LocalLink link, LocalCellGroup project), SoundChannel> _Channels;
        private SoundRef _SoundRef;
        private List<LocalCellGroup> _LastGroups;
        private PlanarPresence _Presence;
        #endregion

        public IEnumerable<SoundParticipant> SoundParticipants
            => Members.OfType<SoundParticipant>();

        /// <summary>Provide local cell groups for participants, or last groups if no participants</summary>
        public IEnumerable<LocalCellGroup> GetLocalCellGroups()
            => !SoundParticipants.Any()
            ? _LastGroups
            : SoundParticipants.SelectMany(_sp => _sp.GetLocalCellGroups()).Distinct();

        public SoundRef SoundPresence => _SoundRef;

        #region private (ICellLocation cell, double range, int distanceDiff, DeltaCalcInfo difficulty, int extra, bool isEffect) TransitSoundParameters(LocalCellGroup targetGroup, LocalLink targetLink)
        private (ICellLocation cell, double range, int distanceDiff, DeltaCalcInfo difficulty, int extra, bool isEffect) TransitSoundParameters(LocalCellGroup targetGroup, LocalLink targetLink)
        {
            // going to look through geometry of each participant
            ICore _audible = null;
            var _tCell = targetLink.InteractionCell(targetGroup);
            var _geometries = from _sp in SoundParticipants
                              let _g = _sp.GetGeometricRegion()
                              where _g != null
                              let _d = _g.NearDistance(targetLink.GeometricRegion)
                              let _m = _sp.Anchor.Setting as LocalMap
                              where _m != null
                              orderby _d ascending
                              select (distance: _d, geom: _g, map: _m);
            foreach (var (_distance, _geom, _map) in _geometries)
            {
                // from geometry to _link...
                _audible ??= _map.GetICore<ICore>(SoundPresence.Audible.SourceID);
                var _distanceDiff = (int)Math.Ceiling(_distance / 10);
                var _range = Math.Max(SoundPresence.RangeRemaining - _distance, 0);
                if (_range > 0)
                {
                    // just going to test one line per participant geometry
                    var _sCell = (from _c in _geom.AllCellLocations()
                                  let _d = IGeometricHelper.Distance(_tCell, _c)
                                  orderby _d ascending
                                  select new CellLocation(_c)).FirstOrDefault();
                    if (_sCell != null)
                    {
                        // this is to target link, so can only work in material presence
                        var _factory = new SegmentSetFactory(targetLink.MapContext.Map, _sCell, _tCell,
                                ITacticalInquiryHelper.EmptyArray, SegmentSetProcess.Geometry);
                        var _line = _map.SegmentCells(_sCell.GetPoint3D(), _tCell.GetPoint3D(), _factory, PlanarPresence.Material);

                        // new range check
                        _range -= Math.Max(_line.IsLineOfEffect ? 0 : 10, 0);
                        if (_range > 0)
                        {
                            var _soundTrans = new SoundTransit(SoundPresence.Audible);
                            var _interact = new Interaction(null, _audible, null, _soundTrans);
                            if (_line.CarryInteraction(_interact))
                            {
                                // line was successful, so we go with this one
                                return (_sCell, _range, _distanceDiff, SoundPresence.BaseDifficulty,
                                    _soundTrans.AddedDifficulty.EffectiveValue, _line.IsLineOfEffect);
                            }
                        }
                    }
                }
            }

            // nothing transitted to link
            return (null, 0, 0, new DeltaCalcInfo(Guid.Empty, string.Empty), 0, false);
        }
        #endregion

        #region public void SetSoundRef(SoundRef soundRef)
        public void SetSoundRef(SoundRef soundRef, ConcurrentDictionary<Guid, LocalCellGroup> sharedGroups = null)
        {
            // update all groups currently within
            var _updateGroups = new ConcurrentDictionary<Guid, LocalCellGroup>(GetLocalCellGroups().ToDictionary(_lcg => _lcg.ID));

            // seed last groups for when there are no participants
            _LastGroups = new List<LocalCellGroup>(_updateGroups.Select(_g => _g.Value));

            // track audibles to process, sound may change so we may end up with 2 distinct IDs
            var _id = SoundPresence.Audible.SoundGroupID;

            // current serial state for change testing
            var _serial = soundRef.SerialState;

            // channels projecting into on each sweep
            var _gatherChannels = new ConcurrentDictionary<(LocalLink link, LocalCellGroup project), SoundChannel>();

            // channels no longer in direct groups must be cancelled
            Parallel.ForEach(_Channels.Values.Where(_c => !_updateGroups.ContainsKey(_c.SourceGroup.ID)).ToList(),
                _channel =>
                {
                    //Debug.WriteLine($@"SoundGroup.SetSoundRef: _gatherChannel; left SourceGroup [{_channel.SourceGroup.Name}]");
                    _channel.ReceiveChannel(this, new SoundRef(SoundPresence.Audible, _serial));
                    _gatherChannels.TryAdd(_channel.ChannelFlow, _channel);
                });

            // cancel any existing direct links that connect directly to current groups
            Parallel.ForEach(_Channels.Values.Where(_c => _updateGroups.ContainsKey(_c.ChannelFlow.project.ID)).ToList(),
                _channel =>
                {
                    //Debug.WriteLine($@"SoundGroup.SetSoundRef: _gatherChannel; entered project-into [{_channel.ChannelFlow.project.Name}]");
                    _channel.ReceiveChannel(this, new SoundRef(SoundPresence.Audible, _serial));
                    _gatherChannels.TryAdd(_channel.ChannelFlow, _channel);
                });

            // really setting sound (SoundPresence is now getting pushed)
            _SoundRef = soundRef;

            // get links in our groups that link to outside the group
            var _links = (from _g in _updateGroups.Values
                          from _l in _g.Links.All
                          let _outSide = _l.NextGroup(_g)
                          where !_updateGroups.ContainsKey(_outSide.ID)
                          select (Link: _l, Project: _outSide)).ToList();

            // receive initial
            Parallel.ForEach(_links,
                _flow =>
                {
                    var (_cell, _range, _distanceDiff, _difficulty, _extra, _isEffect) =
                        TransitSoundParameters(_flow.Project, _flow.Link);
                    if (_range > 0)
                    {
                        // find or create link
                        var _channel = _Channels.GetOrAdd(_flow,
                            (addLink) => addLink.link.ChannelsTo(_flow.Project).GetOrAdd(ID, id => new SoundChannel(this, addLink)));

                        // project sound
                        if (_channel.ReceiveChannel(this,
                            new SoundRef(SoundPresence, _difficulty, _range,
                                _distanceDiff, (_isEffect ? 0 : 1), 1, _extra, _serial, _cell)))
                        {
                            _gatherChannels.TryAdd(_channel.ChannelFlow, _channel);
                        }
                    }
                    else if (_range == 0)
                    {
                        // if defined, cancel and remove
                        if (_flow.Link.ChannelsTo(_flow.Project).TryGetValue(ID, out var _soundChannel))
                        {
                            // project sound (even if it is zero ranged, thus cancellation)
                            if (_soundChannel.ReceiveChannel(this, new SoundRef(SoundPresence.Audible, _serial)))
                            {
                                _gatherChannels.TryAdd(_soundChannel.ChannelFlow, _soundChannel);
                            }
                        }
                    }
                });

            // channel sweep
            var _sx = 0;
            while (_gatherChannels.Any())
            {
                _sx++;

                // sweep and gather
                var _sweep = _gatherChannels.Select(_nc => _nc.Value).ToList();
                _gatherChannels.Clear();

                // transform all channels that received sound changes
                Parallel.ForEach(_sweep,
                    _channel =>
                    {
                        _channel.TransformChannel(_serial);
                    });

                // project the set just received and gather new edge groups
                Parallel.ForEach(_sweep,
                    _channel =>
                    {
                        _channel.ProjectChannel(_gatherChannels);
                        _updateGroups.TryAdd(_channel.ChannelFlow.project.ID, _channel.ChannelFlow.project);
                    });
            }

            // cleanup channels
            Parallel.ForEach(_Channels.Values.ToList(), _channel =>
            {
                _channel.Cleanup(0);
            });

            if (sharedGroups == null)
            {
                //// update listeners in groups...
                //foreach (var _u in _updateGroups)
                //{
                //    Debug.WriteLine($@"SoundGroup.SetSoundRef: _updategroups --> [{_u.Value.Name}]");
                //}
                //Debug.WriteLine($@"SoundGroup.SetSoundRef: _ids = {_id}");
                _updateGroups.UpdateListenersInGroups((id) => _id == id);
            }
            else
            {
                // merge into a single shared listener update
                foreach (var _ug in _updateGroups)
                {
                    sharedGroups.TryAdd(_ug.Key, _ug.Value);
                }
            }
        }
        #endregion

        protected override void OnMemberAdded(GroupMemberAdjunct member)
        {
            base.OnMemberAdded(member);
            if (Members.Count() == 1)
            {
                _Presence = member.Anchor.GetPlanarPresence();
            }
        }

        public override void OnRemoved()
        {
            if (_Channels.Any())
            {
                var _ctx = _Channels.FirstOrDefault().Key.link.MapContext;
                _ctx.SerialState++;
                SetSoundRef(new SoundRef(SoundPresence.Audible, _ctx.SerialState));
            }
            base.OnRemoved();
        }

        public override void ValidateGroup()
        {
            var _planar = _Presence;
            if (_planar != PlanarPresence.None)
            {
                foreach (var _member in Members.OfType<SoundParticipant>()
                    .Where(_sp => !_sp.Anchor.GetPlanarPresence().HasOverlappingPresence(_planar))
                    .ToList())
                {
                    // get rid of members not with same planar presence
                    _member.Eject();
                }

                if (Members.Count() <= 1)
                {
                    // just the master or less left
                    EjectMembers();
                }
            }
            else
            {
                EjectMembers();
            }
        }
    }
}
