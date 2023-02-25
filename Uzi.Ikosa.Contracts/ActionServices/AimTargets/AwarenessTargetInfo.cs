using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class AwarenessTargetInfo : AimTargetInfo
    {
        #region construction
        public AwarenessTargetInfo()
            : base()
        {
        }
        #endregion
    }
}
