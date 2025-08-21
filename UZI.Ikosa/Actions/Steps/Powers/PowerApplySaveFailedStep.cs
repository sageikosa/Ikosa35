using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class PowerApplySaveFailedStep : PreReqListStepBase
    {
        public PowerApplySaveFailedStep(CoreStep predecessor, CoreActor actor, object source, List<IProvideSaves> candidates,
            SaveType saveType, SaveEffect saveEffect, DeltaCalcInfo difficulty, double saveFactor, List<DamageData> damages)
            : base(predecessor)
        {
            _Actor = actor;
            _Source = source;
            _Candidates = candidates;
            _SaveMode = new SaveMode(saveType, saveEffect, difficulty, false);
            _SFactor = saveFactor;
            _Dmgs = damages;

            _PendingPreRequisites.Enqueue(new CoreSelectPrerequisite(this, @"Item", @"Potential Damage",
                _Candidates.Select(_c => _c.ID).ToList(), false));
            _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Roll", @"Saving Roll", new DieRoller(20), false));
        }

        #region data
        private CoreActor _Actor;
        private object _Source;
        private List<IProvideSaves> _Candidates;
        private SaveMode _SaveMode;
        private double _SFactor;
        private List<DamageData> _Dmgs;
        #endregion

        public CoreActor Actor => _Actor;
        public object Source => _Source;
        public List<IProvideSaves> Candidates => _Candidates;
        public SaveMode SaveMode => _SaveMode;
        public double SaveFactor => _SFactor;
        public List<DamageData> Damages => _Dmgs;

        public CoreSelectPrerequisite CoreSelectPrerequisite => _PendingPreRequisites.OfType<CoreSelectPrerequisite>().FirstOrDefault();
        public RollPrerequisite RollPrerequisite => _PendingPreRequisites.OfType<RollPrerequisite>().FirstOrDefault();

        protected override bool OnDoStep()
        {
            var _target = Candidates.FirstOrDefault(_c => _c.ID == CoreSelectPrerequisite.Selected.Value);
            var _roll = new Deltable(RollPrerequisite.RollValue);

            // create saving throw data with the roll
            var _saveDamage = new SaveableDamageData(Actor as Creature, Damages, SaveMode, SaveFactor,
                _roll, false, false);

            // let target's handlers alter the roll if possible
            var _saveDmgInteract = new StepInteraction(this, Actor, Source, _target, _saveDamage);
            _target.HandleInteraction(_saveDmgInteract);
            if (_saveDmgInteract.Feedback.OfType<PrerequisiteFeedback>().Any())
            {
                // TODO: building a list to send data?
                var _list = new List<Guid>();
                if (Actor != null)
                {
                    _list.Add(Actor.ID);
                }

                if (_target is Creature _tCritter)
                {
                    _list.Add(_tCritter.ID);
                }

                new RetryInteractionStep(this, @"Retry", _saveDmgInteract);
            }
            return true;
        }
    }
}
