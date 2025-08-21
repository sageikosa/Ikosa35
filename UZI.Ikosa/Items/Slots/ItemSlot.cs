using System;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// Location for an Item
    /// </summary>
    [Serializable]
    public class ItemSlot : LinkableDock<ISlottedItem>, ICreatureBound, IActionSource
    {
        #region Constructors
        public ItemSlot(object src, string slotType, string subType)
            : this(src, slotType, subType, true, true)
        {
        }

        public ItemSlot(object src, string slotType)
            : this(src, slotType, string.Empty, true, true)
        {
        }

        public ItemSlot(object src, string slotType, bool magicCapable)
            : this(src, slotType, string.Empty, magicCapable, true)
        {
        }

        public ItemSlot(object src, string slotType, string subType, bool magicCapable, bool allowUnslot)
            : base(@"SlottedItem")
        {
            Source = src;
            MagicalSlot = magicCapable;
            SlotType = slotType;
            SubType = subType;
            _UnslotAct = allowUnslot;
        }
        #endregion

        private Guid _ID = Guid.NewGuid();
        private bool _UnslotAct;

        // TODO: consider disabling item slots (¡¡¡ especially holding slots !!!)

        /// <summary>To mark 3rd+ hand (or 2nd head) as being incapable of wearing magical items</summary>
        public bool MagicalSlot { get; private set; }
        public string SlotType { get; private set; }
        public string SubType { get; private set; }

        public virtual bool AllowUnSlotAction => _UnslotAct;

        /// <summary>"SlotType[ (SubType)]"</summary>
        public string ActionName
            => $@"{SlotType}{(!(string.IsNullOrWhiteSpace(SubType)) ? $@" ({SubType})" : string.Empty)}";

        /// <summary>
        /// Virtual slots from magic items, legendary feats, etc.
        /// non-virtual slots are from the creature
        /// </summary>
        public object Source { get; private set; }

        protected override void OnPreLink(ISlottedItem newVal)
        {
            base.OnPreLink(newVal);
            if (SlottedItem != null)
            {
                Creature.ObjectLoad.Remove(SlottedItem, null);
            }
        }

        protected override void OnLink()
        {
            base.OnLink();
            if (SlottedItem != null)
            {
                Creature.ObjectLoad.Add(SlottedItem);
            }
        }

        public Guid ID => _ID;
        public Guid PresenterID => _ID;
        public ISlottedItem SlottedItem => Link;

        #region SlotType Names

        /// <summary>Hats, helms, phylacteries and headbands</summary>
        public const string HeadSlot = @"Head";

        /// <summary>Eye lenses or goggles</summary>
        public const string EyesSlot = @"Eyes";

        /// <summary>Amulets, brooches, medallions, necklaces, periapts and scarabs</summary>
        public const string NeckSlot = @"Neck";

        /// <summary>Vests, vestements and shirts</summary>
        public const string TorsoSlot = @"Torso";

        /// <summary>Bulk for creatures with slam</summary>
        public const string SlamSlot = @"Slam";

        /// <summary>Robes and armors</summary>
        public const string ArmorRobeSlot = @"Armor/Robe";

        /// <summary>Belts and girdles</summary>
        public const string BeltSlot = @"Belt";

        /// <summary>Cloaks, capes and mantles</summary>
        public const string ShouldersSlot = @"Shoulders";

        /// <summary>Bracers and bracelets</summary>
        public const string ArmsSlot = @"Arms";

        /// <summary>Gloves and gauntlets</summary>
        public const string HandsSlot = @"Hands";

        /// <summary>Slippers and boots</summary>
        public const string FeetSlot = @"Feet";

        public const string RingSlot = @"Ring";

        /// <summary>Backpack</summary>
        public const string BackSlot = @"Back";

        /// <summary>For wielding weapons, shields and some spell-activated magic items (wands, staffs and scrolls)</summary>
        public const string HoldingSlot = @"Holding";
        public const string UnarmedSlot = @"Unarmed";

        /// <summary>For bite attack capable creatures</summary>
        public const string Mouth = @"Mouth";

        /// <summary>For gore attack capable creatures</summary>
        public const string Horns = @"Horns";

        /// <summary>For tail slap attack capable creatures</summary>
        public const string Tail = @"Tail";

        /// <summary>For creatures that can use the wing as a weapon</summary>
        public const string Wing = @"Wing";

        /// <summary>For creatures that can use specialized tentacles as weapons in tentacle slots</summary>
        public const string Tentacle = @"Tentacle";

        public const string Spinneret = @"Spinneret";

        // Non-Magical slots for easy access items and small container mounts
        public const string Pouch = @"Pouch";
        public const string AmmoSash = @"Ammo-Sash";

        /// <summary>For weapons held at the hip (light and one-handed)</summary>
        public const string WieldMount = @"Wield-Mount";

        /// <summary>For weapons sheathed behind the back (two-handed, bows and especially long weapons)</summary>
        public const string LargeWieldMount = @"Large-Wield-Mount";

        /// <summary>For storing a shield on the back when not in use</summary>
        public const string BackShieldMount = @"Back-Shield-Mount";

        public const string DevotionalSymbol = @"Devotional-Symbol";

        public const string Attachment = @"Attachment";

        public const string Covering = @"Covering";
        #endregion

        #region ICreatureBound Members
        public Creature Creature
        {
            get
            {
                if (Source is ICreatureBound)
                {
                    return (Source as ICreatureBound).Creature;
                }
                else if (Source is IItemBase)
                {
                    return (Source as IItemBase).CreaturePossessor;
                }
                throw new Exception(@"No Creature for this slot");
            }
        }
        #endregion

        public IVolatileValue ActionClassLevel
            => new Deltable(1);

        #region protected ISInfo ToInfo<ISInfo>(Creature observer)
        protected ISInfo ToInfo<ISInfo>(Creature observer)
            where ISInfo : ItemSlotInfo, new()
            => new ISInfo
            {
                ID = ID,
                Message = SlotType,
                IsMagicalSlot = MagicalSlot,
                SlotType = SlotType,
                SubType = SubType,
                ItemInfo = (SlottedItem?.BaseObject is CoreObject)
                    ? GetInfoData.GetInfoFeedback(SlottedItem?.BaseObject as CoreObject, observer) as ObjectInfo
                    : null,
                HasIdentities = GetIdentityData.GetIdentities(SlottedItem?.BaseObject as CoreObject, observer).Count > 0
            };
        #endregion

        public ItemSlotInfo ToItemSlotInfo()
            => ToInfo<ItemSlotInfo>(Creature);

        public ItemSlotInfo ToItemSlotInfo(Creature observer)
            => ToInfo<ItemSlotInfo>(observer);

        public virtual ItemSlot Clone(object source)
            => new ItemSlot(source, SlotType, SubType, MagicalSlot, AllowUnSlotAction);

        public bool IsOffHand
            => Creature?.Adjuncts.OfType<OffHand>().Any(_oh => _oh.Slot == this) ?? false;
    }
}