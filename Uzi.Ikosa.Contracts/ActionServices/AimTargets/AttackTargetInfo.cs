using System.Runtime.Serialization;
using Uzi.Core.Contracts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class AttackTargetInfo : AimTargetInfo
    {
        #region construction
        public AttackTargetInfo()
            : base()
        {
        }
        #endregion

        [DataMember]
        public bool IsNonLethal { get; set; }
        [DataMember]
        public int? AttackScore { get; set; }
        [DataMember]
        public bool InRange { get; set; }
        [DataMember]
        public int? CriticalConfirm { get; set; }
        [DataMember]
        public AttackImpact Impact { get; set; }

        [DataMember]
        public int? TargetX { get; set; }
        [DataMember]
        public int? TargetY { get; set; }
        [DataMember]
        public int? TargetZ { get; set; }

        public ICellLocation TargetCell
        {
            get
            {
                if (TargetX.HasValue && TargetY.HasValue && TargetZ.HasValue)
                {
                    return new CellPosition(TargetZ.Value, TargetY.Value, TargetX.Value);
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    TargetZ = value.Z;
                    TargetY = value.Y;
                    TargetX = value.X;
                }
                else
                {
                    TargetZ = null;
                    TargetY = null;
                    TargetX = null;
                }
            }
        }

        public bool IsAttackReady
            => (TargetID.HasValue || (TargetCell != null));
    }
}
