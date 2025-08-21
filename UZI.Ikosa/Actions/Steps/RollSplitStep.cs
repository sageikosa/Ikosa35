using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class RollSplitStep : PreReqListStepBase
    {
        // NOTE: this could be Uzi.Core, except that RollPrerequisite is in Ikosa (though it could also be in Core!)

        #region Construction
        public RollSplitStep(CoreStep predecessor, Qualifier workSet, CoreActor fulfiller, string rollKey, string rollName, Roller roller)
            : base(predecessor)
        {
            Initialize(rollKey, rollName, roller);
            InitializePrerequisite(workSet, fulfiller);
        }

        public RollSplitStep(CoreProcess process, Qualifier workSet, CoreActor fulfiller, string rollKey, string rollName, Roller roller)
            : base(process)
        {
            Initialize(rollKey, rollName, roller);
            InitializePrerequisite(workSet, fulfiller);
        }

        public RollSplitStep(CoreStep predecessor, string rollKey, string rollName, Roller roller)
            : base(predecessor)
        {
            Initialize(rollKey, rollName, roller);
            InitializePrerequisite();
        }

        public RollSplitStep(CoreProcess process, string rollKey, string rollName, Roller roller)
            : base(process)
        {
            Initialize(rollKey, rollName, roller);
            InitializePrerequisite();
        }

        private void Initialize(string rollKey, string rollName, Roller roller)
        {
            _RollKey = rollKey;
            _RollName = rollName;
            _Roller = roller;
            _PossibleSteps = [];
        }

        private void InitializePrerequisite()
        {
            // enqueue the roller
            _PendingPreRequisites.Enqueue(new RollPrerequisite(this, _RollKey, _RollName, _Roller, false));
        }

        private void InitializePrerequisite(Qualifier workSet, CoreActor fulfiller)
        {
            // enqueue the roller
            _PendingPreRequisites.Enqueue(new RollPrerequisite(this, workSet, fulfiller, _RollKey, _RollName, _Roller, false));
        }
        #endregion

        #region private data
        private string _RollKey;
        private string _RollName;
        private Roller _Roller;
        private Dictionary<int, CoreStep> _PossibleSteps;
        #endregion

        public override string Name { get { return _RollName; } }

        /// <summary>Dictionary with steps, only one of which will be chosen as the next step based on the roll value</summary>
        public Dictionary<int, CoreStep> PossibleSteps { get { return _PossibleSteps; } }

        protected override bool OnDoStep()
        {
            // get roll prerequisite
            var _rollPre = AllPrerequisites<RollPrerequisite>(_RollKey).FirstOrDefault();
            if ((_rollPre != null) && _rollPre.IsReady)
            {
                // get roll value
                var _value = _rollPre.RollValue;

                // get first step whose max value exceeds or is equal to the roll value
                var _next = PossibleSteps
                    .OrderBy(_kvp => _kvp.Key)
                    .FirstOrDefault(_kvp => _kvp.Key >= _value);
                if (_next.Value != null)
                {
                    // enqueue the next step
                    AppendFollowing(_next.Value);
                }
            }

            // done
            return true;
        }
    }
}