using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class NaturalHealing : GroupParticipantAdjunct, ITrackTime, ITimelineInterruptable
    {
        public NaturalHealing(double startTime, bool fullRest, Guid mapID, RecoveryRest rest)
            : base(typeof(NaturalHealing), rest)
        {
            _StartTime = startTime;
            _FullRest = fullRest;
            _MapID = mapID;
            _HoursNeeded = fullRest ? 24 : 8;
        }

        #region fixup hours so they don't fall in same day
        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            if (Anchor is Creature _critter)
            {
                if (!FullRest)
                {
                    // make sure projected end time is in next calendar day
                    var _endTime = (_critter?.GetCurrentTime() ?? 0) + (HoursNeeded * Hour.UnitFactor);
                    var _endDay = Math.Floor(_endTime / Day.UnitFactor);
                    var _last = Math.Floor(_critter.HealthPoints.LastNaturalHealTime / Day.UnitFactor);
                    if (_endDay <= _last)
                    {
                        // need to dip into the next day, so add hours
                        var _nextDayStart = (_last + 1) * Day.UnitFactor;
                        _HoursNeeded += (int)Math.Ceiling((_nextDayStart - _endTime) / Hour.UnitFactor);
                    }
                }
            }
            base.OnAnchorSet(oldAnchor, oldSetting);
        }
        #endregion

        #region data
        private double _StartTime;
        private bool _FullRest;
        private Guid _MapID;
        private int _HoursNeeded;
        #endregion

        public double StartTime => _StartTime;
        public bool FullRest => _FullRest;
        public Guid MapID => _MapID;
        public int HoursNeeded => _HoursNeeded;

        public void Interrupt()
            => Eject();

        public RecoveryRest RecoveryRest => Group as RecoveryRest;

        public override object Clone()
            => new NaturalHealing(StartTime, FullRest, MapID, RecoveryRest);

        public override void BindToSetting()
        {
            base.BindToSetting();
            if (Anchor?.Setting?.ID != MapID)
            {
                Eject();
            }
        }

        public double Resolution => Hour.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (direction == TimeValTransition.Leaving)
            {
                var _span = timeVal - _StartTime;
                var _spent = (int)Math.Floor(_span / Hour.UnitFactor);
                if (_spent > _HoursNeeded)
                {
                    var _critter = Anchor as Creature;
                    _critter.HealthPoints.LastNaturalHealTime = timeVal;

                    // amount health healed
                    var _amount = new Deltable(_critter.AdvancementLog.NumberPowerDice * (FullRest ? 2 : 1));
                    var _recover = new RecoverHealthPointData(_critter, _amount, false, false);
                    var _heal = new Interaction(_critter, this, _critter, _recover);
                    _critter.HandleInteraction(_heal);

                    // if there are new unyielded prerequisites, create a retry interaction step
                    if (_heal.Feedback.OfType<PrerequisiteFeedback>().Any(_f => !_f.Yielded))
                    {
                        // to accomplish the interaction, we need more information
                        _critter.Setting.ContextSet.ProcessManager.StartProcess(
                            new CoreProcess(
                                new RetryInteractionStep(@"Natural Heal Prerequisites", _heal), @"Natural Healing"));
                    }

                    // recover ability damages
                    var _abilityAmount = FullRest ? 2 : 1;
                    foreach (var _ability in _critter.Abilities.AllAbilities.Where(_a => _a.Damage != null))
                    {
                        _ability.RecoverDamage(_abilityAmount);
                    }

                    // no longer resting
                    Eject();
                }
            }
        }
    }
}
