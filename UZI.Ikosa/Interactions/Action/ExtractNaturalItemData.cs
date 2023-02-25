using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Interactions.Action
{
    [Serializable]
    public class ExtractNaturalItemData : InteractData
    {
        public ExtractNaturalItemData(CoreActor actor, NaturalItemTrait naturalItemTrait)
            : base(actor)
        {
            _Trait = naturalItemTrait;
        }

        #region state
        protected readonly NaturalItemTrait _Trait;
        #endregion

        public NaturalItemTrait NaturalItemTrait => _Trait;

        private static ExtractNaturalItemHandler _Static => new ExtractNaturalItemHandler();

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
            => _Static.ToEnumerable();
    }
}
