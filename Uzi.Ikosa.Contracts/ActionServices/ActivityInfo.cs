using System;
using System.Runtime.Serialization;
using System.Windows;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class ActivityInfo
    {
        [DataMember]
        public Guid ActorID { get; set; }
        [DataMember]
        public Guid ActionID { get; set; }
        [DataMember]
        public string ActionKey { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public AimTargetInfo[] Targets { get; set; }

        public Visibility DescriptionVisibility => string.IsNullOrWhiteSpace(Description) ? Visibility.Collapsed : Visibility.Visible;
    }
}
