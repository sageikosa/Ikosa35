using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Feats
{
    [Serializable]
    public class CleaveAttackCheck : PreReqListStepBase
    {
        public CleaveAttackCheck(CoreStep step, CleaveReactor reactor, List<CoreActor> targets)
            : base(step)
        {
            _Reactor = reactor;
            _Targets = targets;

            // allow no selection
            var _targets = _Targets.Select(_c => _c.ID).ToList();
            _targets.Insert(0, Guid.Empty);

            var _critter = _Reactor.CleaveFeat.Creature;
            _PendingPreRequisites.Enqueue(
                new CoreSelectPrerequisite(this, _critter, null, null, nameof(CleaveAttackCheck), @"Cleave Selection", _targets, true));
        }

        #region data
        private CleaveReactor _Reactor;
        private List<CoreActor> _Targets;
        #endregion

        public CoreSelectPrerequisite CoreSelect
            => GetPrerequisite<CoreSelectPrerequisite>();

        protected override bool OnDoStep()
        {
            if (CoreSelect.Selected != Guid.Empty)
            {
                // target
                var _target = _Targets.FirstOrDefault(_t => _t.ID == CoreSelect.Selected);
                if (_target != null)
                {
                    AppendFollowing(new CleaveAttackRoll(this, _Reactor, _target));
                }
            }
            return true;
        }
    }
}
