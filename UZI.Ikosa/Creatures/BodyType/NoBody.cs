using System;
using System.Collections.Generic;
using Uzi.Ikosa.Items.Materials;
using Uzi.Core;

namespace Uzi.Ikosa.Creatures.BodyType
{
    [SourceInfo(@"No Body")]
    [Serializable]
    public class NoBody : Body
    {
        public NoBody(Size size, int reach)
            : base(size, VoidMaterial.Static, false, reach, false)
        {
        }

        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            yield break;
        }

        protected override Body InternalClone(Material material)
            => new NoBody(Sizer.Size, ReachSquares.BaseValue);

        public override bool HasBones
            => false;

        public override bool HasAnatomy
            => false;
    }
}
