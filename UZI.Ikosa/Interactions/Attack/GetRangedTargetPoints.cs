using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Interactions
{
    public class GetRangedTargetPoints : InteractData
    {
        public GetRangedTargetPoints(CoreActor actor, ICellLocation sourceCell)
            : base(actor)
        {
            SourceCell = sourceCell;
        }

        public ICellLocation SourceCell { get; private set; }

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return PointsHandler.Static;
            yield break;
        }

        /// <summary>Performs interaction to get points and returns feedback</summary>
        public static GetPointsFeedback GetPoints(ReachAttackData attack, IInteract target)
        {
            // points picked from locator core
            var _grtp = new GetRangedTargetPoints(attack.Attacker, attack.SourceCell);
            var _grtpi = new Interaction(attack.Attacker, attack, target, _grtp);
            target.HandleInteraction(_grtpi);
            return _grtpi.Feedback.OfType<GetPointsFeedback>().FirstOrDefault();
        }
    }
}
