using System;
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Core.Contracts;
using Uzi.Visualize.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class ExtraInfoMarker : ExtraInfo
    {
        public ExtraInfoMarker(ExtraInfoSource source, IInformable info, IGeometricRegion geometry, 
            bool directionOnly, AuraStrength strength, IActionProvider provider)
            :base(source, info, provider)
        {
            _Geom = geometry;
            _DirOnly = directionOnly;
            _Strength = strength;
        }

        #region state
        private IGeometricRegion _Geom;
        private bool _DirOnly;
        private AuraStrength _Strength;
        #endregion

        /// <summary>Location of the informations</summary>
        public IGeometricRegion Geometry => _Geom;

        /// <summary>When presenting, only the direction is made known</summary>
        public bool DirectionOnly => _DirOnly;

        public AuraStrength Strength => _Strength;

        public ExtraInfoMarkerInfo ToExtraInfoMarkerInfo(CoreActor actor)
        {
            var _info = ToInfo<ExtraInfoMarkerInfo>(actor);
            _info.DirectionOnly = DirectionOnly;
            _info.Region = new CubicInfo();
            _info.Region.SetCubicInfo(Geometry);
            _info.Strength = Strength;
            return _info;
        }
    }
}
