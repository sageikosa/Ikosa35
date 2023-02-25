using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class ObjectGrabbedCostData : InteractData
    {
        public ObjectGrabbedCostData(CoreActor actor, AnchorFaceList crossings) 
            : base(actor)
        {
            _Crossings = crossings;
        }

        #region data
        private AnchorFaceList _Crossings;
        #endregion

        public AnchorFaceList Crossings => _Crossings;

        #region static
        private readonly static IInteractHandler _Static = new ObjectGrabbedCostHandler();
        #endregion

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Static;
            yield break;
        }

        public static double GetCost(ICoreObject coreObj, MovementBase movement, AnchorFaceList crossings)
        {
            var _actor = coreObj as CoreActor;
            var _ogcd = new ObjectGrabbedCostData(_actor, crossings);
            var _workSet = new Interaction(_actor, movement, coreObj, _ogcd);
            coreObj?.HandleInteraction(_workSet);
            return _workSet.Feedback.OfType<ValueFeedback<double>>().FirstOrDefault()?.Value ?? 1.5d;
        }
    }
}
