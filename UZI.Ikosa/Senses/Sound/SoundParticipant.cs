using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class SoundParticipant : GroupParticipantAdjunct, ITrackTime
    {
        public SoundParticipant(object source, SoundGroup soundGroup)
            : base(source, soundGroup)
        {
        }

        #region state
        private double? _Expire;
        #endregion

        public SoundGroup SoundGroup => Group as SoundGroup;

        public PlanarPresence PlanarPresence => Anchor?.GetPlanarPresence() ?? PlanarPresence.None;

        /// <summary>Yields local cell groups to which this sound participant belongs (if non-ethereal)</summary>
        public IEnumerable<LocalCellGroup> GetLocalCellGroups()
        {
            var _loc = Anchor?.GetLocated()?.Locator;
            if (_loc?.PlanarPresence.HasMaterialPresence() ?? false)
                return _loc.GetLocalCellGroups();
            return Enumerable.Empty<LocalCellGroup>();
        }

        public IGeometricRegion GetGeometricRegion()
            => Anchor?.GetLocated()?.Locator.GeometricRegion;

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            base.OnAnchorSet(oldAnchor, oldSetting);
            if ((Anchor != null) && (SoundGroup.SoundPresence != null))
            {
                SoundGroup?.SetSoundRef(SoundGroup.SoundPresence);
            }
        }

        public override object Clone()
            => new SoundParticipant(Source, SoundGroup);

        public bool WillExpire => _Expire != null;

        public void SetExpire(double? expiration)
            => _Expire = expiration;

        public double Resolution => Round.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _Expire) && (direction == TimeValTransition.Leaving))
            {
                Eject();
            }
        }
    }
}
