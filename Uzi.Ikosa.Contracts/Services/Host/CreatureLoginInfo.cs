using System;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Contracts.Host
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CreatureLoginInfo
    {
        #region construction
        public CreatureLoginInfo()
        {
        }
        #endregion

        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Species { get; set; }
        [DataMember]
        public Collection<ClassInfo> Classes { get; set; }
        [DataMember]
        public string ClassString { get; set; }

        public BitmapImageInfo Portrait { get; set; }
    }
}