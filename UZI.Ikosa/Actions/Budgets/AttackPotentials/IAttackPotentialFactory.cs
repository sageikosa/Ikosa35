using System.Collections.Generic;

namespace Uzi.Ikosa.Actions
{
    public interface IAttackPotentialFactory
    {
        IEnumerable<IAttackPotential> GetIAttackPotentials(FullAttackBudget budget);
    }
}
