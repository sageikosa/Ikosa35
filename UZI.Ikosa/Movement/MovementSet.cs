using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Uzi.Core;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class MovementSet : ICreatureBound, IControlChange<MovementBase>, IDeserializationCallback
    {
        public MovementSet(Creature critter)
        {
            _Critter = critter;
            _Movements = new Collection<MovementBase>();
            _MCtrl = new ChangeController<MovementBase>(this, null);
        }

        #region state
        private readonly Creature _Critter;
        private readonly Collection<MovementBase> _Movements;
        private ChangeController<MovementBase> _MCtrl;
        #endregion

        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        #region ICreatureBound Members
        public Creature Creature { get { return _Critter; } }
        #endregion

        public void Add(MovementBase movement)
        {
            if (!_Movements.Contains(movement))
            {
                _Movements.Add(movement);
                _MCtrl.DoValueChanged(movement, @"Add");
                Creature.Actions.Providers.Add(movement, movement);
            }
        }

        public void Remove(MovementBase movement)
        {
            if (_Movements.Contains(movement))
            {
                // TODO: movement should probably get a chance to do something when it ends
                _Movements.Remove(movement);
                _MCtrl.DoValueChanged(movement, @"Remove");
                Creature.Actions.Providers.Remove(movement);
            }
        }

        public IEnumerable<MovementBase> AllMovements
            => _Movements.Select(_m => _m);

        public void AddChangeMonitor(IMonitorChange<MovementBase> monitor)
            => _MCtrl.AddChangeMonitor(monitor);

        public void RemoveChangeMonitor(IMonitorChange<MovementBase> monitor)
            => _MCtrl.RemoveChangeMonitor(monitor);

        public void OnDeserialization(object sender)
            => _MCtrl ??= new ChangeController<MovementBase>(this, null);
    }
}
