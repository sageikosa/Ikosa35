using System;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class ItemSlotInfo : CoreInfo
    {
        #region construction
        public ItemSlotInfo() : base()
        {
        }
        #endregion

        [DataMember]
        public bool? IsMagicalSlot { get; set; }
        [DataMember]
        public string SlotType { get; set; }
        [DataMember]
        public string SubType { get; set; }
        [DataMember]
        public ObjectInfo ItemInfo { get; set; }
        [DataMember]
        public bool HasIdentities { get; set; }

        public bool HasID(Guid id)
        {
            if (ID == id)
                return true;
            else if (ItemInfo?.ID == id)
                return true;
            else if (ItemInfo is DoubleMeleeWeaponInfo _dbl)
            {
                return _dbl.WeaponHeads.Any(_wh => _wh.ID == id);
            }
            else if (ItemInfo is MeleeWeaponInfo _melee)
            {
                return _melee.WeaponHeads.Any(_wh => _wh.ID == id);
            }
            else if (ItemInfo is NaturalWeaponInfo _natural)
            {
                return _natural.WeaponHead.ID == id;
            }
            else if (ItemInfo is ProjectileWeaponInfo _projectile)
            {
                return _projectile.VirtualHead.ID == id;
            }
            return false;
        }
    }
}
