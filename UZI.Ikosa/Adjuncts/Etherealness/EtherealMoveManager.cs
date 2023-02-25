using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class EtherealMoveManager : Adjunct, IMonitorChange<DeltaValue>, IMonitorChange<MovementBase>
    {
        public EtherealMoveManager(EtherealState ethereal)
            : base(ethereal)
        {
        }

        public EtherealState Ethereal => Source as EtherealState;
        public Creature Creature => Anchor as Creature;

        public override object Clone()
            => new EtherealMoveManager(Ethereal);

        public void PreTestChange(object sender, AbortableChangeEventArgs<MovementBase> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<MovementBase> args) { }
        public void ValueChanged(object sender, ChangeValueEventArgs<MovementBase> args) => AssignSpeed();

        public void PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args) { }
        public void ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args) => AssignSpeed();

        /// <summary>Get best non-ethereal movement speed</summary>
        private int GetSpeed()
            => Creature.Movements.AllMovements
            .Where(_m => _m.Source != this)
            .OrderByDescending(_m => _m.BaseValue)
            .FirstOrDefault()?
            .BaseValue ?? 0;

        /// <summary>Sync best non-ethereal movement speed to existing movements</summary>
        private void AssignSpeed()
        {
            // NOTE: called whenever movements change
            var _move = Creature.Movements.AllMovements.OfType<EtherealMovement>().FirstOrDefault(_em => _em.Source == this);
            if (_move != null)
            {
                _move.BaseValue = GetSpeed();
            }
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);

            // add movement, then start monitoring
            Creature.Movements.Add(new EtherealMovement(GetSpeed(), Creature, this));
            Creature.Movements.AddChangeMonitor(this);
        }

        protected override void OnDeactivate(object source)
        {
            // stop monitoring, then remove movement
            Creature.Movements.RemoveChangeMonitor(this);
            Creature.Movements.Remove(
                Creature.Movements.AllMovements.OfType<EtherealMovement>().FirstOrDefault(_em => _em.Source == this));
            base.OnDeactivate(source);
        }
    }
}
