using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>[Serial]</summary>
    [Serializable]
    public class ReactivePrerequisite : StepPrerequisite
    {
        /// <summary>[Serial]</summary>
        public ReactivePrerequisite(
            Info trigger,
            CoreActor actor,
            IEnumerable<(IActionProvider, ActionBase)> actions
            )
            : base(trigger, actor, null, null, @"Reaction.WillAct", @"React to Trigger?")
        {
            _Actions = actions.ToList();
        }

        #region state
        private List<(IActionProvider provider, ActionBase action)> _Actions;
        private bool _Finished;
        #endregion

        /// <summary>Reactions that can be made</summary>
        public List<(IActionProvider provider, ActionBase action)> Actions => _Actions;

        /// <summary>Info on what caused the reactive check</summary>
        public Info Trigger 
            => Source as Info;

        /// <summary>Once any decisions to pass or to react are performed, this must be set to true.</summary>
        public bool Finished { get => _Finished; set => _Finished = value; }

        public override CoreActor Fulfiller
            => Qualification.Actor;

        public override bool IsReady => Finished;
        public override bool IsSerial => true;

        // NOTE: the reaction itself doesn't fail the process that caused the reaction
        // NOTE: however, if the process may cease for other reasons related to the reaction
        public override bool FailsProcess => false;

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
        {
            var _info = ToInfo<ReactivePrerequisiteInfo>(step);
            _info.TriggeringCondition = Trigger;
            _info.ReactiveActions = (from _act in Actions
                                     let _rslt = new ActionResult
                                     {
                                         Provider = _act.provider,
                                         Action = _act.action,
                                         IsExternal = false
                                     }
                                     select _rslt.ToActionInfo(Fulfiller)).ToArray();
            return _info;
        }

        public override void MergeFrom(PrerequisiteInfo info)
        {
            if (info is ReactivePrerequisiteInfo _react)
            {
                if (_react.ResponseActivity != null)
                {
                    var _activity = _react.ResponseActivity.CreateReactiveActivity(this);
                    if (_activity?.Action != null)
                    {
                        if (_activity.Actor.ProcessManager is IkosaProcessManager _manager)
                        {
                            (_manager.LocalTurnTracker.GetBudget(_activity.Actor.ID))
                                ?.DoAction(_manager, _activity);
                        }
                    }
                }
                Finished = true;
            }
        }
    }
}
