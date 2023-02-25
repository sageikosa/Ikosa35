using System.Runtime.Serialization;
using Uzi.Core.Contracts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used primarily for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class MultiStepDestinationInfo : AimTargetInfo
    {
        public MultiStepDestinationInfo() 
            : base()
        {
        }

        [DataMember]
        public int ZSteps { get; set; }

        [DataMember]
        public int YSteps { get; set; }

        [DataMember]
        public int XSteps { get; set; }

        public ICellLocation Offset => new CellPosition(ZSteps, YSteps, XSteps);
    }
}
