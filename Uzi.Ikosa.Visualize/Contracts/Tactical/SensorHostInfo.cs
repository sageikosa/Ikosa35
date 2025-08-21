using System;
using System.Runtime.Serialization;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class SensorHostInfo : CubicInfo, ICloneable
    {
        [DataMember]
        public string SensorHostName { get; set; }
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public bool ForTerrain { get; set; }
        [DataMember]
        public bool ForTargeting { get; set; }
        [DataMember]
        public int GravityFace { get; set; }
        [DataMember]
        public int Heading { get; set; }
        [DataMember]
        public int Incline { get; set; }
        [DataMember]
        public TacticalPoint Offset { get; set; }
        [DataMember]
        public TacticalPoint AimPoint { get; set; }
        [DataMember]
        public double AimPointRelLongitude { get; set; }
        [DataMember]
        public double AimPointRelLatitude { get; set; }
        [DataMember]
        public double AimPointRelDistance { get; set; }
        [DataMember]
        public int? AimCellZ { get; set; }
        [DataMember]
        public int? AimCellY { get; set; }
        [DataMember]
        public int? AimCellX { get; set; }
        [DataMember]
        public int ThirdCameraRelativeHeading { get; set; }
        [DataMember]
        public int ThirdCameraIncline { get; set; }
        [DataMember]
        public TacticalPoint ThirdCameraPoint { get; set; }
        [DataMember]
        public PointEffect CenterEffect { get; set; }
        [DataMember]
        public PointEffect AimCellEffect { get; set; }
        [DataMember]
        public PointEffect ThirdCameraEffect { get; set; }

        #region ICloneable Members

        public object Clone()
        {
            return new SensorHostInfo
            {
                AimPointRelDistance = AimPointRelDistance,
                AimPointRelLatitude = AimPointRelLatitude,
                AimPointRelLongitude = AimPointRelLongitude,
                AimPoint = new TacticalPoint
                {
                    Z = AimPoint.Z,
                    Y = AimPoint.Y,
                    X = AimPoint.X
                },
                ForTargeting = ForTargeting,
                ForTerrain = ForTerrain,
                GravityFace = GravityFace,
                Heading = Heading,
                Incline = Incline,
                ID = ID,
                X = X,
                Y = Y,
                Z = Z,
                Offset = new TacticalPoint
                {
                    Z = Offset.Z,
                    Y = Offset.Y,
                    X = Offset.X
                },
                ThirdCameraIncline = ThirdCameraIncline,
                ThirdCameraRelativeHeading = ThirdCameraRelativeHeading,
                ThirdCameraPoint = new TacticalPoint
                {
                    Z = ThirdCameraPoint.Z,
                    Y = ThirdCameraPoint.Y,
                    X = ThirdCameraPoint.X
                },
                XTop = XTop,
                YTop = YTop,
                ZTop = ZTop,
                AimCellZ = AimCellZ,
                AimCellY = AimCellY,
                AimCellX = AimCellX
            };
        }

        #endregion

        /// <summary>Cell from which melee attacks start</summary>
        public ICellLocation AimCell
        {
            get
            {
                if (AimCellX.HasValue && AimCellY.HasValue && AimCellZ.HasValue)
                {
                    return new CellPosition(AimCellZ ?? 0, AimCellY ?? 0, AimCellX ?? 0);
                }
                return new CellPosition(Z, Y, X);
            }
        }

        /// <summary>De-Marshalled value</summary>
        public AnchorFace GravityAnchorFace { get { return (AnchorFace)GravityFace; } }

        public Point3D AimPoint3D
        {
            get { return new Point3D(AimPoint.X, AimPoint.Y, AimPoint.Z); }
            set
            {
                AimPoint = new TacticalPoint
                {
                    X = value.X,
                    Y = value.Y,
                    Z = value.Z
                };
            }
        }
    }
}
