using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class PossessionInfo
    {
        #region construction
        public PossessionInfo()
        {
        }
        #endregion

        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public ObjectInfo ObjectInfo { get; set; }
        [DataMember]
        public bool HasIdentities { get; set; }
        [DataMember]
        public bool IsLocal { get; set; }
    }
}
