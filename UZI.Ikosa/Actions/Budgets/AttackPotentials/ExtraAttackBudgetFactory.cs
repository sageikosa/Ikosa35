using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class ExtraAttackBudgetFactory : Adjunct, IAttackPotentialFactory
    {
        public ExtraAttackBudgetFactory(object source)
            : base(source)
        {
        }

        public override object Clone()
            => new ExtraAttackBudgetFactory(Source);

        public IEnumerable<IAttackPotential> GetIAttackPotentials(FullAttackBudget budget)
        {
            if (!budget.Budget.BudgetItems.Items.OfType<ExtraAttackPotential>().Any())
            {
                // do not add if there is already one...only allowed one of these
                yield return new ExtraAttackPotential(Source);
            }
            yield break;
        }
    }
}
