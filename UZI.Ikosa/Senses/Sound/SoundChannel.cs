using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Senses
{
    /// <summary>
    /// Used to propagate sound through a LocalLink
    /// </summary>
    [Serializable]
    public class SoundChannel : ISerializable
    {
        public SoundChannel(SoundGroup group, (LocalLink link, LocalCellGroup project) channelFlow)
        {
            _Group = group;
            _Flow = channelFlow;
            _Inbound = new ConcurrentDictionary<object, SoundRef>();
            _Next = new ConcurrentDictionary<LocalLink, SoundChannel>();
        }

        protected SoundChannel(SerializationInfo info, StreamingContext context)
        {
            _Group = (SoundGroup)info.GetValue(nameof(_Group), typeof(SoundGroup));
            _Flow = ((LocalLink link, LocalCellGroup project))info.GetValue(nameof(_Flow), typeof((LocalLink link, LocalCellGroup project)));
            _Outbound = (SoundRef)info.GetValue(nameof(_Outbound), typeof(SoundRef));
            _Inbound = new ConcurrentDictionary<object, SoundRef>((Dictionary<object, SoundRef>)info.GetValue(nameof(_Inbound), typeof(Dictionary<object, SoundRef>)));
            _Next = new ConcurrentDictionary<LocalLink, SoundChannel>((Dictionary<LocalLink, SoundChannel>)info.GetValue(nameof(_Next), typeof(Dictionary<LocalLink, SoundChannel>)));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(_Group), _Group);
            info.AddValue(nameof(_Flow), _Flow);
            info.AddValue(nameof(_Inbound), _Inbound.ToDictionary(_x => _x.Key, _x => _x.Value));
            info.AddValue(nameof(_Outbound), _Outbound);
            info.AddValue(nameof(_Next), _Next.ToDictionary(_x => _x.Key, _x => _x.Value));
        }

        #region state
        private readonly SoundGroup _Group;
        private readonly (LocalLink link, LocalCellGroup project) _Flow;
        private ConcurrentDictionary<object, SoundRef> _Inbound;
        private SoundRef _Outbound;
        private ConcurrentDictionary<LocalLink, SoundChannel> _Next;
        #endregion

        public SoundGroup SoundGroup => _Group;

        /// <summary>LocalLink and projection group on which this channel is defined</summary>
        public (LocalLink link, LocalCellGroup project) ChannelFlow => _Flow;

        /// <summary>Source group the sound projects from</summary>
        public LocalCellGroup SourceGroup => _Flow.link.NextGroup(_Flow.project);

        public SoundRef SoundPresence => _Outbound;
        public ConcurrentDictionary<object, SoundRef> ReceivedSounds => _Inbound;

        #region public void ReceiveChannel(object source, SoundRef sound)
        /// <summary>Channel receives sound.  If sound added, return true.</summary>
        public bool ReceiveChannel(object source, SoundRef sound)
        {
            if (sound != null)
            {
                if (_Inbound.AddOrUpdate(source,
                    (key) => sound,
                    (key, current) =>
                        // new sound is from a later map state AND different
                        (((sound.SerialState > current.SerialState)
                            && ((sound.RangeRemaining != current.RangeRemaining) || (sound.TotalDifficulty != current.TotalDifficulty)))
                        // OR new sound is from same map state, but is stronger
                        || ((sound.SerialState == current.SerialState) && (sound.RangeRemaining > current.RangeRemaining)))
                        ? sound
                        : current) == sound)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region public void TransformChannel(ulong serialState)
        public void TransformChannel(ulong serialState)
        {
            var _out = _Outbound;

            // transform
            var _extra = ChannelFlow.link.ExtraSoundDifficulty;
            var _best = _Inbound.Values.OrderByDescending(_i => _i.RangeRemaining).FirstOrDefault();
            if (_best != null)
            {
                // most range remaining
                var _range = Math.Max(_best.RangeRemaining - (_extra * 10), 0);
                _Outbound = new SoundRef(_best, _best.BaseDifficulty, _range, 0, 0, 0,
                    _extra, serialState, _best.Cell);
            }
            else
            {
                // nothing?
                _Outbound = new SoundRef(_Outbound?.Audible, serialState);
            }
        }
        #endregion

        #region public void ProjectChannel(ConcurrentDictionary<(LocalLink link, LocalCellGroup project), SoundChannel> gatherChannels)
        public void ProjectChannel(ConcurrentDictionary<(LocalLink link, LocalCellGroup project), SoundChannel> gatherChannels)
        {
            var _presence = SoundPresence;

            // outbound probably has enough range to get to other nearby links
            var _sourceGroup = SourceGroup;
            var _outOf = ChannelFlow.project;
            var _targets = _outOf.Links.All.Where(_l => _l.NextGroup(_outOf) != _sourceGroup).ToList();

            Parallel.ForEach(
                // do not project back into source group directly
                _targets,
                _targetLink =>
                {
                    var _into = _targetLink.NextGroup(_outOf);
                    var (_cell, _range, _distanceDiff, _difficulty, _extra, _roomTrans, _isEffect)
                        = TransitSoundParameters(_outOf, _targetLink);
                    if (_range > 0)
                    {
                        // need a link even if there isn't one
                        var _channel = _Next.GetOrAdd(_targetLink,
                            (addLink) => addLink.ChannelsTo(_into).GetOrAdd(SoundGroup.ID,
                                id => new SoundChannel(SoundGroup, (addLink, _into))));

                        // project sound
                        if (_channel.ReceiveChannel(this,
                            new SoundRef(_presence, _difficulty, _range,
                                _distanceDiff, (_isEffect ? 0 : 1), _roomTrans, _extra,
                                _presence.SerialState, _cell)))
                        {
                            gatherChannels.TryAdd((_targetLink, _into), _channel);
                        }
                    }
                    else if (_range == 0)
                    {
                        // don't need a link, but if there is one, it'll be zeroed out
                        if (_targetLink.ChannelsTo(_into).TryGetValue(SoundGroup.ID, out var _soundChannel))
                        {
                            // project sound (even if it is zero ranged, thus cancellation)
                            if (_soundChannel.ReceiveChannel(this, new SoundRef(_presence.Audible, _presence.SerialState)))
                            {
                                gatherChannels.TryAdd((_targetLink, _into), _soundChannel);
                            }
                        }
                    }
                });
        }
        #endregion

        #region private int LinkPathDifficulty(LocalLink targetLink)
        private int LinkPathDifficulty(LocalLink targetLink)
        {
            int _offSet(CellLocation c1, CellLocation c2, Axis exclude)
            {
                return exclude switch
                {
                    Axis.X => (Math.Abs(c1.Z - c2.Z) + Math.Abs(c1.Y - c2.Y)),
                    Axis.Y => (Math.Abs(c1.X - c2.X) + Math.Abs(c1.Z - c2.Z)),
                    _ => (Math.Abs(c1.Y - c2.Y) + Math.Abs(c1.X - c2.X)),
                };
            }

            // NOTE: project into is the room being transitted, so for targetLink...it means "ProjectFrom"
            var _into = ChannelFlow.project;
            var _fromFace = ChannelFlow.link.GetAnchorFaceInGroup(_into);
            var _toFace = targetLink.GetAnchorFaceInGroup(_into);
            var _fromCell = ChannelFlow.link.InteractionCell(_into);
            var _toCell = targetLink.InteractionCell(_into);
            if (_fromFace == _toFace)
            {
                return (_offSet(_fromCell, _toCell, _fromFace.GetAxis()) / 2) + 1;
            }
            else if (_fromFace.IsOppositeTo(_toFace))
            {
                return (_offSet(_fromCell, _toCell, _fromFace.GetAxis()) / 4);
            }
            return (_offSet(_fromCell, _toCell, _fromFace.GetAxis()) / 3) + 1;
        }
        #endregion

        #region private (ICellLocation cell, double range, int distanceDiff, DeltaCalcInfo difficulty, int extra, int roomTrans, bool isEffect) TransitSoundParameters(LocalCellGroup projectIntoGroup, LocalLink targetLink)
        private (ICellLocation cell, double range, int distanceDiff, DeltaCalcInfo difficulty, int extra, int roomTrans, bool isEffect) TransitSoundParameters(LocalCellGroup projectIntoGroup, LocalLink targetLink)
        {
            // distance and attenuation/cancellation
            var _sCell = ChannelFlow.link.InteractionCell(projectIntoGroup);
            var _tCell = targetLink.InteractionCell(projectIntoGroup);
            var _distance = IGeometricHelper.Distance(_sCell, _tCell) + 5;  // +5 to get to target next group center
            var _ortho = ChannelFlow.link.AnchorFaceInA.IsOrthogonalTo(targetLink.AnchorFaceInA.GetAxis());
            var _distanceDiff = (int)Math.Ceiling(_distance / 10);

            // from Link to _link...
            var _range = Math.Max(SoundPresence.RangeRemaining - _distance, 0);
            if (_range > 0)
            {
                // just going to test one line...
                var _line = ChannelFlow.link.MapContext.Map.SegmentCells(_sCell.GetPoint3D(), _tCell.GetPoint3D(),
                    new SegmentSetFactory(targetLink.MapContext.Map, _sCell, _tCell, ITacticalInquiryHelper.EmptyArray,
                    SegmentSetProcess.Geometry), PlanarPresence.Both);

                // new range check: blocked is -20 range, otherwise ortho is -10 range
                _range = Math.Max(_range - (_line.IsLineOfEffect ? (_ortho ? 10 : 0) : 20), 0); // +10 for not having a direct line (oblique angles)
                if (_range > 0)
                {
                    var _soundTrans = new SoundTransit(SoundPresence.Audible);
                    var _interact = new Interaction(null, ChannelFlow.link.MapContext.Map.GetICore<ICore>(SoundPresence.Audible.SourceID), null, _soundTrans);
                    if (_line.CarryInteraction(_interact))
                    {
                        var _linkPath = LinkPathDifficulty(targetLink);
                        return (_sCell, _range, _distanceDiff,
                            SoundPresence.BaseDifficulty, _soundTrans.AddedDifficulty.EffectiveValue + _linkPath, _linkPath, _line.IsLineOfEffect);
                    }
                }
            }
            return (null, 0, 0, new DeltaCalcInfo(Guid.Empty, string.Empty), 0, 0, false);
        }
        #endregion

        #region public void Cleanup()
        public void Cleanup(int generation)
        {
            // clean up inbounds that were non-contributory
            foreach (var _i in _Inbound.Where(_r => _r.Value?.RangeRemaining <= 0).ToList())
            {
                _Inbound.TryRemove(_i.Key, out var _);
            }

            // perform cleanup on children
            var _nList = _Next.ToList();
            if (generation < 2)
            {
                Parallel.ForEach(_nList, _channel =>
                {
                    // child performs same cleanup (maybe removing this inbound...)
                    _channel.Value?.Cleanup(generation + 1);

                    // no longer an inbound for child?
                    if (!(_channel.Value?.ReceivedSounds.ContainsKey(this) ?? true))
                    {
                        // if so, drop child also (reciprocity)
                        _Next.TryRemove(_channel.Key, out var _);
                    }
                });
            }
            else
            {
                foreach (var _channel in _nList)
                {
                    // child performs same cleanup (maybe removing this inbound...)
                    _channel.Value?.Cleanup(generation + 1);

                    // no longer an inbound for child?
                    if (!(_channel.Value?.ReceivedSounds.ContainsKey(this) ?? true))
                    {
                        // if so, drop child also (reciprocity)
                        _Next.TryRemove(_channel.Key, out var _);
                    }
                }
            }

            // no inbounds at all?
            if (_Inbound.Count == 0)
            {
                // remove from the local-link
                ChannelFlow.link.ChannelsTo(ChannelFlow.project).TryRemove(SoundGroup.ID, out var _);
            }
        }
        #endregion
    }
}
