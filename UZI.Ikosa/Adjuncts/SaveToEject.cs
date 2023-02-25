using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class SaveToEject : PreReqListStepBase
    {
        public SaveToEject(object source, Creature creature, SaveType saveType, DeltaCalcInfo difficulty, Adjunct adjunct,
            string description)
            : base((CoreProcess)null)
        {
            _Adjunct = adjunct;
            _Description = description;
            _PendingPreRequisites.Enqueue(new SavePrerequisite(this, new Qualifier(creature, source, creature),
                @"Save.Eject", @"Save", new SaveMode(saveType, SaveEffect.Negates, difficulty)));
        }

        #region state
        private Adjunct _Adjunct;
        private string _Description;
        #endregion

        public string Description => _Description;

        protected override bool OnDoStep()
        {
            var _save = DispensedPrerequisites.OfType<SavePrerequisite>().FirstOrDefault();
            if (_save?.IsReady ?? false)
            {
                if (_save.Success)
                {
                    _Adjunct?.Eject();

                    EnqueueNotify(new CheckResultNotify(_save.Qualification.Target.ID, @"Save", true,
                        new Info { Message = _Description }), _save.Qualification.Target.ID);
                }
                else
                {
                    EnqueueNotify(new CheckResultNotify(_save.Qualification.Target.ID, @"Save", false,
                        new Info { Message = _Description }), _save.Qualification.Target.ID);
                }
            }
            return true;
        }
    }
}
