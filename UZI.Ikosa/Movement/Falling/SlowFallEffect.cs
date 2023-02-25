using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Movement
{
    /// <summary>
    /// Marker to let falling step know it should change between fall movement and slow fall movement
    /// </summary>
    [Serializable]
    public class SlowFallEffect : Adjunct
    {
        /// <summary>
        /// Marker to let falling step know it should change between fall movement and slow fall movement
        /// </summary>
        public SlowFallEffect(object source)
            : base(source)
        {
        }

        public override object Clone()
            => new SlowFallEffect(Source);
    }
}
