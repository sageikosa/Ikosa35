using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class RecoverHealthPointData : InteractData
    {
        public RecoverHealthPointData(CoreActor actor, Deltable amount, bool nonLethalOnly, bool nonLiving)
            : base(actor)
        {
            _Amount = amount;
            _NonLethalOnly = nonLethalOnly;
            _NonLiving = nonLiving;
        }

        #region Private Data
        private Deltable _Amount;
        private bool _NonLethalOnly;
        private bool _NonLiving;
        #endregion

        public Deltable Amount { get { return _Amount; } }
        public bool NonLethalOnly { get { return _NonLethalOnly; } }

        /// <summary>Indicates this is a recovery due for a non-living creature</summary>
        public bool NonLivingRecovery { get { return _NonLiving; } }

        private static CreatureRecoverHealthPointHandler _Static = new CreatureRecoverHealthPointHandler();

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Static;
            yield break;
        }
    }
}
