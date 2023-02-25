using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class MeleeTriggerable : AttackTriggerable
    {
        #region ctor()
        public MeleeTriggerable(string name, Material material, int seedDifficulty)
            : base(name, material, seedDifficulty)
        {
        }
        #endregion

        protected override string ClassIconKey => nameof(MeleeTriggerable);

        public override void DoTrigger(IActivatableObject mechanism, IEnumerable<Locator> locators)
        {
            if (Activation.IsActive && !IsDisabled)
            {
                if (this.GetLocated()?.Locator is Locator _loc)
                {
                    ConfusedDisablers.Clear();
                    _loc.Map.ContextSet.ProcessManager.StartProcess(
                        new CoreTargetingProcess(new TriggeredMeleeAttackStep(this), this, @"Melee Trap",
                            IsDirect
                            ? new List<AimTarget> { new ValueTarget<List<Locator>>(@"Direct", locators.ToList()) }
                            : null));
                }
            }
        }
    }
}
