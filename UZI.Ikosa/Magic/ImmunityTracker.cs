using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Magic
{
    /// <summary>
    /// Used to track IDs affected or immune to a magical effect.
    /// </summary>
    [Serializable]
    public class ImmunityTracker
    {
        #region Construction
        public ImmunityTracker()
        {
            _Infos = new Dictionary<Guid, ImmunityInfo>();
        }
        #endregion

        #region private data
        private Dictionary<Guid, ImmunityInfo> _Infos;
        #endregion

        public bool IsTracking(Guid id) { return _Infos.ContainsKey(id); }
        public ImmunityInfo Info(Guid id) { return _Infos[id]; }

        /// <summary>
        /// Track lack of immunity for an indeterminate time
        /// </summary>
        /// <param name="id"></param>
        public void TrackAffect(Guid id)
        {
            if (IsTracking(id))
                _Infos.Remove(id);
            _Infos.Add(id, new ImmunityInfo { Immune = false, ImmunityEndTime = double.MaxValue });
        }

        /// <summary>
        /// Track lack of immunity to a specific time
        /// </summary>
        /// <param name="id"></param>
        /// <param name="endTime"></param>
        public void TrackAffect(Guid id, double endTime)
        {
            if (IsTracking(id))
                _Infos.Remove(id);
            _Infos.Add(id, new ImmunityInfo { Immune = false, ImmunityEndTime = endTime });
        }

        /// <summary>
        /// Track immunity for an indeterminate time
        /// </summary>
        /// <param name="id"></param>
        public void TrackImmunity(Guid id)
        {
            if (IsTracking(id))
                _Infos.Remove(id);
            _Infos.Add(id, new ImmunityInfo { Immune = true, ImmunityEndTime = double.MaxValue });
        }

        /// <summary>
        /// Track immunity to a specific time
        /// </summary>
        /// <param name="id"></param>
        /// <param name="endTime"></param>
        public void TrackImmunity(Guid id, double endTime)
        {
            if (IsTracking(id))
                _Infos.Remove(id);
            _Infos.Add(id, new ImmunityInfo { Immune = true, ImmunityEndTime = endTime });
        }

        /// <summary>
        /// If an adjunct tracks time, it should call this on its sweep if immunity can terminate
        /// </summary>
        public void EndExpiredInfos(double currentTime)
        {
            // snapshot expired guids into array then remove all KVPs matching the expired
            foreach (var _expired in (from _inf in _Infos
                                      where _inf.Value.ImmunityEndTime < currentTime
                                      select _inf.Key).ToArray())
                _Infos.Remove(_expired);
        }
    }
}
