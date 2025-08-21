using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Skills;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Advancement
{
    /// <summary>
    /// Keeps track of all things specific to hit dice.  
    /// Tracks by power dice group, so monster races that have multiple racial starting PD can operate as a unit.
    /// PowerDie class is used to lock, rollback and remove AdvancementLogItems and advancement class levels.
    /// </summary>
    [Serializable]
    public class AdvancementLog : IEnumerable<AdvancementLogItem>, ICreatureBound, IControlChange<PowerDiceCount>,
        IControlChange<AdvancementLogItem>, INotifyCollectionChanged
    {
        #region construction
        public AdvancementLog(Creature creature)
        {
            _Creature = creature;
            _Groups = new LinkedList<AdvancementLogItem>();
            _HDTallyCtrl = new ChangeController<PowerDiceCount>(this, new PowerDiceCount(0));
            _AdvLogCtrl = new ChangeController<AdvancementLogItem>(this, null);
            _PowerLevel = new Deltable(0);
        }
        #endregion

        #region data
        // more of a reviewable stack
        private LinkedList<AdvancementLogItem> _Groups;
        private Deltable _PowerLevel;
        #endregion

        // TODO: more precise distributor...?

        #region public void DistributeSkillPoints(int powerLevelLow, int powerLevelHigh, Type [] skills, int [] ratios)
        /// <summary>Distributes skills at the desired ratios as best as is possible</summary>
        public void DistributeSkillPoints(int powerLevelLow, int powerLevelHigh, Type[] skills, int[] ratios)
        {
            // TODO: could use the 'if all else fails, try the fall-back list' option
            if (skills.Length != ratios.Length)
            {
                throw new ArgumentOutOfRangeException(@"skills and ratios must be same Length");
            }

            // skill indexer
            var _skx = 0;
            // ratio step indexer
            var _rsx = 0;

            // step through all hit dice
            for (var _pdx = powerLevelLow; _pdx <= powerLevelHigh; _pdx++)
            {
                var _powerDie = this[_pdx];

                // countdown for failure to assign points
                var _failDown = skills.Length;

                // keep assigning while there is something to assign
                while (_powerDie.SkillPointsLeft > 0)
                {
                    SkillBase _skill = Creature.Skills[skills[_skx]];
                    var _ptsLeft = _powerDie.SkillPointsLeft;
                    _powerDie.AssignSkillPoints(_skill, _powerDie.CurrentSkillPointAssignment(_skill) + 1);
                    if (_ptsLeft == _powerDie.SkillPointsLeft)
                    {
                        // failure to budge the assignable points
                        _failDown--;

                        // total failure must mean all skills are maxxed
                        if (_failDown == 0)
                        {
                            break;
                        }

                        // reset ratio indexer
                        _rsx = 0;

                        // go to next skill
                        _skx++;
                    }
                    else
                    {
                        // step up ratio indexer
                        _rsx++;

                        // if exceeded the ratio value, go to next skill
                        if (_rsx >= ratios[_skx])
                        {
                            _rsx = 0;
                            _skx++;
                        }
                    }

                    // loop past the last skill means we go back to the beginning
                    if (_skx >= skills.Length)
                    {
                        _skx = 0;
                    }
                }
            }
        }
        #endregion

        #region public int NumberPowerDice { get; }
        /// <summary>Use this for level-dependent effects, use PowerDiceCount for tallies.</summary>
        public int NumberPowerDice
        {
            get
            {
                if (_Groups.Any())
                {
                    return _Groups.Last.Value.EndLevel;
                }
                else
                {
                    return 0;
                }
            }
        }
        #endregion

        #region public decimal PowerDiceCount { get; }
        /// <summary>Use this in multi-creature tallies, such as when limiting powers to a number of power dice</summary>
        public decimal PowerDiceCount
        {
            get
            {
                if (_Groups.Any())
                {
                    if ((_Groups.Count == 1) && (_Groups.First().Count == 1))
                    {
                        var _pd = _Groups.First().First();
                        return _pd.AltFraction;
                    }
                    else
                    {
                        return _Groups.Last.Value.EndLevel;
                    }
                }
                {
                    return 0m;
                }
            }
        }
        #endregion

        /// <summary>Used for qualifed deltas that may provide boosts to power level depending on interactions</summary>
        public Deltable PowerLevel => _PowerLevel;

        #region internal void Push(AdvancementClass advClass, int advClassLevel, PowerDieCalcMethod method)
        /// <summary>Add another level, so add more power dice</summary>
        internal void Push(AdvancementClass advClass, int advClassLevel, PowerDieCalcMethod method)
        {
            Push(advClass, advClassLevel, advClassLevel, method, method);
        }
        #endregion

        #region internal void Push(AdvancementClass advClass, int advClassLow, int advClassHigh, PowerDieCalcMethod methodFirst, PowerDieCalcMethod methodOthers)
        /// <summary>Add another level, so add more power dice</summary>
        internal void Push(AdvancementClass advClass, int advClassLow, int advClassHigh, PowerDieCalcMethod methodFirst, PowerDieCalcMethod methodOthers)
        {
            var _nextHD = NumberPowerDice + 1;
            var _group = new AdvancementLogItem(Creature, NumberPowerDice, advClass, advClassLow, advClassHigh);
            if (!advClass.UsesNoPowerDice)
            {
                for (var _hx = advClassLow; _hx <= advClassHigh; _hx++)
                {
                    if (_nextHD == NumberPowerDice + 1)
                    {
                        _group.Add(new PowerDie(Creature, _nextHD, _group, methodFirst));
                    }
                    else
                    {
                        _group.Add(new PowerDie(Creature, _nextHD, _group, methodOthers));
                    }
                    _nextHD++;
                }
            }

            // just in case the levels didn't go anywhere
            if ((_group.Count > 0) || (advClass.UsesNoPowerDice))
            {
                _Groups.AddLast(_group);
                PowerLevel.BaseValue = NumberPowerDice;
                _HDTallyCtrl.DoValueChanged(new PowerDiceCount(PowerLevel.BaseValue));
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(NumberPowerDice)));
                DoAdded(_group);
            }
        }
        #endregion

        /// <summary>Indexed power dice</summary>
        public PowerDie this[int powerDieIndex]
            => _Groups
            .Where(_g => _g.StartLevel <= powerDieIndex && _g.EndLevel >= powerDieIndex)
            .SelectMany(_g => _g)
            .FirstOrDefault(_pd => _pd.Level == powerDieIndex);

        #region public AdvancementLogItem this[AdvancementClass advClass, int level] { get; }
        public AdvancementLogItem this[AdvancementClass advClass, int level]
        {
            get
            {
                foreach (var _group in _Groups)
                {
                    if ((_group.AdvancementClass == advClass)
                        && (_group.AdvancementClassLevelLow >= level)
                        && (_group.AdvancementClassLevelHigh <= level))
                    {
                        return _group;
                    }
                }
                return null;
            }
        }
        #endregion

        #region public AdvancementLogItem FirstLockableAdvancementLogItem()
        /// <summary>
        /// Steps through PDGroups to see if they are lockable
        /// </summary>
        public AdvancementLogItem FirstLockableAdvancementLogItem()
        {
            foreach (AdvancementLogItem _group in this)
            {
                if (_group.IsLockable)
                {
                    return _group;
                }
            }
            return null;
        }
        #endregion

        #region public AdvancementLogItem LastLockedAdvancementLogItem()
        public AdvancementLogItem LastLockedAdvancementLogItem()
        {
            AdvancementLogItem _locked = null;
            foreach (AdvancementLogItem _group in this)
            {
                // keep stepping until we find an unlocked group
                if (_group.IsLocked)
                {
                    _locked = _group;
                }
                else
                {
                    break;
                }
            }

            // whatever we picked up last (if anything) is it
            return _locked;
        }
        #endregion

        public AdvancementLogItem FirstUnlockedAdvancementLogItem()
            => _Groups.FirstOrDefault(_g => !_g.IsLocked);

        #region public void LockUpTo(int targetPowerLevel)
        /// <summary>
        /// Attempts to lock the power dice up to the designated power level.  Will stop early if it cannot.
        /// </summary>
        public void LockUpTo(int targetPowerLevel)
        {
            // lowest locked group should be lower than our target
            AdvancementLogItem _unlocked = FirstUnlockedAdvancementLogItem();
            if ((_unlocked != null) && (_unlocked.StartLevel <= targetPowerLevel))
            {
                // get node for the group
                LinkedListNode<AdvancementLogItem> _hdGroupNode = _Groups.Find(_unlocked);
                if (_hdGroupNode != null)
                {
                    do
                    {
                        if (_hdGroupNode.Value.IsLockable)
                        {
                            _hdGroupNode.Value.Lock();
                        }
                        else
                        {
                            // if the node is not lockable, time to stop
                            return;
                        }

                        // step up to next
                        _hdGroupNode = _hdGroupNode.Next;

                        if (_hdGroupNode == null)
                        {
                            // if we walk off the edge of the linked list, time to stop
                            return;
                        }

                        // repeat while the node class level does not exceed the target
                    } while (_hdGroupNode.Value.StartLevel <= targetPowerLevel);
                }
            }
        }
        #endregion

        public int Count => _Groups.Count;

        #region public bool LockNext()
        /// <summary>
        /// Locks next unlocked item (if it is lockable)
        /// </summary>
        /// <returns>true is the lock worked</returns>
        public bool LockNext()
        {
            AdvancementLogItem _unlocked = FirstUnlockedAdvancementLogItem();
            if (_unlocked != null)
            {
                if (_unlocked.IsLockable)
                {
                    _unlocked.Lock();
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region public void UnlockBackTo(int targetPowerLevel)
        /// <summary>
        /// Attempts to roll back to the designated power level.  It may stop early if it cannot roll back.  It may throw an exception.
        /// </summary>
        public void UnlockBackTo(int targetPowerLevel)
        {
            AdvancementLogItem _locked = LastLockedAdvancementLogItem();
            if ((_locked != null) && (_locked.EndLevel > targetPowerLevel))
            {
                // get node for the group
                LinkedListNode<AdvancementLogItem> _hdGroupNode = _Groups.Find(_locked);
                if (_hdGroupNode != null)
                {
                    do
                    {
                        if (_hdGroupNode.Value.IsLocked)
                        {
                            _hdGroupNode.Value.Unlock();
                        }
                        else
                        {
                            // if it is not locked, we have gone far enough
                            return;
                        }

                        // step back to previous
                        _hdGroupNode = _hdGroupNode.Previous;

                        if (_hdGroupNode == null)
                        {
                            // step past front of list means we stop
                            return;
                        }

                        // repeat while the node class level is higher than the target
                    } while (_hdGroupNode.Value.EndLevel > targetPowerLevel);
                }
            }
        }
        #endregion

        #region public void UnlockLast()
        /// <summary>
        /// Unlocks last advancement item that is locked
        /// </summary>
        public void UnlockLast()
        {
            AdvancementLogItem _locked = LastLockedAdvancementLogItem();
            if (_locked != null)
            {
                if (_locked.IsLocked)
                {
                    _locked.Unlock();
                }
            }
        }
        #endregion

        #region public void RemoveTo(int targetPowerLevel)
        /// <summary>
        /// This unlocks (if necessary) the group, and removes it until the last group's EndLevel is lower than the target.
        /// This may remove groups at a time, so if the targetPowerLevel falls in the middle of a group, the whole group is removed.
        /// </summary>
        public void RemoveTo(int targetPowerLevel)
        {
            LinkedListNode<AdvancementLogItem> _last = _Groups.Last;
            while ((_last != null) && (_last.Value.EndLevel > targetPowerLevel))
            {
                AdvancementLogItem _removed = _last.Value;
                _last = _last.Previous;
                RemoveGroup(_removed);
            }
            PowerLevel.BaseValue = NumberPowerDice;
            _HDTallyCtrl.DoValueChanged(new PowerDiceCount(PowerLevel.BaseValue));
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(NumberPowerDice)));
        }
        #endregion

        #region public void RemoveLast()
        /// <summary>This unlocks (if necessary) the last group, and removes it</summary>
        public void RemoveLast()
        {
            LinkedListNode<AdvancementLogItem> _last = _Groups.Last;
            RemoveGroup(_last.Value);
            PowerLevel.BaseValue = NumberPowerDice;
            _HDTallyCtrl.DoValueChanged(new PowerDiceCount(PowerLevel.BaseValue));
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(NumberPowerDice)));
        }
        #endregion

        #region private void RemoveGroup(AdvancementLogItem logItem)
        private void RemoveGroup(AdvancementLogItem logItem)
        {
            // unlock PD Group
            logItem.Unlock();
            for (var _clx = logItem.AdvancementClassLevelHigh; _clx >= logItem.AdvancementClassLevelLow; _clx--)
            {
                // decrease all class levels in this group
                logItem.AdvancementClass.DecreaseLevel();
            }
            logItem.Remove();

            // see if the advancementClass is empty
            if (logItem.AdvancementClass.CanRemove())
            {
                // attempt to unbind the class
                logItem.AdvancementClass.UnbindFromCreature();
            }

            // remove
            _Groups.RemoveLast();
            DoRemoved(logItem);
        }
        #endregion

        #region public int BaseAttackAt(int powerLevel)
        /// <summary>
        /// Calculates what the BaseAttack would be at a particular PD by accumulating the AdvancementClasses in the AdvancementLogItems.
        /// </summary>
        public int BaseAttackAt(int powerLevel)
        {
            // for every advancement class we find, track the last level
            var _maxClassLevel = new Dictionary<AdvancementClass, int>();
            LinkedListNode<AdvancementLogItem> _curr = _Groups.First;
            while ((_curr != null) && (_curr.Value.StartLevel <= powerLevel))
            {
                if (powerLevel <= _curr.Value.EndLevel)
                {
                    // found the last group (assume power die and levels match)
                    var _level = _curr.Value.AdvancementClassLevelLow + (powerLevel - _curr.Value.StartLevel);
                    // fixup levels to agree with group boundaries
                    if (_level > _curr.Value.AdvancementClassLevelHigh)
                    {
                        _level = _curr.Value.AdvancementClassLevelHigh;
                    }

                    if (_level < _curr.Value.AdvancementClassLevelLow)
                    {
                        _level = _curr.Value.AdvancementClassLevelLow;
                    }

                    if (_maxClassLevel.ContainsKey(_curr.Value.AdvancementClass))
                    {
                        // existing class tracked
                        _maxClassLevel[_curr.Value.AdvancementClass] = _level;
                    }
                    else
                    {
                        // new class found
                        _maxClassLevel.Add(_curr.Value.AdvancementClass, _level);
                    }
                    break;
                }
                else
                {
                    if (_maxClassLevel.ContainsKey(_curr.Value.AdvancementClass))
                    {
                        // existing class tracked
                        _maxClassLevel[_curr.Value.AdvancementClass] = _curr.Value.AdvancementClassLevelHigh;
                    }
                    else
                    {
                        // new class found
                        _maxClassLevel.Add(_curr.Value.AdvancementClass, _curr.Value.AdvancementClassLevelHigh);
                    }
                }

                // step to next
                _curr = _curr.Next;
            }

            var _bab = 0;
            foreach (KeyValuePair<AdvancementClass, int> _kvp in _maxClassLevel)
            {
                _bab += _kvp.Key.BaseAttackAt(_kvp.Value);
            }
            return _bab;
        }
        #endregion

        public double SkillRanks(SkillBase skill, int powerLevel)
        {
            var _ranks = 0d;
            LinkedListNode<AdvancementLogItem> _node = _Groups.First;
            while ((_node != null) && (_node.Value.StartLevel <= powerLevel))
            {
                _ranks += _node.Value.SkillRanks(skill);
                _node = _node.Next;
            }
            return _ranks;
        }

        #region ICreatureBound Members
        private Creature _Creature;
        public Creature Creature
        {
            get { return _Creature; }
        }
        #endregion

        #region IEnumerable<AdvancementLogItem> Members
        public IEnumerator<AdvancementLogItem> GetEnumerator()
        {
            foreach (AdvancementLogItem _group in _Groups)
            {
                yield return _group;
            }
            yield break;
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (AdvancementLogItem _group in _Groups)
            {
                yield return _group;
            }
            yield break;
        }
        #endregion

        #region AdvancementLogItemAdded Event
        private void DoAdded(AdvancementLogItem group)
        {
            _AdvLogCtrl.DoValueChanged(group, "Added");
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, group));
        }
        #endregion

        #region AdvancementLogItemRemoved Event
        private void DoRemoved(AdvancementLogItem group)
        {
            _AdvLogCtrl.DoValueChanged(group, "Removed");
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, group, _Groups.Count));
        }
        #endregion

        #region IControlChange<PowerDiceCount> Members
        private ChangeController<PowerDiceCount> _HDTallyCtrl;
        public void AddChangeMonitor(IMonitorChange<PowerDiceCount> subscriber)
        {
            _HDTallyCtrl.AddChangeMonitor(subscriber);
        }

        public void RemoveChangeMonitor(IMonitorChange<PowerDiceCount> subscriber)
        {
            _HDTallyCtrl.RemoveChangeMonitor(subscriber);
        }
        #endregion

        #region IControlChange<AdvancementLogItem> Members
        private ChangeController<AdvancementLogItem> _AdvLogCtrl;
        /// <summary>
        /// Actions = { Added | Removed }
        /// </summary>
        public void AddChangeMonitor(IMonitorChange<AdvancementLogItem> subscriber)
        {
            _AdvLogCtrl.AddChangeMonitor(subscriber);
        }

        public void RemoveChangeMonitor(IMonitorChange<AdvancementLogItem> subscriber)
        {
            _AdvLogCtrl.RemoveChangeMonitor(subscriber);
        }

        #endregion

        [field: NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        [field: NonSerialized, JsonIgnore]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #region IControlChange<Activation> Members
        public void AddChangeMonitor(IMonitorChange<Activation> monitor) { }
        public void RemoveChangeMonitor(IMonitorChange<Activation> monitor) { }
        #endregion
    }
}
