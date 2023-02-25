using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Interactions
{
    public class ApplyLight : InteractData
    {
        public ApplyLight(CoreActor actor, LightRange range)
            : base(actor)
        {
            _Range = range;
        }

        private LightRange _Range;

        public LightRange Range =>_Range;
    }
}
