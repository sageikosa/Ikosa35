using System;
using Uzi.Ikosa.Skills;
using System.ComponentModel;
using Uzi.Ikosa.Contracts;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class SkillBuy : INotifyPropertyChanged
    {
        internal SkillBuy(SkillBase skill, PowerDie powerDie)
        {
            Skill = skill;
            PowerDie = powerDie;
        }

        private int _Points = 0;

        public SkillBase Skill { get; private set; }
        public PowerDie PowerDie { get; private set; }

        /// <summary>Indicates the ranks added to the skill</summary>
        public double RanksAccumulated
            => Skill.IsClassSkillAtPowerLevel(PowerDie.Level)
            ? Convert.ToDouble(_Points)
            : Convert.ToDouble(_Points) / 2d;

        #region public int PointsUsed { get; internal set; }
        /// <summary>
        /// Gets and sets the points used to buy the Skill from the designated PowerDie.  
        /// Updates both the PowerDie and the Skill.BaseValue.
        /// </summary>
        public int PointsUsed
        {
            get => _Points;
            internal set
            {
                // boundary checks
                if (value < 0)
                {
                    // no negative points!
                    value = 0;
                }

                // going up or down?
                var _diff = value - _Points;

                // adjust skill base value
                if (_diff != 0)
                {
                    // do not exceed power die capacity
                    if (_diff > PowerDie.SkillPointsLeft)
                    {
                        // only from the skill point bucket, please
                        value -= (_diff - PowerDie.SkillPointsLeft);
                        _diff = PowerDie.SkillPointsLeft;
                    }

                    // current ranks
                    var _normal = Skill.IsClassSkillAtPowerLevel(PowerDie.Level);
                    var _xDiff = _diff * (_normal ? 1d : 0.5d);
                    var _ranks = Skill.SkillRanksAtPowerLevel(PowerDie.Level);

                    // most legal max
                    var _max = Skill.IsClassSkill
                        ? SkillBase.MaxClassSkillRanksAtPowerLevel(PowerDie.Level)
                        : SkillBase.MaxCrossClassSkillRanksAtPowerLevel(PowerDie.Level);

                    // ranks exceeding max, adjust down
                    if ((_ranks + _xDiff) > _max)
                    {
                        // take some off
                        _xDiff = _max - _ranks;

                        // and adjust points
                        _diff = (int)(_xDiff * (_normal ? 1d : 2d));
                        value = _Points + _diff;
                    }

                    // set new base...
                    Skill.BaseDoubleValue += _xDiff;
                }

                // adjust remaining points and points tracked
                PowerDie.SkillPointsLeft -= _diff;
                _Points = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PointsUsed)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RanksAccumulated)));
            }
        }
        #endregion

        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        public SkillBuyInfo ToSkillBuyInfo()
            => new SkillBuyInfo
            {
                IsClassSkill = (Skill?.IsClassSkill ?? false),
                Skill = Skill?.ToSkillInfo(Skill?.Creature),
                PointsUsed = PointsUsed
            };
    }
}
