using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class DriveCreatureDeliveryStep : PowerActivationStep<SuperNaturalPowerActionSource>
    {
        public DriveCreatureDeliveryStep(
            CoreTargetingProcess targetProcess,
            IPowerUse<SuperNaturalPowerActionSource> powerUse,
            CoreActor actor
            ) : base(targetProcess, powerUse, actor)
        {
            _PendingPreReqs = new Queue<StepPrerequisite>();
            var _workSet = new Interaction(actor, powerUse, null, null);
            _PendingPreReqs.Enqueue(new RollPrerequisite(powerUse, _workSet, _workSet.Actor,
                @"Roll.MaxPowerDie", @"Maximum Power Die Roll", new DieRoller(20), false));
            _PendingPreReqs.Enqueue(new RollPrerequisite(powerUse, _workSet, _workSet.Actor,
                @"Roll.TotalPowerDice", @"Total Power Dice Roll", new DiceRoller(2, 6), false));

            // extra prerequisites
            var _drive = powerUse as DriveCreature;
            if (_drive != null)
            {
                var _extra = _drive.CapabilityRoot.GetCapability<IPowerDeliveryCapable>();
                if (_extra != null)
                {
                    foreach (var _pre in _extra.PowerDeliveryPrerequisites(targetProcess, actor))
                    {
                        _PendingPreReqs.Enqueue(_pre);
                    }
                }
            }
        }

        protected Queue<StepPrerequisite> _PendingPreReqs;

        /// <summary>True if the queue of pending prerequisites still has items.</summary>
        public override bool IsDispensingPrerequisites { get { return _PendingPreReqs.Count > 0; } }

        public DriveCreature DriveCreature => PowerUse as DriveCreature;

        protected override StepPrerequisite OnNextPrerequisite()
        {
            if (_PendingPreReqs.Count > 0)
            {
                return _PendingPreReqs.Dequeue();
            }
            return null;
        }
    }
}