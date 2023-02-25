using System;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;
using Uzi.Visualize;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class CreatureObjectInfo : CoreInfo
    {
        public CreatureObjectInfo()
        {
        }

        [DataMember]
        public string BodySource { get; set; }

        [DataMember]
        public string[] Features { get; set; }
        // TODO: features that may be hard to see due to concealment (feature scan interaction)

        [DataMember]
        public SizeInfo Size { get; set; }

        [DataMember]
        public ItemSlotInfo[] ItemSlots { get; set; }

        // TODO: portrait (handle interaction and illusions...)
        [DataMember]
        public BitmapImageInfo Portrait { get; set; }

        public override object Clone()
        {
            return new CreatureObjectInfo
            {
                Message = Message,
                ID = ID,
                BodySource = BodySource,
                Features = Features.ToArray(),
                Size = Size.Clone() as SizeInfo,
                ItemSlots = ItemSlots.ToArray()
            };
        }

        /// <summary>Sets IconResolver for all ItemInfo instances in ItemSlots</summary>
        public void SetIconResolver(IResolveIcon resolver)
        {
            if (ItemSlots?.Any() ?? false)
            {
                foreach (var _slot in ItemSlots)
                    if (_slot?.ItemInfo != null)
                        _slot.ItemInfo.IconResolver = resolver;
            }
        }
    }
}
