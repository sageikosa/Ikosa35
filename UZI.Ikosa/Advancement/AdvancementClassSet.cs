using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Skills;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class AdvancementClassSet : IEnumerable<IAdvancementClass>, ICreatureBound, IControlChange<IAdvancementClass>, INotifyCollectionChanged
    {
        #region Constructon
        public AdvancementClassSet(Creature creature)
        {
            _Creature = creature;
            _Classes = new Collection<IAdvancementClass>();
            _AdvClassCtrlr = new ChangeController<IAdvancementClass>(this, null);
        }
        #endregion

        #region data
        private Creature _Creature;
        private Collection<IAdvancementClass> _Classes;
        private ChangeController<IAdvancementClass> _AdvClassCtrlr;
        #endregion

        public virtual bool IsClassSkill(SkillBase skill)
            => _Classes.Any(_c => _c.IsClassSkill(skill));

        public Creature Creature => _Creature;
        public int Count => _Classes.Count;
        public IAdvancementClass this[int index] => _Classes[index];

        public IAdvClass Get<IAdvClass>() where IAdvClass : IAdvancementClass
            => _Classes
            .OfType<IAdvClass>()
            .FirstOrDefault();

        public IAdvClass Get<IAdvClass>(Type type) where IAdvClass : IAdvancementClass
            => _Classes
            .OfType<IAdvClass>()
            .Where(_c => _c.GetType() == type)
            .FirstOrDefault();

        /// <summary>Sum of all character class levels</summary>
        public int CharacterLevel =>
            (from _c in _Classes.OfType<CharacterClass>()
             select _c.CurrentLevel)
            .Sum();

        #region public string PowerLevelSummary()
        public string PowerLevelSummary()
        {
            var _output = new StringBuilder();
            foreach (var _advClass in _Classes)
            {
                _output.Append($@"{(_output.Length > 0 ? @" + " : string.Empty)}{_advClass.CurrentLevel}d{_advClass.PowerDieSize}");
            }
            return _output.ToString();
        }
        #endregion

        public bool Contains(Type advClassType)
            => _Classes.Any(_c => _c.GetType() == advClassType);

        #region public bool CanAdd(IAdvancementClass advClass)
        /// <summary>Used to ask the collection whether a new class can be added.  This in turn fires an event.</summary>
        public bool CanAdd(IAdvancementClass advClass)
        {
            // check if the class is already present in some form
            if (Contains(advClass.GetType()))
                return false;

            // see if anything is going to object to the class
            return !_AdvClassCtrlr.WillAbortChange(advClass, @"PreAdd");
        }
        #endregion

        #region internal void Add(IAdvancementClass advClass)
        internal void Add(IAdvancementClass advClass)
        {
            _Classes.Add(advClass);
            _AdvClassCtrlr.DoValueChanged(advClass, @"Added");
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, advClass));
        }
        #endregion

        #region internal void Remove(IAdvancementClass advClass)
        internal void Remove(IAdvancementClass advClass)
        {
            if (advClass.CanRemove())
            {
                var _index = _Classes.IndexOf(advClass);
                _Classes.Remove(advClass);
                _AdvClassCtrlr.DoValueChanged(advClass, @"Removed");
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, advClass, _index));
            }
        }
        #endregion

        /// <summary>Ensures nothing is going to veto the change in level</summary>
        public bool CanIncrease(IAdvancementClass advClass)
            => !_AdvClassCtrlr.WillAbortChange(advClass, @"PreIncrease");

        #region public bool CanDecrease(IAdvancementClass advClass)
        /// <summary>Top PowerDie must be for this advancement class, and nothing must veto.</summary>
        public bool CanDecrease(IAdvancementClass advClass)
        {
            if (_Classes.Count > 0)
            {
                // must be at top of PD list
                var _pd = Creature.AdvancementLog[Creature.AdvancementLog.NumberPowerDice];
                if (_pd != null)
                {
                    if (!_pd.AdvancementClass.Equals(advClass))
                    {
                        // something is stacked on top
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // nothing there!
                return false;
            }

            return !_AdvClassCtrlr.WillAbortChange(advClass, @"PreDecrease");
        }
        #endregion

        #region IEnumerable<IAdvancementClass> Members
        public IEnumerator<IAdvancementClass> GetEnumerator()
        {
            return _Classes.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _Classes.GetEnumerator();
        }
        #endregion

        #region IControlChange<IAdvancementClass> Members
        // replaces AdvancementClass Add and Remove
        /// <summary>
        /// Abortable = PreIncrease | PreAdded | PreDecrease
        /// Actions = Added | Removed
        /// </summary>
        void IControlChange<IAdvancementClass>.AddChangeMonitor(IMonitorChange<IAdvancementClass> subscriber)
        {
            _AdvClassCtrlr.AddChangeMonitor(subscriber);
        }

        void IControlChange<IAdvancementClass>.RemoveChangeMonitor(IMonitorChange<IAdvancementClass> subscriber)
        {
            _AdvClassCtrlr.RemoveChangeMonitor(subscriber);
        }
        #endregion

        [field: NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        [field: NonSerialized, JsonIgnore]
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}