using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class ObjectGrabbedPivotData : InteractData
    {
        public ObjectGrabbedPivotData(CoreActor actor)
            : base(actor)
        {
        }

        #region static
        private readonly static IInteractHandler _Static = new ObjectGrabbedPivotHandler();
        #endregion

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Static;
            yield break;
        }

        public static int GetPivots(ICoreObject coreObj)
        {
            var _actor = coreObj as CoreActor;
            var _ogcd = new ObjectGrabbedPivotData(_actor);
            var _workSet = new Interaction(_actor, _actor, coreObj, _ogcd);
            coreObj?.HandleInteraction(_workSet);
            return _workSet.Feedback.OfType<ValueFeedback<int>>().FirstOrDefault()?.Value ?? 1;
        }
    }
}
