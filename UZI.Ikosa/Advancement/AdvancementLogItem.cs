using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core;
using System.ComponentModel;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Contracts;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Advancement
{
    /// <summary>represents a set of advancement items</summary>
    [Serializable]
    public sealed class AdvancementLogItem : IEnumerable<PowerDie>, ICreatureBound, IControlChange<int>, IMonitorChange<int>
    {
        #region Construction
        internal AdvancementLogItem(Creature creature, int startPowerLevel, AdvancementClass advClass, int level)
        {
            _Creature = creature;
            _AltPowerLevel = startPowerLevel;
            AdvancementClass = advClass;
            AdvancementClassLevelLow = level;
            AdvancementClassLevelHigh = level;
            _PowerDiceList = [];
            _ValueCtrlr = new ChangeController<int>(this, 0);
        }

        internal AdvancementLogItem(Creature creature, int startPowerLevel, AdvancementClass advClass, int levelLow, int levelHigh)
        {
            _Creature = creature;
            _AltPowerLevel = startPowerLevel;
            AdvancementClass = advClass;
            AdvancementClassLevelLow = levelLow;
            AdvancementClassLevelHigh = levelHigh;
            _PowerDiceList = [];
            _ValueCtrlr = new ChangeController<int>(this, 0);
        }
        #endregion

        #region data
        private Collection<PowerDie> _PowerDiceList;
        private int _AltPowerLevel;
        private int _HealthPoints;
        private Creature _Creature;
        #endregion

        public AdvancementClass AdvancementClass { get; private set; }
        public int AdvancementClassLevelLow { get; private set; }
        public int AdvancementClassLevelHigh { get; private set; }

        #region internal void Add(PowerDie powerDie)
        internal void Add(PowerDie powerDie)
        {
            // if no level given, then the advancement class does not use power dice
            if (powerDie.AdvancementClass == AdvancementClass)
            {
                // ignore if the class doesn't use power dice
                if (!AdvancementClass.UsesNoPowerDice)
                {
                    // add power die and hook change
                    powerDie.AddChangeMonitor(this); // TODO: determine when a power die drops this list
                    _PowerDiceList.Add(powerDie);
                    ValueChanged(this, null);
                }
            }
        }
        #endregion

        #region internal void Unlock()
        /// <summary>
        /// Ensures advancement class locking is rolled back to before the lowest level.
        /// Then rolls back the power die set.  These calls are checked so that if the 
        /// items are unlocked no bad results happen.  (Should only be called when the 
        /// top Power Dice are being removed or rolled back.)
        /// </summary>
        internal void Unlock()
        {
            for (var _level = AdvancementClassLevelHigh; _level >= AdvancementClassLevelLow; _level--)
            {
                // roll back levels (should be in sync so should only loop once per "for" iteration)
                while (AdvancementClass.LockedLevel >= _level)
                {
                    AdvancementClass.UnlockOneLevel();
                }
            }
            foreach (var _pd in _PowerDiceList)
            {
                // unlock PD
                _pd.IsLocked = false;
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsLocked)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsLockable)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Requirements)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasRequirements)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Features)));
            }
        }
        #endregion

        #region internal void Lock()
        /// <summary>Locks each power die and each advancementClass level</summary>
        internal void Lock()
        {
            foreach (PowerDie _pd in _PowerDiceList)
            {
                // lock PD
                _pd.Lock();
            }
            for (var _level = AdvancementClassLevelLow; _level <= AdvancementClassLevelHigh; _level++)
            {
                // lock class
                AdvancementClass.LockOneLevel();
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsLocked)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsLockable)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Requirements)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasRequirements)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Features)));
            }
        }
        #endregion

        #region internal void Remove()
        /// <summary>
        /// Rolls back skill points, health points, ability boosts and feats associated with each power die in the group
        /// </summary>
        internal void Remove()
        {
            foreach (PowerDie _pd in _PowerDiceList.Where(_pd => !_pd.IsLocked))
            {
                _pd.RollbackSkillPoints();
                _pd.RollbackHealthPoints();
                if (!_pd.IsAbilityBoostMissing)
                {
                    _pd.AbilityBoost = null;
                }
                if (_pd.Feat != null)
                {
                    _pd.Feat.UnbindFromCreature();
                }
            }
        }
        #endregion

        #region public bool IsLocked { get; }
        /// <summary>
        /// Every power die is checked for locking.
        /// Advancement class must be locked up to or beyond the high mark for this group.
        /// </summary>
        public bool IsLocked
        {
            get
            {
                foreach (PowerDie _pd in _PowerDiceList)
                {
                    if (!_pd.IsLocked)
                    {
                        return false;
                    }
                }
                return AdvancementClass.LockedLevel >= AdvancementClassLevelHigh;
            }
        }
        #endregion

        #region public bool IsLockable { get; }
        /// <summary>True if the highest advancement class level can be locked (and each PowerDie is lockable)</summary>
        public bool IsLockable
        {
            get
            {
                foreach (PowerDie _pd in _PowerDiceList)
                {
                    if (!_pd.IsLockable)
                    {
                        return false;
                    }
                }
                return AdvancementClass.CanLockLevel(AdvancementClassLevelHigh);
            }
        }
        #endregion

        /// <summary>Last power die level in the group</summary>
        public int EndLevel
            => (_PowerDiceList.Count > 0)
            ? _PowerDiceList[_PowerDiceList.Count - 1].Level
            : _AltPowerLevel;

        /// <summary>First power die level in the group</summary>
        public int StartLevel
            => (_PowerDiceList.Count > 0)
            ? _PowerDiceList[0].Level
            : _AltPowerLevel;

        public int Count => _PowerDiceList.Count;

        #region public IEnumerable<IFeature> Features { get; }
        /// <summary>Gets all features for the hit dice in thie group</summary>
        public IEnumerable<IFeature> Features
        {
            get
            {
                // most likely this is a single level anyway
                for (var _lx = AdvancementClassLevelLow; _lx <= AdvancementClassLevelHigh; _lx++)
                {
                    foreach (var _ftr in AdvancementClass.Features(_lx))
                    {
                        yield return _ftr;
                    }
                }
                if (AdvancementClass.Creature?.Species != null)
                {
                    foreach (var _pd in _PowerDiceList)
                    {
                        foreach (var _feature in AdvancementClass.Creature.Species.Features(_pd.Level))
                        {
                            yield return _feature;
                        }
                    }
                }
                yield break;
            }
        }
        #endregion

        #region public IEnumerable<AdvancementRequirement> Requirements { get; }
        public IEnumerable<AdvancementRequirement> Requirements
        {
            get
            {
                for (var _lx = AdvancementClassLevelLow; _lx <= AdvancementClassLevelHigh; _lx++)
                {
                    foreach (var _advReq in AdvancementClass.Requirements(_lx))
                    {
                        yield return _advReq;
                    }
                }
                if (AdvancementClass.Creature?.Species != null)
                {
                    foreach (var _pd in _PowerDiceList)
                    {
                        foreach (var _advReq in AdvancementClass.Creature.Species.Requirements(_pd.Level))
                        {
                            yield return _advReq;
                        }
                    }
                }
                yield break;
            }
        }
        #endregion

        public bool HasRequirements
            => Requirements.Any();

        public bool HasOpenRequirements
            => Requirements.Any(_req => !_req.IsSet);

        #region public int SkillPointsLeft { get; }
        /// <summary>gets total skill points left in the group</summary>
        public int SkillPointsLeft
        {
            get
            {
                var _left = 0;
                foreach (var _hd in this)
                {
                    _left += _hd.SkillPointsLeft;
                }
                return _left;
            }
        }
        #endregion

        #region public double SkillRanks(SkillBase skill)
        public double SkillRanks(SkillBase skill)
        {
            var _ranks = 0d;
            foreach (var _pd in this)
            {
                _ranks += _pd.SkillRanks(skill);
            }
            return _ranks;
        }
        #endregion

        #region public int HealthPoints { get; }
        /// <summary>Conveniently tracks all hit points for this group.  Management of Creature HealthPoints occurs in PowerDie class.</summary>
        public int HealthPoints
        {
            get => _HealthPoints;
            set { if (Count == 1)
                {
                    PowerDie.SetHealthPoints(value);
                }
            }
        }
        #endregion

        public IEnumerator<PowerDie> GetEnumerator()
            => _PowerDiceList.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => _PowerDiceList.GetEnumerator();

        public PowerDie this[int index] => _PowerDiceList[index];
        public PowerDie PowerDie => _PowerDiceList[0];

        public Creature Creature => _Creature;

        #region ValueChanged Event
        private void DoValueChanged()
        {
            _ValueCtrlr.DoValueChanged(_HealthPoints);
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(HealthPoints)));
        }

        #region IControlChange<int> Members
        private ChangeController<int> _ValueCtrlr;
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

        #region IMonitorChange<int> Members
        public void PreValueChanged(object sender, ChangeValueEventArgs<int> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<int> args)
        {
            // get all hit points
            var _hp = 0;
            foreach (var _pd in _PowerDiceList)
            {
                _hp += _pd.HealthPoints;
            }

            // store and notify
            _HealthPoints = _hp;
            DoValueChanged();
        }

        public void PreTestChange(object sender, AbortableChangeEventArgs<int> args)
        {
        }
        #endregion

        public AdvancementLogInfo ToAdvancementLogInfo()
            => new AdvancementLogInfo
            {
                Class = AdvancementClass?.ToClassInfo(),
                ClassLevelHigh = AdvancementClassLevelHigh,
                ClassLevelLow = AdvancementClassLevelLow,
                IsLockable = IsLockable,
                IsLocked = IsLocked,
                Features = Features.Select(_f => _f.ToFeatureInfo()).ToList(),
                Requirements = !IsLocked
                                ? Requirements.Select(_r => _r.ToAdvancementRequirementInfo()).ToList()
                                : [],
                PowerDice = _PowerDiceList.Select(_pd => _pd.ToPowerDieInfo()).ToList()
            };

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
