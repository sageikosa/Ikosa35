using System;
using System.Collections.Generic;
using Uzi.Ikosa.Feats;
using Uzi.Core.Dice;
using Uzi.Core;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.TypeListers;
using System.Linq;
using System.ComponentModel;
using Uzi.Ikosa.Contracts;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Advancement
{
    /// <summary>
    /// Manages health points (from PD rolls) on creature.  Also tracks feats, abilities and skill points.  
    /// Once set, they can only be rolled back.  
    /// Skill points are interfaced higher up the chain to the creature, but managed here.
    /// </summary>
    [Serializable]
    public sealed class PowerDie : ICreatureBound, IControlChange<int>
    {
        #region Internal Construction
        internal PowerDie(Creature creature, int level, AdvancementLogItem pdGroup, PowerDieCalcMethod method)
        {
            _Creature = creature;
            _Level = level;
            _PowerGroup = pdGroup;
            _ValueCtrlr = new ChangeController<int>(this, 0);

            // skill setup
            _SkillPointsLeft = AdvancementClass.SkillPointsPerLevel + Creature.Abilities.Intelligence.AdvancementSkillPointModifier(level);
            if (_SkillPointsLeft < 1)
                _SkillPointsLeft = 1;
            if (Creature.AdvancementLog.NumberPowerDice == 0)
            {
                // first power die
                _SkillPointsLeft *= 4;
            }
            _SkillAssignments = new Dictionary<SkillBase, SkillBuy>();

            IsLocked = false;

            // hit-point setup
            switch (method)
            {
                case PowerDieCalcMethod.Average:
                    AverageHealthPoints();
                    break;

                case PowerDieCalcMethod.Max:
                    MaxHealthPoints();
                    break;

                case PowerDieCalcMethod.Roll:
                    RerollHealthPoints();
                    break;
            }
        }
        #endregion

        #region data
        private int _Level;
        private bool _IsSmallFraction = false;
        private bool _IsFractional = false;
        private int _HealthPoints = -1;
        private int _SkillPointsLeft;
        private Dictionary<SkillBase, SkillBuy> _SkillAssignments;
        private FeatBase _Feat;
        private string _AbilityBoostMnemonic = string.Empty;
        private AdvancementLogItem _PowerGroup;
        private Creature _Creature;
        private ChangeController<int> _ValueCtrlr;
        #endregion

        public bool IsFeatMissing
            => IsFeatPowerDie && ((Feat == null) || !Feat.MeetsRequirementsAtPowerLevel);

        public bool IsAbilityBoostMissing
            => IsAbilityBoostPowerDie && string.IsNullOrEmpty(AbilityBoostMnemonic);

        public bool IsSkillAssignmentMissing
            => SkillPointsLeft != 0;

        public bool IsHealthPointCountLow
            => HealthPoints < 1;

        #region public bool IsLockable { get; }
        /// <summary>All necessary PowerDie settings must be set (Feats, Abilities, and Skills)</summary>
        public bool IsLockable
            => !(IsFeatMissing 
            || IsAbilityBoostMissing 
            || IsSkillAssignmentMissing 
            || IsHealthPointCountLow 
            || IsLocked);
        #endregion

        /// <summary>Indication whether the settings of the PowerDie have all been applied</summary>
        public bool IsLocked { get; internal set; }

        #region internal void Lock()
        /// <summary>
        /// Commits skill points.  
        /// </summary>
        internal void Lock()
        {
            if (!IsLockable)
                return;
            IsLocked = true;
        }
        #endregion

        /// <summary>PowerDie level for the creature at this particular power die</summary>
        public int Level => _Level;

        public decimal AltFraction
            => IsFractional
            ? (IsSmallestFractional ? PowerGroup.AdvancementClass.SmallestFraction : PowerGroup.AdvancementClass.OptionalFraction)
            : 1m;

        #region public bool IsSmallestFractional { get; set; }
        public bool IsSmallestFractional
        {
            get => _IsSmallFraction;
            set
            {
                if (!IsLocked && (Level == 1) && (PowerGroup.AdvancementClass.SmallestFraction < PowerGroup.AdvancementClass.OptionalFraction))
                {
                    if (_IsSmallFraction != value)
                    {
                        _IsSmallFraction = value;
                        if (IsSmallestFractional && !IsFractional)
                        {
                            IsFractional = true;
                        }
                        AverageHealthPoints();
                    }
                }
            }
        }
        #endregion

        #region public bool IsFractional { get; set; }
        /// <summary>Only the first power die can be fractional</summary>
        public bool IsFractional
        {
            get => _IsFractional;
            set
            {
                if (!IsLocked && (Level == 1) && (PowerGroup.AdvancementClass.OptionalFraction < 1m))
                {
                    if (_IsFractional != value)
                    {
                        _IsFractional = value;
                        if (!IsFractional && IsSmallestFractional)
                        {
                            IsSmallestFractional = false;
                        }
                        AverageHealthPoints();
                    }
                }
            }
        }
        #endregion

        #region Reroll HP
        public void RerollHealthPoints()
        {
            RollbackHealthPoints();
            EnsureOneHealthPoint(DiceRoller.RollDie(Creature.ID, AdvancementClass.PowerDieSize, @"Health Points", @"Power-die"));
        }
        #endregion

        #region Max HP
        public void MaxHealthPoints()
        {
            RollbackHealthPoints();
            EnsureOneHealthPoint(AdvancementClass.PowerDieSize);
        }
        #endregion

        #region Avg HP
        public void AverageHealthPoints()
        {
            RollbackHealthPoints();
            var _hp = (AdvancementClass.PowerDieSize + 1) / 2;
            if (IsFractional)
            {
                // fractional average power die
                _hp = (int)Math.Floor((decimal)_hp * AltFraction);
            }
            else
            {
                if ((Level % 2) == 0)
                    _hp++;
            }
            EnsureOneHealthPoint(_hp);
        }
        #endregion

        #region public void SetHealthPoints(int val)
        public void SetHealthPoints(int val)
        {
            if ((val > 0) && (val <= AdvancementClass.PowerDieSize))
            {
                RollbackHealthPoints();
                EnsureOneHealthPoint(val);
            }
        }
        #endregion

        #region Common HP Functions
        private void EnsureOneHealthPoint(int newHealthPoints)
        {
            var _cMod = Creature.Abilities.Constitution.AdvancementHealthPointModifier(Level);
            if (newHealthPoints + _cMod < 1)
            {
                // must go above 0
                newHealthPoints = 1 - _cMod;
            }
            _HealthPoints = newHealthPoints;
            Creature.HealthPoints.HealthDieHealthPoints += newHealthPoints;
            DoValueChanged();
        }

        internal void RollbackHealthPoints()
        {
            if (_HealthPoints > 0)
            {
                Creature.HealthPoints.HealthDieHealthPoints -= _HealthPoints;
                _HealthPoints = 0;
                DoValueChanged();
            }
        }
        #endregion

        #region Health Points
        /// <summary>Health points added by this power die</summary>
        public int HealthPoints => _HealthPoints;

        public int EffectiveHealthPoints
            => _HealthPoints + Creature.Abilities.Constitution.IModifier.Value;
        #endregion

        #region public int SkillPointsLeft { get; set; }
        public int SkillPointsLeft
        {
            get
            {
                if (Creature.Abilities.Intelligence.IsNonAbility)
                    return 0;
                return _SkillPointsLeft;
            }
            set
            {
                // only let it change if there is something left (if all allocated, then no more can be added)
                if (!IsLocked)
                    _SkillPointsLeft = value;
            }
        }
        #endregion

        #region public int CurrentSkillPointAssignment(SkillBase skill)
        public int CurrentSkillPointAssignment(SkillBase skill)
        {
            if (_SkillAssignments.ContainsKey(skill))
            {
                return _SkillAssignments[skill].PointsUsed;
            }
            else
                return 0;
        }
        #endregion

        #region public void AssignSkillPoints(SkillBase skill, int points)
        /// <summary>
        /// Groups the absolute assignment of points to the skill for this power die.  
        /// Continues to work until the PowerDie is locked or all points are used.
        /// </summary>
        public void AssignSkillPoints(SkillBase skill, int points)
        {
            if (IsLocked)
                return;
            if (skill.Creature == Creature)
            {
                if (points <= 0)
                {
                    // 0 clears the skillbuy and removes item from the list
                    if (_SkillAssignments.ContainsKey(skill))
                    {
                        // only remove if assigned
                        _SkillAssignments[skill].PointsUsed = 0;
                        _SkillAssignments.Remove(skill);
                    }
                }
                else if (points > 0)
                {
                    if (_SkillAssignments.ContainsKey(skill))
                    {
                        // modifying
                        _SkillAssignments[skill].PointsUsed = points;
                    }
                    else
                    {
                        var _buy = new SkillBuy(skill, this)
                        {
                            PointsUsed = points
                        };
                        _SkillAssignments.Add(skill, _buy);
                    }
                }
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(SkillsAssigned)));
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsSkillAssignmentMissing)));
                }
            }
        }
        #endregion

        #region public IEnumerable<SkillBuy> SkillsAssigned
        public IEnumerable<SkillBuy> SkillsAssigned
        {
            get
            {
                foreach (KeyValuePair<SkillBase, SkillBuy> _kvp in _SkillAssignments)
                {
                    yield return _kvp.Value;
                }
                yield break;
            }
        }
        #endregion

        #region public double SkillRanks(SkillBase skill)
        public double SkillRanks(SkillBase skill)
        {
            if (_SkillAssignments.ContainsKey(skill))
                return _SkillAssignments[skill].RanksAccumulated;
            else
                return 0d;
        }
        #endregion

        #region public void RollbackSkillPoints()
        public void RollbackSkillPoints()
        {
            if (!IsLocked)
            {
                foreach (SkillBuy _buy in SkillsAssigned)
                {
                    _buy.PointsUsed = 0;
                }
                _SkillAssignments.Clear();
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(@"SkillsAssigned"));
                    PropertyChanged(this, new PropertyChangedEventArgs(@"IsSkillAssignmentMissing"));
                }
            }
        }
        #endregion

        #region public bool IsFeatPowerDie { get; }
        public bool IsFeatPowerDie
        {
            get
            {
                if (Creature.Abilities.Intelligence.IsNonAbility)
                    return false;
                return (((Level % 3) == 0) || (Level == 1));
            }
        }
        #endregion

        #region public FeatBase Feat { get; set; }
        /// <summary>
        /// Setting a value when the property is 'null' will test CanAdd(), store the value and BindTo() the creature.
        /// Only valid for every third PD (and PowerDie #1)
        /// </summary>
        public FeatBase Feat
        {
            get => _Feat;
            set
            {
                // only every third level
                if (IsFeatPowerDie)
                {
                    // make sure we're trying to truly assign a feat
                    if (value != null)
                    {
                        // try to remove existing feat
                        if (!IsLocked && (_Feat != null))
                        {
                            if (Feat.CanRemove())
                            {
                                Feat.UnbindFromCreature();
                                _Feat = null;
                            }
                            else
                                throw new InvalidOperationException($@"Cannot unbind feat {_Feat.Name} on PowerDie {Level}");
                        }

                        // if not set, allow it to be set
                        if (_Feat == null)
                        {
                            if (value.CanAdd(Creature))
                            {
                                _Feat = value;
                                _Feat.BindTo(Creature);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region public IEnumerable<FeatListItem> AvailableFeats { get; }
        public IEnumerable<FeatListItem> AvailableFeats
        {
            get
            {
                if (IsFeatPowerDie)
                {
                    foreach (FeatListItem _item in from _available in FeatLister.AvailableFeats(Creature, this, Level)
                                                   orderby _available.Name
                                                   select _available)
                    {
                        yield return _item;
                    }
                }
                yield break;
            }
        }
        #endregion

        public bool IsAbilityBoostPowerDie
            => Creature.Species.SupportsAbilityBoosts && ((Level % 4) == 0);

        #region public string AbilityBoostMnemonic { get; set; }
        public string AbilityBoostMnemonic
        {
            get => _AbilityBoostMnemonic;
            set
            {
                // only every fourth level
                if (IsAbilityBoostPowerDie)
                {
                    if (string.Compare(value, _AbilityBoostMnemonic, true) == 0)
                        return;

                    // try to clear any existing value
                    if (!IsLocked && !string.IsNullOrWhiteSpace(_AbilityBoostMnemonic))
                    {
                        // unboost and stop tracking
                        Creature.Abilities[AbilityBoostMnemonic]?.Unboost(Level);
                        _AbilityBoostMnemonic = null;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs(nameof(AbilityBoostMnemonic)));
                            PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsAbilityBoostMissing)));
                        }
                    }

                    // then set new (as long as it is cleared)
                    if (string.IsNullOrWhiteSpace(_AbilityBoostMnemonic))
                    {
                        // get ability (if possible)
                        var _ability = Creature.Abilities[value];

                        // track and boost
                        _AbilityBoostMnemonic = _ability?.Mnemonic;
                        _ability?.Boost(Level);

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs(nameof(AbilityBoostMnemonic)));
                            PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsAbilityBoostMissing)));
                        }
                    }
                }
            }
        }
        #endregion

        #region public AbilityBase AbilityBoost { get; set; }
        public AbilityBase AbilityBoost
        {
            get
            {
                if (!AbilityBoostMnemonic.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
                    return Creature.Abilities[AbilityBoostMnemonic];
                else
                    return null;
            }
            set
            {
                if (value != null)
                {
                    AbilityBoostMnemonic = value.Mnemonic;
                }
                else
                {
                    AbilityBoostMnemonic = null;
                }
            }
        }
        #endregion

        public AdvancementLogItem PowerGroup => _PowerGroup;

        /// <summary>Advancement class for the power die</summary>
        public AdvancementClass AdvancementClass
            => _PowerGroup.AdvancementClass;

        public Creature Creature => _Creature;

        #region ValueChanged Event
        private void DoValueChanged()
        {
            _ValueCtrlr.DoValueChanged(_HealthPoints);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("HealthPoints"));
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("EffectiveHealthPoints"));
            }
        }

        #region IControlChange<int> Members
        public void AddChangeMonitor(IMonitorChange<int> subscriber)
        {
            _ValueCtrlr.AddChangeMonitor(subscriber);
        }

        public void RemoveChangeMonitor(IMonitorChange<int> subscriber)
        {
            _ValueCtrlr.RemoveChangeMonitor(subscriber);
        }
        #endregion
        #endregion

        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #region public PowerDieInfo ToPowerDieInfo()
        public PowerDieInfo ToPowerDieInfo()
            => new PowerDieInfo
            {
                BoostedAbility = Creature.Abilities[AbilityBoostMnemonic]?.ToAbilityInfo(Creature),
                AltFraction = AltFraction,
                EffectiveHealthPoints = EffectiveHealthPoints,
                Feat = Feat?.ToFeatInfo(),
                HealthPoints = HealthPoints,
                IsAbilityBoostPowerDie = IsAbilityBoostPowerDie,
                IsFeatPowerDie = IsFeatPowerDie,
                IsFractional = IsFractional,
                IsLockable = IsLockable,
                IsLocked = IsLocked,
                IsSmallestFractional = IsSmallestFractional,
                PowerLevel = Level,
                SkillPointsLeft = SkillPointsLeft,
                SkillsAssigned = SkillsAssigned.Select(_sb => _sb.ToSkillBuyInfo()).ToList()
            };
        #endregion
    }

    public enum PowerDieCalcMethod
    {
        Roll, Average, Max
    }

    /// <summary>Indicates the number of power dice the creature has.</summary>
    [Serializable]
    public readonly struct PowerDiceCount
    {
        public PowerDiceCount(int count)
        {
            Count = count;
        }

        public readonly int Count;
    }
}
