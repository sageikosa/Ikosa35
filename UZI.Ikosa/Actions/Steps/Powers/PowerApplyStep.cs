using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Magic.Spells;
using System.Drawing.Imaging;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class PowerApplyStep<PowerSrc> : InteractionPreReqStep, ISplinterableStep
        where PowerSrc : IPowerActionSource
    {
        #region Construction
        public PowerApplyStep(
            CoreStep predecessor,
            IPowerUse<PowerSrc> powerUse,
            CoreActor actor,
            IEnumerable<StepPrerequisite> preRequisites,
            Interaction deliverInteract, bool criticalHit, bool silentFail)
            : base(predecessor?.Process as CoreTargetingProcess, deliverInteract)
        {
            _Root = predecessor;
            _Use = powerUse;
            _Actor = actor;
            _Critical = criticalHit;
            _SilentFail = silentFail;

            // add any additional prerequisites
            if (preRequisites != null)
            {
                foreach (var _addPre in preRequisites)
                {
                    // if the prerequisite assumes a unique key, check for existing
                    if (_addPre.UniqueKey)
                    {
                        // if there are no matching bindKeys, add the prerequisite
                        if (_PendingPreRequisites.FirstOrDefault(_p => _p.BindKey.Equals(_addPre.BindKey)) == null)
                        {
                            _PendingPreRequisites.Enqueue(_addPre);
                        }
                    }
                    else
                    {
                        // add it!
                        _PendingPreRequisites.Enqueue(_addPre);
                    }
                }
            }
        }
        #endregion

        #region data
        private CoreStep _Root;
        private bool _Critical;
        private bool _SilentFail;
        private IPowerUse<PowerSrc> _Use;
        private CoreActor _Actor;
        #endregion

        public bool IsCriticalHit => _Critical;
        public bool FailsSilently => _SilentFail;
        public IPowerUse<PowerSrc> PowerUse => _Use;
        public CoreActor Actor => _Actor;
        public Interaction DeliveryInteraction => _Interaction;

        public CoreTargetingProcess TargetingProcess => Process as CoreTargetingProcess;

        /// <summary>Durable magic effects (if available)</summary>
        public IEnumerable<DurableMagicEffect> DurableMagicEffects
            => (DeliveryInteraction.InteractData as MagicPowerEffectTransit<SpellSource>)
            ?.MagicPowerEffects.OfType<DurableMagicEffect>()
            ?? new DurableMagicEffect[] { };

        public CoreStep MasterStep { get => _Root; set => _Root = value; }

        public NotifyStep GetNotifyStep(string description, string topic, bool isGoodNews)
        {
            if (TargetingProcess is CoreActivity _activity)
            {
                return _activity.GetActivityResultNotifyStep(description);
            }
            if (isGoodNews)
            {
                return TargetingProcess.GetNotifyStep(
                    new GoodNewsNotify(Actor?.ID ?? Guid.Empty, topic,
                    new Info { Message = description }),
                    Actor);
            }
            return TargetingProcess.GetNotifyStep(
                new BadNewsNotify(Actor?.ID ?? Guid.Empty, topic,
                new Info { Message = description }),
                Actor);
        }

        public void Notify(string description, string topic, bool isGoodNews)
            => AppendFollowing(GetNotifyStep(description, topic, isGoodNews));

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            // see if any prerequisite fails the activity
            var _fail = FailingPrerequisite;
            if (_fail != null)
            {
                if (!FailsSilently)
                {
                    Notify($@"Power apply: {_fail.Name} caused failure", @"Power apply failed", false);
                }
            }
            else
            {
                // perform activity
                PowerUse.ApplyPower(this);
                Notify(@"Applied", @"Power applied", true);
            }

            // done
            return true;
        }
        #endregion
    }
}
