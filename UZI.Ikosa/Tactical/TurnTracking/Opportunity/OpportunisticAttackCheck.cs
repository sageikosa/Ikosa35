using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// determines whether a potential opportunistic attack can (and will) be negated; 
    /// and if not, allows the opportunistic attacker the option to attack
    /// </summary>
    [Serializable]
    public class OpportunisticAttackCheck : CoreStep
    {
        #region ctor()
        /// <summary>
        /// determines whether a potential opportunistic attack can (and will) be negated; 
        /// and if not, allows the opportunistic attacker the option to attack
        /// </summary>
        public OpportunisticAttackCheck(
            OpportunisticInquiry inquiry, 
            CoreActor attacker, 
            List<(IActionProvider, OpportunisticAttack)> attacks)
            : base(inquiry)
        {
            _Inquiry = inquiry;
            _Attacker = attacker;
            _Attacks = attacks;
            // TODO: get viable attack targets from the _Inquiry (may be different than actor)

            // interact to determine whether a negation can happen
            var _actor = _Inquiry.Activity.Actor;
            var _provoke = new OpportunisticProvokeData(_Inquiry.Activity, attacker);
            var _workset = new Interaction(_actor, this, _actor, _provoke);
            _actor.HandleInteraction(_workset);

            // flatten negate checks
            _NegateChecks = new Queue<ISuccessCheckPrerequisite>(
                _workset.Feedback.OfType<OpportunisticProvokeFeedback>()
                .Select(_fb => _fb.SuccessCheck));

            // track dispensed
            _CurrNegate = null;
            _AttackerCheck = null;
        }
        #endregion

        #region state
        private OpportunisticInquiry _Inquiry;
        private CoreActor _Attacker;
        private List<(IActionProvider, OpportunisticAttack)> _Attacks;
        private Queue<ISuccessCheckPrerequisite> _NegateChecks;
        private ISuccessCheckPrerequisite _CurrNegate;
        private OpportunisticPrerequisite _AttackerCheck;
        #endregion

        protected override StepPrerequisite OnNextPrerequisite()
        {
            // TODO: assumes negates are serial!!! should we allow non-serial negates?

            // have a dispensed negate check?
            if (_CurrNegate != null)
            {
                // is it ready?
                if (_CurrNegate.IsReady)
                {
                    // will it prevent an opportunistic attack?
                    if (_CurrNegate.Success)
                    {
                        // no more prerequsites for this step
                        return null;
                    }
                    else
                    {
                        // the current negate was useless, so clear it
                        _CurrNegate = null;
                    }
                }
                else
                {
                    // must take care of the current one before we can move on
                    return null;
                }
            }

            // have some negates to dispense?
            if (_NegateChecks.Any())
            {
                _CurrNegate = _NegateChecks.Dequeue();
                // NOTE: might need this is the process stalls...
                //if (_CurrNegate.IsReady && _CurrNegate.WillNegate)
                //    return null;
                return _CurrNegate as StepPrerequisite; // NOTE: always implement on StepPrerequsites!
            }

            // made it this far without negation
            if (_AttackerCheck == null)
            {
                // haven't built this, so build it now
                _AttackerCheck = new OpportunisticPrerequisite(_Inquiry.Activity, _Attacker, _Attacks);
                return _AttackerCheck;
            }

            // nothing more to give
            return null;
        }

        public override bool IsDispensingPrerequisites
        {
            get
            {
                // TODO: assumes negates are serial!!! should we allow non-serial negates?

                // can still try to negate?
                if (_NegateChecks.Any())
                    return true;

                // have a negate in the hopper?
                if (_CurrNegate != null)
                {
                    // not ready, so we don't know what's next
                    if (!_CurrNegate.IsReady)
                        return true;

                    // it will negate, so we're done
                    if (_CurrNegate.Success)
                        return false;
                }

                // haven't asked whether an opportunity attack will be made yet
                if (_AttackerCheck == null)
                    return true;

                // no more prerequisites to give
                return false;
            }
        }

        protected override bool OnDoStep()
        {
            // nothing to really do here, all the real fun is in the prerequisites
            return true;
        }
    }
}
