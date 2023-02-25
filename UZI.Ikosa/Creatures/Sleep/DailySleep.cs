using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Creatures
{
    [Serializable]
    public class DailySleep : Adjunct
    {
        public DailySleep(object source)
            : base(source)
        {
        }

        #region data
        // TODO: tracking flow
        #endregion

        public override object Clone()
            => new DailySleep(Source);

        public static TraitBase GetTrait(Species species)
            => new ExtraordinaryTrait(species, @"Daily Sleep", @"Needs daily sleep to avoid fatigue",
                TraitCategory.Quality, new AdjunctTrait(species, new DailySleep(species)));
    }
}
