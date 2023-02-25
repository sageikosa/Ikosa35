using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class ObjectGrabbedPivotAllowance : Adjunct, IInteractHandler
    {
        public ObjectGrabbedPivotAllowance(object source, int allowance)
            : base(source)
        {
            _Allow = allowance;
        }

        #region data
        private int _Allow;
        #endregion

        public int Allowance => _Allow;

        public override object Clone()
            => new ObjectGrabbedPivotAllowance(Source, Allowance);


        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is ObjectGrabbedPivotData _ogpd)
                workSet.Feedback.Add(new ValueFeedback<int>(this, Allowance));
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(ObjectGrabbedPivotData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return true;
        }
    }
}
