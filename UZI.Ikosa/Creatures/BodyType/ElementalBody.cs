using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Creatures.BodyType
{
    [SourceInfo(@"Elemental")]
    [Serializable]
    public class ElementalBody : Body
    {
        #region ctor
        public ElementalBody(Material bodyMaterial, Size size, int reach)
            : base(size, bodyMaterial, false, reach, false)
        {
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.SlamSlot));
        }
        #endregion

        protected override Body InternalClone(Material material)
        {
            var _body = new ElementalBody(material, Sizer.BaseSize, ReachSquares.BaseValue);
            _body.Sizer.NaturalSize = Sizer.NaturalSize;
            return _body;
        }

        public override bool HasAnatomy => false;
        public override bool HasBones => false;
    }
}
