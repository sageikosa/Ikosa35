using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class MentalRest : GroupParticipantAdjunct, ITrackTime, ITimelineInterruptable
    {
        public MentalRest(double startTime, Guid mapID, RecoveryRest rest)
            : base(typeof(MentalRest), rest)
        {
            _MapID = mapID;
            _StartTime = startTime;
            _HoursNeeded = 8;
            _HoursSpent = 0;
        }

        #region data
        private Guid _MapID;
        private double _StartTime;
        private int _HoursNeeded;
        private int _HoursSpent;
        #endregion

        public double StartTime => _StartTime;
        public Guid MapID => _MapID;
        public int HoursNeeded => _HoursNeeded;
        public int HoursSpent => _HoursSpent;
        public int HoursLeft => _HoursNeeded - _HoursSpent;

        public void Interrupt()
            => _HoursNeeded++;

        public RecoveryRest RecoveryRest => Group as RecoveryRest;

        public override object Clone()
            => new MentalRest(StartTime, MapID, RecoveryRest);

        public override void BindToSetting()
        {
            base.BindToSetting();
            if (Anchor?.Setting?.ID != MapID)
                Eject();
        }

        public double Resolution => Hour.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (direction == TimeValTransition.Leaving)
            {
                var _span = timeVal - _StartTime;
                var _spent = (int)Math.Floor(_span / Hour.UnitFactor);
                if (_spent > _HoursSpent)
                {
                    _HoursSpent = _spent;
                    DoPropertyChanged(nameof(HoursSpent));
                    DoPropertyChanged(nameof(HoursLeft));
                    if (HoursLeft <= 0)
                    {
                        var _critter = Anchor as Creature;
                        foreach (var _caster in _critter.Classes.OfType<ISlottedCasterBaseClass>()
                            .Where(_slotBase => _slotBase.MustRestToRecharge).ToList())
                            _critter.AddAdjunct(new FreshMind(_caster));

                        // no longer need mental rest...
                        Eject();
                    }
                }
            }
        }
    }
}
