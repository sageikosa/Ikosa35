using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class MinimumSizeRequirement : RequirementAttribute
    {
        public MinimumSizeRequirement(int sizeOrder)
        {
            SizeOrder = sizeOrder;
        }

        public int SizeOrder { get; }

        public override string Name 
            => $@"Minimum size {Size.FromSizeOrder(SizeOrder).Name}";

        public override string Description 
            => $@"Must have a natural size of {Size.FromSizeOrder(SizeOrder).Name} or larger";

        public override bool MeetsRequirement(Creature creature)
            => (creature?.Body.Sizer.NaturalSize.Order >= SizeOrder);

        public override RequirementMonitor CreateMonitor(IRefreshable target, Creature owner)
        {
            return null;
        }
    }
}
