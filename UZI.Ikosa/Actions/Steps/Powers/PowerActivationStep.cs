using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class PowerActivationStep<PowerSrc> : CoreStep, ISplinterableStep
        where PowerSrc : IPowerActionSource
    {
        public PowerActivationStep(
            CoreTargetingProcess targetProcess,
            IPowerUse<PowerSrc> powerUse,
            CoreActor actor)
            : base(targetProcess)
        {
            _Root = null;
            _Use = powerUse;
            _Actor = actor;
        }

        #region state
        private IPowerUse<PowerSrc> _Use;
        private CoreActor _Actor;
        private CoreStep _Root;
        #endregion

        public CoreTargetingProcess TargetingProcess => Process as CoreTargetingProcess;
        public IPowerUse<PowerSrc> PowerUse => _Use;
        public CoreActor Actor => _Actor;

        public void GeneratePowerBurstVisualizers(Point3D source, double radius, int sequence, bool showSplash)
        {
            // TODO:
        }

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
            PowerUse.ActivatePower(this);
            Notify(@"Activated", @"Power activated", true);
            return true;
        }
        #endregion

        // ISplinterableStep Members
        public CoreStep MasterStep { get => _Root; set => _Root = value; }

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        public override bool IsDispensingPrerequisites
            => false;
    }

    /// <summary>Defines extra prerequisites for power delivery steps</summary>
    public interface IPowerDeliveryCapable : ICapability
    {
        /// <summary>Defines extra prerequisites for power delivery steps</summary>
        IEnumerable<StepPrerequisite> PowerDeliveryPrerequisites(CoreTargetingProcess targetProcess, CoreActor actor);
    }
}