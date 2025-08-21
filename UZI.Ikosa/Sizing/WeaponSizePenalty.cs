using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa
{
    [Serializable]
    public class WeaponSizePenalty : IQualifyDelta, ICreatureBound
    {
        public WeaponSizePenalty(Creature critter)
        {
            _Critter = critter;
            _Terminator = new TerminateController(this);
        }

        #region data
        private readonly TerminateController _Terminator;
        private Creature _Critter;
        #endregion

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  
        /// Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Terminator.DoTerminate();
        }

        #region IControlTerminate Members
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
        #endregion
        #endregion

        public Creature Creature => _Critter;

        public virtual IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (!(qualify?.Source is IWeaponHead _wpnHead))
            {
                yield break;
            }

            var _critterSize = Creature.Sizer.Size.Order;
            var _itemSize = _wpnHead.ContainingWeapon.ItemSizer.EffectiveCreatureSize.Order;
            var _steps = Math.Abs(_itemSize - _critterSize);

            if (_steps > 0)
            {
                yield return new QualifyingDelta(-2 * _steps, typeof(WeaponSizePenalty), $@"Weapon Size: {_steps} Step Difference");
            }

            yield break;
        }
    }
}
