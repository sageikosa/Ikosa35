using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class DeeperDarkness : Darkness
    {
        public override string DisplayName => @"Deeper Darkness";
        public override string Description => @"Object illuminates a 60 foot radius sphere with shadowy illumination";

        public override DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Day(), 1));

        public override IEnumerable<double> Dimensions(CoreActor actor, int powerLevel)
            => 60d.ToEnumerable();
    }
}
