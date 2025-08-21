using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class SoundAwareness
    {
        #region ctor()
        public SoundAwareness(IAudible audible, int rollValue)
        {
            // invariants
            _Audible = audible;
            _Roll = rollValue;

            // impacts
            _Impacts = [];
            _Magnitude = null;
            _Vector = null;
            _LastPoint = null;
            _Exceed = 0;
            _SrcRange = 0;
            _Stream = [];

            // times
            _CheckRemove = null;
            _NoticeRemove = null;
            _LostNoticeTime = null;
        }
        #endregion

        #region state
        private IAudible _Audible;
        private double _SrcRange;
        private int _Roll;
        private List<SoundImpact> _Impacts;
        private double? _CheckRemove;
        private double? _NoticeRemove;
        private double? _LostNoticeTime;
        private Point3D? _LastPoint;
        private double _MoveFade;
        private int _Exceed;

        // tracking dominant
        private Vector3D? _Vector;
        private double? _Magnitude;
        private int _Linger;
        private List<ExpirableDescription> _Stream;
        #endregion

        private class ExpirableDescription { public double? Expiry; public SoundInfo SInfo; };

        public int RollValue => _Roll;
        public IAudible Audible => _Audible;
        public double SourceRange => _SrcRange;

        /// <summary>How much the last succesful check exceeded the difficulty</summary>
        public int CheckExceed => _Exceed;

        /// <summary>Time at which the sound awareness should be removed from tracking.</summary>
        public double? CheckRemove => _CheckRemove;

        /// <summary>Time at which lingering sound awareness ends.</summary>
        public double? NoticeRemove => _NoticeRemove;

        /// <summary>Last time at which sound became de-active</summary>
        public double? LostNoticeTime => _LostNoticeTime;

        /// <summary>Effective magnitude</summary>
        public double? Magnitude => _Magnitude;

        /// <summary>NULL if never actually heard.</summary>
        public Vector3D? Vector => _Vector;

        /// <summary>Max distance from last notice</summary>
        public double MoveFade => _MoveFade;

        /// <summary>Sound impacts that were actually heard</summary>
        public IEnumerable<SoundImpact> SoundImpacts
            => _Impacts.Select(_si => _si);

        /// <summary>True if there are active impacts</summary>
        public bool IsActive
            => _Impacts.Any();

        /// <summary>True if a dominant sound was ever noticed.  This is the filter for presentation in the VisualizationService.</summary>
        public bool HasNoticed
            => _Magnitude != null;

        #region private void SetRemoveTimes(double time)
        private void SetRemoveTimes(double time)
        {
            // last deactive time, or current time
            _LostNoticeTime ??= time;

            // last remove time or variable based on conditions
            _CheckRemove ??= (time + Round.UnitFactor);

            if (!_NoticeRemove.HasValue && HasNoticed)
            {
                _NoticeRemove = time + (2 * Round.UnitFactor);
            }

            // remove expirables
            foreach (var _ed in _Stream.Where(_p => _p.Expiry != null && _p.Expiry <= time).ToList())
            {
                _Stream.Remove(_ed);
            }

            // non-expiring previous sounds set to expire
            foreach (var _ed in _Stream.Where(_p => _p.Expiry == null))
            {
                _ed.Expiry = _NoticeRemove;
            }
        }
        #endregion

        /// <summary>Not active, has noticed, and notice has timed-out</summary>
        public bool CanClearNotice(double time)
            => !IsActive
            && (HasNoticed && time >= _NoticeRemove);

        #region public bool IsRemoveable(double time)
        /// <summary>Not active and check remove has timed out</summary>
        public bool IsRemoveable(double time)
        {
            if (IsActive)
            {
                return false;
            }

            // if not active, should have CheckRemove
            if (!HasNoticed)
            {
                // not noticed, so no need to compare lingering
                return time >= _CheckRemove;
            }
            else
            {
                // if noticed, should have linger remove
                return time >= Math.Max(_CheckRemove.Value, _NoticeRemove.Value);
            }
        }
        #endregion

        #region ClearNotice()
        /// <summary>Clear remnance of sound, but not lingering checks so previous checks are re-used</summary>
        public void ClearNotice()
        {
            _Magnitude = null;
            _Vector = null;
            _NoticeRemove = null;
            _Stream.Clear();
        }
        #endregion

        #region ClearImpacts(...)
        /// <summary>clear impacts, but not everything, so some remnance of sound is tracked</summary>
        public void ClearImpacts(double time, Point3D refPoint)
        {
            _Impacts.Clear();
            _MoveFade = Math.Max(_MoveFade, ((_LastPoint ?? refPoint) - refPoint).Length);
            SetRemoveTimes(time);
        }
        #endregion

        #region public void SetImpacts(List<SoundImpact> impacts, ISensorHost sensors, double time, Point3D ptImpact, double sourceRange, int exceed)
        public void SetImpacts(List<SoundImpact> impacts, ISensorHost sensors, double time, Point3D ptImpact, IAudible audible, double sourceRange, int exceed)
        {
            _Impacts = impacts;
            if (impacts.Any())
            {
                // positive impact, means clear
                var _dominant = impacts.OrderByDescending(_i => _i.RelativeMagnitude).Select(_i => _i).FirstOrDefault();
                _Magnitude = _dominant.RelativeMagnitude;
                _Audible = audible;
                _SrcRange = sourceRange;
                _Linger = _dominant.Check.Result - _dominant.Difficulty.Result;
                _Vector = _dominant.Region.GetPoint3D() - ptImpact;

                // capture current info into stream...
                var _info = Audible.GetSoundInfo(sensors, this);
                if (_info != null)
                {
                    // remove expirables
                    foreach (var _ed in _Stream.Where(_p => _p.Expiry != null && _p.Expiry <= time).ToList())
                    {
                        _Stream.Remove(_ed);
                    }

                    // mark non-matchers with no expiry as expirable
                    foreach (var _ed in _Stream.Where(_p => !_p.SInfo.IsMatch(_info) && _p.Expiry == null).ToList())
                    {
                        _ed.Expiry = time + Round.UnitFactor;
                    }

                    // if there are no previous without expiry
                    if (!_Stream.Any(_p => _p.Expiry == null))
                    {
                        // add ours without expiry...
                        _Stream.Add(new ExpirableDescription { SInfo = _info });
                    }
                }

                // not deactivated, nor scheduled for removal
                _LostNoticeTime = null;
                _CheckRemove = null;
                _NoticeRemove = null;
                _LastPoint = ptImpact;
                _MoveFade = 0d;
                _Exceed = exceed;
            }
            else
            {
                _MoveFade = Math.Max(_MoveFade, ((_LastPoint ?? ptImpact) - ptImpact).Length);
                SetRemoveTimes(time);
            }
        }
        #endregion

        #region public SoundAwarenessInfo ToSoundAwarenessInfo(LocalMap map, Creature creature)
        public SoundAwarenessInfo ToSoundAwarenessInfo(LocalMap map, Creature creature)
        {
            // vector (to source, or channel source)
            var _vector = Vector.Value;
            var _range = Vector?.Length ?? 0d;

            // scale range by time fade...use time fade for something else...?
            if (_range < 5)
            {
                _range = 0;
            }
            else if (_range < 20)
            {
                _range = 5;
            }
            else
            {
                _range = 10;
            }

            _vector.Normalize();
            if (MoveFade >= 10)
            {
                // could be co-greatest...
                var _x = Math.Abs(_vector.X);
                var _y = Math.Abs(_vector.Y);
                var _z = Math.Abs(_vector.Z);
                var _max = Math.Max(Math.Max(_x, _y), _z);

                // greatest single anchor-face cardinal vector
                _vector = new Vector3D(
                    (_x == _max) ? _vector.X : 0,
                    (_y == _max) ? _vector.Y : 0,
                    (_z == _max) ? _vector.Z : 0);

                // cap range to 5 if moved too much
                if (_range > 5)
                {
                    _range = 5;
                }
            }
            else if (MoveFade >= 5)
            {
                // nearest of 26 coordinated anchor-face cardinal vectors
                _vector = new Vector3D(Math.Round(_vector.X), Math.Round(_vector.Y), Math.Round(_vector.Z));
            }
            else
            {
                // original source is really close/strong; but creature not quite aware
                if (((SourceRange - Magnitude) <= 30d)
                    && (creature.Awarenesses.GetAwarenessLevel(Audible.SourceID) < AwarenessLevel.Aware))
                {
                    // simplfy the vector
                    _vector = new Vector3D(Math.Round(_vector.X), Math.Round(_vector.Y), Math.Round(_vector.Z));
                }
            }

            double? _fade()
                => IsActive
                ? 1d
                : (NoticeRemove - map.CurrentTime) / (NoticeRemove - LostNoticeTime);

            return new SoundAwarenessInfo
            {
                ID = Audible.SoundGroupID,
                Vector = _vector,
                Magnitude = Magnitude ?? 0d,
                IsActive = IsActive,
                Range = _range,
                TimeFade = _fade() ?? 0,
                Stream = _Stream.Select(_ed => _ed.SInfo).ToList()
            };
        }
        #endregion
    }
}
