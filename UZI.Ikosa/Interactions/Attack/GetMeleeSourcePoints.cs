using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Interactions
{
    public class GetMeleeSourcePoints : InteractData
    {
        public GetMeleeSourcePoints(CoreActor actor, ICellLocation sourceCell, ICellLocation targetCell)
            : base(actor)
        {
            SourceCell = sourceCell;
            TargetCell = targetCell;
        }

        public ICellLocation SourceCell { get; private set; }
        public ICellLocation TargetCell { get; private set; }

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return PointsHandler.Static;
            yield break;
        }

        /// <summary>Performs interaction to get points and returns feedback</summary>
        public static GetPointsFeedback GetPoints(MeleeAttackData attack)
        {
            // points picked from locator core
            var _ptSource = attack.AttackLocator.ICore as IInteract;
            var _gmsp = new GetMeleeSourcePoints(attack.Attacker, attack.SourceCell, attack.TargetCell);
            var _gmspi = new Interaction(attack.Attacker, attack, _ptSource, _gmsp);
            _ptSource.HandleInteraction(_gmspi);
            return _gmspi.Feedback.OfType<GetPointsFeedback>().FirstOrDefault();
        }
    }
}
