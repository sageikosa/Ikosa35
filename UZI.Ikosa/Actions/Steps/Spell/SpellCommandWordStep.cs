using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class SpellCommandWordStep : CoreStep
    {
        public SpellCommandWordStep(CoreActivity activity)
            : base(activity)
        {
        }

        public CoreActivity Activity
            => Process as CoreActivity;

        public CommandWordSpell CommandWordSpell
            => Activity?.Action as CommandWordSpell;

        public override bool IsDispensingPrerequisites
            => false;

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        protected override bool OnDoStep()
        {
            // register with command word (may need charges)
            CommandWordSpell.SpellCommandWord.PowerCapacity
                .UseCharges(CommandWordSpell.SpellCommandWord.ChargeCost);
            AppendFollowing(new PowerActivationStep<SpellSource>(Activity, CommandWordSpell, Activity.Actor));
            return true;
        }
    }
}
