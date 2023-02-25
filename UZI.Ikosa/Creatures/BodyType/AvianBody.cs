using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Creatures.BodyType
{
    [SourceInfo(@"Canine")]
    [Serializable]
    public class AvianBody : Body
    {
        public AvianBody(Material bodyMaterial, Size size, int reach, bool multiTalon)
            : base(size, bodyMaterial, false, reach, false)
        {
            _MultiTalon = multiTalon;
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.Wing, @"Main"));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.Wing, @"Off"));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.NeckSlot));
            if (multiTalon)
            {
                _ItemSlots.Add(new HoldingSlot(this, @"Main", true));
                _ItemSlots.Add(new HoldingSlot(this, @"Off", false));
            }
            else
            {
                _ItemSlots.Add(new HoldingSlot(this, string.Empty, true));
            }
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.ShouldersSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.TorsoSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.Mouth, false));
        }

        #region data
        private bool _MultiTalon;
        #endregion

        public override bool HasBones => true;
        public override bool HasAnatomy => true;
        public bool MultiTalon => _MultiTalon;

        protected override Body InternalClone(Material material)
        {
            var _body = new AvianBody(material, Sizer.BaseSize, ReachSquares.BaseValue, MultiTalon);
            _body.Sizer.NaturalSize = Sizer.NaturalSize;
            return _body;
        }
    }
}
