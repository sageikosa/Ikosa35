using System.Runtime.Serialization;
using Uzi.Visualize.Packaging;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class IconInfo
    {
        #region construction
        public IconInfo()
        {
        }

        public IconInfo(IconPart iconPart)
        {
            Name = iconPart.Name;
            Bytes = iconPart.StreamBytes;
        }
        #endregion

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public byte[] Bytes { get; set; }

        public IconPart ToIconPart()
        {
            return new IconPart(this);
        }
    }
}
