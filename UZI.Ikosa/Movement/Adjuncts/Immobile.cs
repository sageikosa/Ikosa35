using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Movement
{
    /// <summary>
    /// Cannot be picked up, cannot relocate, and block rotations.
    /// </summary>
    [Serializable]
    public class Immobile : Adjunct, IPathDependent
    {
        /// <summary>
        /// Cannot be picked up, cannot relocate, and block rotations
        /// </summary>
        public Immobile(ICore immobilizer)
            : base(immobilizer)
        {
            _PBlock = new BlockInteraction<PickUp>();
            _FRBlock = new BlockInteraction<ObjectManipulateData>();
        }

        #region data
        private BlockInteraction<PickUp> _PBlock;
        private BlockInteraction<ObjectManipulateData> _FRBlock;
        #endregion

        public ICore Immobilizer
            => Source as ICore;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as CoreObject)?.AddIInteractHandler(_PBlock);
            (Anchor as CoreObject)?.AddIInteractHandler(_FRBlock);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as CoreObject)?.RemoveIInteractHandler(_FRBlock);
            (Anchor as CoreObject)?.RemoveIInteractHandler(_PBlock);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new Immobile(Immobilizer);

        public void PathChanged(Pathed source)
        {
            Eject();
        }
    }
}
