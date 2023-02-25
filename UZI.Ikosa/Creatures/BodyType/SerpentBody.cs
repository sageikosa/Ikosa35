using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Creatures.BodyType
{
    [SourceInfo(@"Serpentine")]
    [Serializable]
    public class SerpentBody : Body
    {
        public SerpentBody(Material bodyMaterial, Size size, int reach)
            : base(size, bodyMaterial, true, reach, true)
        {
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.Mouth, false));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.NeckSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.BeltSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.ShouldersSlot));
        }

        protected override Body InternalClone(Material material)
        {
            var _body = new SerpentBody(material, Sizer.BaseSize, ReachSquares.BaseValue);
            _body.Sizer.NaturalSize = Sizer.NaturalSize;
            return _body;
        }

        public override bool HasBones => true;
        public override bool HasAnatomy => true;
    }
}
