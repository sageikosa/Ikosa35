using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class RapidShotBudgetFactory : Adjunct, IAttackPotentialFactory
    {
        public RapidShotBudgetFactory(object source)
            : base(source)
        {
        }

        public override object Clone()
            => new RapidShotBudgetFactory(Source);

        public IEnumerable<IAttackPotential> GetIAttackPotentials(FullAttackBudget budget)
        {
            yield return new RapidShotPotential(Source);
            yield break;
        }
    }
}
