using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class ObjectGrabbedManeuverabilityCost : Adjunct, IInteractHandler
    {
        public ObjectGrabbedManeuverabilityCost(object source, double factor, params Type[] movementTypes)
            : base(source)
        {
            _Factor = factor;
            _Movements = movementTypes.ToList();
        }

        #region data
        private double _Factor;
        private List<Type> _Movements;
        #endregion

        public double Factor => _Factor;
        public IEnumerable<Type> MovementTypes => _Movements.Select(_m => _m);

        public override object Clone()
            => new ObjectGrabbedManeuverabilityCost(Source, Factor, MovementTypes.ToArray());

        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet.InteractData is ObjectGrabbedCostData _ogcd)
                && _Movements.Contains(workSet.Source.GetType()))
                workSet.Feedback.Add(new ValueFeedback<double>(this, Factor));
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(ObjectGrabbedCostData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return true;
        }
    }
}
