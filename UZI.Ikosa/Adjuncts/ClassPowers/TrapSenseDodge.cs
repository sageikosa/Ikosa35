using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class TrapSenseDodge : Adjunct, IQualifyDelta
    {
        public TrapSenseDodge(IPowerClass powerClass)
            : base(powerClass)
        {
            _Terminator = new TerminateController(this);
            _Delta = new Delta((powerClass?.ClassPowerLevel.EffectiveValue ?? 3) / 3, this, @"Trap Dodge");
        }

        #region data
        private Delta _Delta;
        private readonly TerminateController _Terminator;
        #endregion

        public IPowerClass PowerClass
            => Source as IPowerClass;

        private Creature Critter
            => Anchor as Creature;

        #region OnActivate()
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Critter?.NormalArmorRating.Deltas.Add(this);
            Critter?.TouchArmorRating.Deltas.Add(this);
        }
        #endregion

        #region OnDeactivate()
        protected override void OnDeactivate(object source)
        {
            // terminate boost
            DoTerminate();
            base.OnDeactivate(source);
        }
        #endregion

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // actionless attack is a trap
            if ((qualify is Interaction _atkInteract)
                && (_atkInteract.InteractData as AttackData)?.Action == null)
            {
                if (Critter?.CanDodge(_atkInteract) ?? false)
                {
                    _Delta.Value = (PowerClass?.ClassPowerLevel.EffectiveValue ?? 3) / 3;
                    yield return _Delta;
                }
            }
            yield break;
        }

        public override object Clone()
            => new TrapSenseDodge(PowerClass);

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
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
    }

}
