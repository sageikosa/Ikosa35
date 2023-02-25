using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    /// <summary>This is a marker to the client that they are allowed to act or end turn.</summary>
    [DataContract(Namespace = Statics.Namespace)]
    public class ActionInquiryPrerequisiteInfo : PrerequisiteInfo
    {
    }
}
