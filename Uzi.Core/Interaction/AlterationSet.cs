using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>
    /// Allows for enhanced interaction alteration support, including contains(type), fetch(type), and add filtering via actor and target filters...
    /// </summary>
    [Serializable]
    public class AlterationSet : IEnumerable<InteractionAlteration>, IControlChange<InteractionAlteration>
    {
        public const string ActorAction = @"Actor";
        public const string TargetAction = @"Target";
        /// <summary>
        /// Allows for enhanced interaction alteration support, including contains(type), fetch(type), and add filtering via actor and target filters...
        /// </summary>
        public AlterationSet(CoreActor actor)
        {
            _Alterations = new List<InteractionAlteration>();
            _ChangeCtrl = new ChangeController<InteractionAlteration>(this, null);
            if (actor != null)
            {
                // chain this instance to the actor's interaction alteration controller rules
                _ChangeCtrl.AddChangeController(actor.InteractAlterationController);
            }
        }

        private List<InteractionAlteration> _Alterations;

        // change controller from the actor (if available)
        private ChangeController<InteractionAlteration> _ChangeCtrl;

        /// <summary>
        /// Adds the alteration, but only signals the change
        /// </summary>
        /// <param name="target"></param>
        /// <param name="alteration"></param>
        public void Add(IInteract target, InteractionAlteration alteration)
        {
            // see if actor alteration filters limit the alteration
            if (!(_ChangeCtrl?.WillAbortChange(alteration, ActorAction) ?? false))
            {
                // see if target alteration filters limit the alteration
                if (!((target as CoreActor)
                    ?.InteractAlterationController
                    ?.WillAbortChange(alteration, TargetAction) ?? false))
                {
                    _ChangeCtrl.DoPreValueChanged(alteration);
                    _Alterations.Add(alteration);
                    _ChangeCtrl.DoValueChanged(alteration);
                }
            }
        }

        /// <summary>Indicates the set contains at least one instance of the selected type</summary>
        public bool Contains(Type type)
            => this.Any(_a => _a.GetType().Equals(type));

        public InteractionAlteration this[Type type]
            => _Alterations.FirstOrDefault(alter => alter.GetType() == type);

        // IEnumerable<InteractionAlteration> Members
        public IEnumerator<InteractionAlteration> GetEnumerator()
            => _Alterations.GetEnumerator();

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _Alterations.GetEnumerator();
        }
        #endregion

        #region IControlChange<InteractionAlteration> Members
        public void AddChangeMonitor(IMonitorChange<InteractionAlteration> monitor)
        {
            _ChangeCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<InteractionAlteration> monitor)
        {
            _ChangeCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        #region INotifyPropertyChanged Members
        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
