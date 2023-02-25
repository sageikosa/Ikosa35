using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class FlightMovementInfo : MovementInfo
    {
        public FlightMovementInfo()
        {
        }

        public FlightMovementInfo(FlightMovementInfo copySource)
            : base(copySource)
        {
            MinSpeed = copySource.MinSpeed;
            CanHover = copySource.CanHover;
            FreeYaw = copySource.FreeYaw;
            MaxYaw = copySource.MaxYaw;
            YawGap = copySource.YawGap;
            MaxUpAngle = copySource.MaxUpAngle;
            MaxDownAngle = copySource.MaxDownAngle;
            DownUpGap = copySource.DownUpGap;
        }

        [DataMember]
        public double MinSpeed { get; set; }

        [DataMember]
        public bool CanHover { get; set; }

        [DataMember]
        public int FreeYaw { get; set; }

        [DataMember]
        public int MaxYaw { get; set; }

        [DataMember]
        public double YawGap { get; set; }

        [DataMember]
        public double MaxUpAngle { get; set; }

        [DataMember]
        public double MaxDownAngle { get; set; }

        [DataMember]
        public double DownUpGap { get; set; }
    }
}
