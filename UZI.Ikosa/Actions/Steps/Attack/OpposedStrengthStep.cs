using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class OpposedStrengthStep : PreReqListStepBase
    {
        public OpposedStrengthStep(CoreStep predecessor, Creature actor, Creature opposer,
            CoreStep actorWins, CoreStep opposerWins)
            : base(predecessor)
        {
            _Actor = actor;
            _Opposer = opposer;
            _PendingPreRequisites.Enqueue(new RollPrerequisite(this, new Qualifier(Actor, this, Opposer), Actor,
                @"Strength.Actor", @"Strength check to move", new DieRoller(20), false));
            _PendingPreRequisites.Enqueue(new RollPrerequisite(this, new Qualifier(Opposer, this, Actor), Opposer,
                @"Strength.Opposer", @"Strength check to resist move", new DieRoller(20), false));
            _ActorWins = actorWins;
            _OpposerWins = opposerWins;
        }

        public OpposedStrengthStep(CoreProcess process, Creature actor, Creature opposer,
            CoreStep actorWins, CoreStep opposerWins)
            : base(process)
        {
            _Actor = actor;
            _Opposer = opposer;
            _PendingPreRequisites.Enqueue(new RollPrerequisite(this, new Qualifier(Actor, this, Opposer), Actor,
                @"Strength.Actor", @"Strength check to move", new DieRoller(20), false));
            _PendingPreRequisites.Enqueue(new RollPrerequisite(this, new Qualifier(Opposer, this, Actor), Opposer,
                @"Strength.Opposer", @"Strength check to resist move", new DieRoller(20), false));
            _ActorWins = actorWins;
            _OpposerWins = opposerWins;
        }

        #region data
        private Creature _Actor;
        private Creature _Opposer;
        private CoreStep _ActorWins;
        private CoreStep _OpposerWins;
        #endregion

        public Creature Actor => _Actor;
        public Creature Opposer => _Opposer;

        protected override bool OnDoStep()
        {
            if (IsComplete)
                return true;

            var _actRoll = AllPrerequisites<RollPrerequisite>(@"Strength.Actor").FirstOrDefault();
            var _actScore = _actRoll.RollValue + Actor.Abilities.Strength.QualifiedDeltas(_actRoll.Qualification).Sum(_d => _d.Value);

            var _oppRoll = AllPrerequisites<RollPrerequisite>(@"Strength.Opposer").FirstOrDefault();
            var _oppScore = _oppRoll.RollValue + Opposer.Abilities.Strength.QualifiedDeltas(_oppRoll.Qualification).Sum(_d => _d.Value);

            for (var _cx = 10; _cx > 0; _cx--)
            {
                if (_actScore > _oppScore)
                {
                    // actor wins
                    if (_ActorWins != null)
                        AppendFollowing(_ActorWins);
                    return true;
                }
                else if (_oppScore > _actScore)
                {
                    // opposer wins (abort activity)
                    if (_OpposerWins != null)
                        AppendFollowing(_OpposerWins);
                    return true;
                }
                else
                {
                    // random roll off?
                    var _d20 = new DieRoller(20);
                    _actScore = _d20.RollValue(Actor.ID, @"Strength", $@"Roll-Off {_cx}", Actor.ID);
                    _oppScore = _d20.RollValue(Opposer.ID, @"Strength", $@"Roll-Off {_cx}", Opposer.ID);
                }
            }

            // since no one won, just let the actor win
            AppendFollowing(_ActorWins);
            return true;
        }
    }
}
