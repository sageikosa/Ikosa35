using System;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Creatures.BodyType
{
    [SourceInfo(@"Humanoid")]
    [Serializable]
    public class HumanoidBody : Body
    {
        public HumanoidBody(Material bodyMaterial, Size size, int reach, bool groundStability)
            : base(size, bodyMaterial, false, reach, groundStability)
        {
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.ArmorRobeSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.ArmsSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.BackSlot, false));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.BeltSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.EyesSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.FeetSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.HandsSlot));
            _ItemSlots.Add(new HoldingSlot(this, @"Main", true));
            _ItemSlots.Add(new HoldingSlot(this, @"Off", true));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.HeadSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.NeckSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.RingSlot, @"Left"));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.RingSlot, @"Right"));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.ShouldersSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.TorsoSlot));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.AmmoSash, @"Left-Shoulder", false, true));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.AmmoSash, @"Right-Shoulder", false, true));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.Pouch, @"Pouch", false, true));
            _ItemSlots.Add(new ItemSlot(this, ItemSlot.DevotionalSymbol, false));
            _ItemSlots.Add(new MountSlot(this, ItemSlot.WieldMount, @"Upper-Left", false));
            _ItemSlots.Add(new MountSlot(this, ItemSlot.WieldMount, @"Upper-Right", false));
            _ItemSlots.Add(new MountSlot(this, ItemSlot.WieldMount, @"Lower-Left", false));
            _ItemSlots.Add(new MountSlot(this, ItemSlot.WieldMount, @"Lower-Right", false));
            _ItemSlots.Add(new MountSlot(this, ItemSlot.LargeWieldMount, @"Left", false));
            _ItemSlots.Add(new MountSlot(this, ItemSlot.LargeWieldMount, @"Right", false));
            _ItemSlots.Add(new MountSlot(this, ItemSlot.LargeWieldMount, @"Middle", false));
            _ItemSlots.Add(new MountSlot(this, ItemSlot.BackShieldMount, string.Empty, false));

            // create unarmed slot
            var _uSlot = new ItemSlot(this, ItemSlot.UnarmedSlot, false);
            _ItemSlots.Add(_uSlot);

            // start watching it
            _uSlot.AddChangeMonitor(this);
        }

        protected override void OnConnectBody()
        {
            base.OnConnectBody();

            // bootstrap it
            var _uSlot = _ItemSlots[ItemSlot.UnarmedSlot];
            EnsureUnarmedWeapon(_uSlot);
        }

        protected override Body InternalClone(Material material)
        {
            var _body = new HumanoidBody(material, this.Sizer.BaseSize, ReachSquares.BaseValue, GroundStability);
            _body.Sizer.NaturalSize = this.Sizer.NaturalSize;
            return _body;
        }

        public override bool HasBones
        {
            get { return true; }
        }

        public override bool HasAnatomy
        {
            get { return true; }
        }
    }
}