using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Reflex saves against traps</summary>
    [Serializable]
    public class TrapSenseSave : Adjunct, IQualifyDelta
    {
        /// <summary>Reflex saves against traps</summary>
        public TrapSenseSave(IPowerClass powerClass)
            : base(powerClass)
        {
            _Terminator = new TerminateController(this);
            _Delta = new Delta((powerClass?.ClassPowerLevel.EffectiveValue ?? 3) / 3, this, @"Trap Sense");
        }

        #region data
        private Delta _Delta;
        private readonly TerminateController _Terminator;
        #endregion

        public IPowerClass PowerClass
            => Source as IPowerClass;

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if ((qualify?.Source is IAdjunctable _adj)
                && _adj.Adjuncts.OfType<TrapPart>().Any())
            {
                _Delta.Value = (PowerClass?.ClassPowerLevel.EffectiveValue ?? 3) / 3;
                yield return _Delta;
            }
            yield break;
        }

        private Creature Critter
            => Anchor as Creature;

        #region OnActivate()
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Critter?.ReflexSave.Deltas.Add(this);
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

        public override object Clone()
            => new TrapSenseSave(PowerClass);

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
