using System;
using Uzi.Ikosa.Senses;
using Uzi.Core;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Interactions;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    /// <summary>Used to provide light from an ambient light</summary>
    public class AmbientIllumination : IIllumination, IInteract
    {
        #region construction
        /// <summary>Used to provide light from an ambient light</summary>
        public AmbientIllumination(AmbientLight light)
        {
            _Light = light;
            _Distance = _Light.Vector3D.Length;
        }
        #endregion

        #region private data
        private AmbientLight _Light;
        private double _Distance;
        #endregion

        public double VeryBrightRange => (_Light.VeryBright > 0 ? _Distance + _Light.VeryBright : 0);
        public double BrightRange => (_Light.Bright > 0 ? _Distance + _Light.Bright : 0);
        public double ShadowyRange => (_Light.NearShadow > 0 ? _Distance + _Light.NearShadow : 0);
        public double FarShadowyRange => (_Light.FarShadow > 0 ? _Distance + _Light.FarShadow : 0);

        #region IIllumination Members

        public IGeometricRegion SourceGeometry(IGeometricRegion target)
        {
            var _oZ = Convert.ToInt32(_Light.Vector3D.Z / 5);
            var _oY = Convert.ToInt32(_Light.Vector3D.Y / 5);
            var _oX = Convert.ToInt32(_Light.Vector3D.X / 5);
            return new CellPosition(target.LowerZ + _oZ, target.LowerY + _oY, target.LowerX + _oX);
        }

        public PlanarPresence PlanarPresence => PlanarPresence.Material;
        public IInteract LightHandler => this;
        public Point3D InteractionPoint3D(IGeometricRegion target)
            => target.GetPoint3D() + _Light.Vector3D;

        public Point3D InteractionPoint3D(Point3D target)
            => target + _Light.Vector3D;

        public LightRange MaximumLight => _Light.AmbientLevel;
        public bool IsActive => true;
        public bool IsUsable => true;
        public object Source => _Light;
        public Guid ID => Guid.Empty;

        public double SolarLeft(ICellLocation location)
            => _Light.IsSolar ? VeryBrightRange : 0;

        public double VeryBrightLeft(ICellLocation location)
            => VeryBrightRange;

        public double BrightLeft(ICellLocation location) => BrightRange;
        public double ShadowyLeft(ICellLocation location) => ShadowyRange;
        public double FarShadowyLeft(ICellLocation location) => FarShadowyRange;

        public double NearBoostLeft(ICellLocation location)
            => (BrightRange > 0)
            ? (ShadowyRange + BrightRange) / 2
            : 0;

        public double FarBoostLeft(ICellLocation location)
            => (ShadowyRange > 0)
            ? ShadowyRange + 5
            : 0;

        public double ExtentBoostLeft(ICellLocation location)
            => (FarShadowyRange > 0)
            ? FarShadowyRange + 5
            : 0;

        public double SolarLeft(Point3D location)
            => _Light.IsSolar ? VeryBrightRange : 0;

        public double VeryBrightLeft(Point3D location)
            => VeryBrightRange;

        public double BrightLeft(Point3D location) => BrightRange;
        public double ShadowyLeft(Point3D location) => ShadowyRange;
        public double FarShadowyLeft(Point3D location) => FarShadowyRange;

        public double NearBoostLeft(Point3D location)
            => (BrightRange > 0)
            ? (ShadowyRange + BrightRange) / 2
            : 0;

        public double FarBoostLeft(Point3D location)
            => (ShadowyRange > 0)
            ? ShadowyRange + 5
            : 0;

        public double ExtentBoostLeft(Point3D location)
            => (FarShadowyRange > 0)
            ? FarShadowyRange + 5
            : 0;

        #endregion

        #region IInteract Members

        public void HandleInteraction(Interaction interact)
        {
            // illuminate is a request to provide light to a target
            var _illuminate = interact.InteractData as Illuminate;
            if (_illuminate != null)
            {
                // ... then the link can provide light
                var _handler = new IlluminateHandler();
                _handler.HandleInteraction(interact);
                // NOTE: otherwise, the target is in the same "room" as the light, and doesn't need the link
            }
        }

        #endregion
    }
}