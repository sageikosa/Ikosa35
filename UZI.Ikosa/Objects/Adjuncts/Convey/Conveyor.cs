using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    /// <summary>Representees a conveyance</summary>
    [Serializable]
    public class Conveyor : GroupMasterAdjunct, IProcessFeedback // TODO: IActionProvider???
    {
        /// <summary>Representees a conveyance</summary>
        public Conveyor(ConveyanceGroup conveyance)
            : base(typeof(ConveyanceGroup), conveyance)
        {
        }

        public ConveyanceGroup ConveyanceGroup => Group as ConveyanceGroup;
        public ICoreObject CoreObject => Anchor as ICoreObject;

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is ICoreObject) && base.CanAnchor(newAnchor);

        public override object Clone()
            => new Conveyor(ConveyanceGroup);

        protected override void OnActivate(object source)
        {
            CoreObject?.AddIInteractHandler(this);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            CoreObject?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }

        #region IProcessFeedback
        public void ProcessFeedback(Interaction workSet)
        {
            // successfully added this locator to the gathering set?
            if (workSet.InteractData is GetLocatorsData _gld
                && workSet.Feedback.OfType<ValueFeedback<bool>>().Any(_vfb => _vfb.Value))
            {
                // if any were retrieved, then add the object also...
                var _move = workSet.Source as MovementBase;
                Parallel.ForEach(
                    ConveyanceGroup?.Passengers.Select(_p => _p.CoreObject),
                    (_crt => _gld.AddObject(_crt, _move)));

                // movement sourced from something else increases cost
                if (_move.CoreObject != CoreObject)
                {
                    _gld.SetCost(CoreObject, 1);
                }
            }
        }

        public void HandleInteraction(Interaction workSet)
        {
            // FEEDBACK ONLY!
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetLocatorsData);
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => (interactType == typeof(GetLocatorsData))
            && (existingHandler is GetLocatorsHandler);

        #endregion
    }
}
