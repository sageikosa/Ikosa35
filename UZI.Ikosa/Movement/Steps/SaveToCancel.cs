using Uzi.Core.Contracts;
using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class SaveToCancel : PreReqListStepBase
    {
        public SaveToCancel(CoreProcess process, Creature creature, SaveType saveType, DeltaCalcInfo difficulty)
            : base(process)
        {
            _PendingPreRequisites.Enqueue(new SavePrerequisite(this, new Qualifier(null, this, creature),
                @"Save.Cancel", @"Save", new SaveMode(saveType, SaveEffect.Negates, difficulty)));
        }

        protected override bool OnDoStep()
        {
            var _save = DispensedPrerequisites.OfType<SavePrerequisite>().FirstOrDefault();
            if ((_save != null) && _save.IsReady && _save.Success)
            {
                AppendFollowing(new TerminationStep(Process, new CheckResultNotify(_save.Qualification.Target.ID, @"Save", true,
                    new Info { Message = @"Saved to Cancel Activity" }))
                {
                    InfoReceivers = new[] { _save.Qualification.Target.ID }
                });
            }
            return true;
        }
    }
}
