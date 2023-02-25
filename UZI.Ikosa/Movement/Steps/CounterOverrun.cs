using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class CounterOverrun : PreReqListStepBase
    {
        public CounterOverrun(CoreStep predecessor, Creature pusher, Creature counterer)
            : base(predecessor)
        {
            _Pusher = pusher;
            _Counterer = counterer;
            _PendingPreRequisites.Enqueue(new ChoicePrerequisite(this, _Pusher, this, _Counterer, @"CounterOverrun.Attempt", @"Attempt an overrun pushback",
                    DecideToCounter(), true));
        }

        #region private data
        private Creature _Pusher;
        private Creature _Counterer;
        #endregion

        public ChoicePrerequisite CounterAttemptChoice
            => AllPrerequisites<ChoicePrerequisite>(@"CounterOverrun.Attempt").FirstOrDefault();

        #region private IEnumerable<OptionAimOption> DecideToCounter()
        private IEnumerable<OptionAimOption> DecideToCounter()
        {
            yield return new OptionAimValue<bool>
            {
                Key = @"True",
                Description = @"Attempt overrun pushback",
                Name = @"Yes",
                Value = true
            };
            yield return new OptionAimValue<bool>
            {
                Key = @"False",
                Description = @"Do not attempt overrun pushback",
                Name = @"No",
                Value = false
            };
            yield break;
        }
        #endregion

        protected override bool OnDoStep()
        {
            if ((CounterAttemptChoice?.Selected as OptionAimValue<bool>)?.Value ?? false)
            {
                AppendFollowing(new OverrunChecks(this, _Counterer, _Pusher));
            }
            return true;
        }
    }
}
