using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Creatures.BodyType
{
    [SourceInfo(@"Centipede")]
    [Serializable]
    public class BeetleBody : Body
    {
        public BeetleBody(Creature creature, Size size, int reach)
            : base(size, ExoskeletonMaterial.Static, true, reach, true)
        {
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.Mouth));
        }

        public override bool HasBones => false;
        public override bool HasAnatomy => true;

        protected override Body InternalClone(Material material)
        {
            var _body = new BeetleBody(Creature, Sizer.BaseSize, ReachSquares.BaseValue);
            _body.Sizer.NaturalSize = Sizer.NaturalSize; // TODO: Size?
            return _body;
        }
    }
}
