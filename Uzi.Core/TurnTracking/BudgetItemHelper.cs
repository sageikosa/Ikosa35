using Uzi.Core.Contracts;

namespace Uzi.Core
{
    public static class BudgetItemHelper
    {
        public static BIInfo ToBudgetItemInfo<BIInfo>(this IBudgetItem self)
            where BIInfo: BudgetItemInfo, new ()
        {
            return new BIInfo
            {
                Description = self.Description,
                Name = self.Name
            };
        }
    }
}
