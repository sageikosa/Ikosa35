using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class DeepSlumber : Sleep
    {
        public override string DisplayName => @"Deep Slumber";
        public override string Description => @"Puts up to 10 PD of creatures to sleep";

        public override IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new LocationAim(@"Origin", @"Burst Origin", LocationAimMode.Any, FixedRange.One, FixedRange.One, new NearRange());
            yield break;
        }

        protected override decimal GetPowerDiceEffected()
            => 10m;
    }
}
