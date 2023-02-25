using Uzi.Core;
using Uzi.Ikosa.Tactical;
using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Ikosa.Senses
{
    public interface IIllumination
    {
        IInteract LightHandler { get; }
        /// <summary>Solar range (if defined)</summary>
        double SolarLeft(ICellLocation location);
        /// <summary>Very bright range (if defined)</summary>
        double VeryBrightLeft(ICellLocation location);
        /// <summary>Bright range for normal vision</summary>
        double BrightLeft(ICellLocation location);
        /// <summary>Farthest shadowy range for normal vision, and bright range for low-light vision</summary>
        double ShadowyLeft(ICellLocation location);
        /// <summary>Farthest shadowy range for low light vision</summary>
        double FarShadowyLeft(ICellLocation location);

        double NearBoostLeft(ICellLocation location);
        double FarBoostLeft(ICellLocation location);
        double ExtentBoostLeft(ICellLocation location);

        /// <summary>Solar range (if defined)</summary>
        double SolarLeft(Point3D point3D);
        /// <summary>Very bright range (if defined)</summary>
        double VeryBrightLeft(Point3D point3D);
        /// <summary>Bright range for normal vision</summary>
        double BrightLeft(Point3D point3D);
        /// <summary>Farthest shadowy range for normal vision, and bright range for low-light vision</summary>
        double ShadowyLeft(Point3D point3D);
        /// <summary>Farthest shadowy range for low light vision</summary>
        double FarShadowyLeft(Point3D point3D);

        double NearBoostLeft(Point3D point3D);
        double FarBoostLeft(Point3D point3D);
        double ExtentBoostLeft(Point3D point3D);

        /// <summary>Returns the maximum light level</summary>
        LightRange MaximumLight { get; }
        bool IsActive { get; }
        PlanarPresence PlanarPresence { get; }

        /// <summary>False if not usable as a light currently</summary>
        bool IsUsable { get; }
        object Source { get; }
        /// <summary>Physical light geometry</summary>
        IGeometricRegion SourceGeometry(IGeometricRegion target);
        /// <summary>Light point for determining interaction lines</summary>
        Point3D InteractionPoint3D(IGeometricRegion target);
        /// <summary>Light point for determining interaction lines</summary>
        Point3D InteractionPoint3D(Point3D point3D);
    }
}
