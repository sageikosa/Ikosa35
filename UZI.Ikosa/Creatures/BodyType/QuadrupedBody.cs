using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Creatures.BodyType
{
    [SourceInfo(@"Quaduped")]
    [Serializable]
    public class QuadrupedBody : Body
    {
        public QuadrupedBody(Material bodyMaterial, Size size, int reach)
            : base(size, bodyMaterial, true, reach, true)
        {
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.ArmsSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.BackSlot, false));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.BeltSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.FeetSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.NeckSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.ShouldersSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.TorsoSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.Mouth, false));
        }

        protected override Body InternalClone(Material material)
        {
            var _body = new QuadrupedBody(material, Sizer.BaseSize, ReachSquares.BaseValue);
            _body.Sizer.NaturalSize = Sizer.NaturalSize;
            return _body;
        }

        public override bool HasBones => true;
        public override bool HasAnatomy => true;
    }
}
