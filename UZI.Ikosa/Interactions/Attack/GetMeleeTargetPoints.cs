using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Interactions
{
    public class GetMeleeTargetPoints : InteractData
    {
        public GetMeleeTargetPoints(CoreActor actor, ICellLocation sourceCell, ICellLocation targetCell, bool downWard, AnchorFace downFace)
            : base(actor)
        {
            SourceCell = sourceCell;
            TargetCell = targetCell;
            DownWard = downWard;
            DownFace = downFace;
        }

        public ICellLocation SourceCell { get; private set; }
        public ICellLocation TargetCell { get; private set; }
        public bool DownWard { get; private set; }
        public AnchorFace DownFace { get; private set; }

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return PointsHandler.Static;
            yield break;
        }

        /// <summary>Performs interaction to get points and returns feedback</summary>
        public static GetPointsFeedback GetPoints(MeleeAttackData attack, IInteract target, bool downWard, AnchorFace downFace)
        {
            var _gmtp = new GetMeleeTargetPoints(attack.Attacker, attack.SourceCell, attack.TargetCell, downWard, downFace);
            var _gmtpi = new Interaction(attack.Attacker, attack, target, _gmtp);
            target.HandleInteraction(_gmtpi);
            return _gmtpi.Feedback.OfType<GetPointsFeedback>().FirstOrDefault();
        }
    }
}
