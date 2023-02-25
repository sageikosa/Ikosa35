using System;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class GeometricTarget : AimTarget
    {
        #region Construction
        public GeometricTarget(string key, BuilderInfo builder, ICellLocation location, 
            Intersection origin, MapContext mapContext)
            : base(key, null)
        {
            _Geom = new Geometry(builder.GetBuilder(), new CellLocation(location), true);
            _Ctx = mapContext;
            _Origin = origin;
        }
        #endregion

        #region Private Data
        private Geometry _Geom;
        private MapContext _Ctx;
        private Intersection _Origin;
        #endregion

        public Geometry Geometry { get { return _Geom; } }
        public MapContext MapContext { get { return _Ctx; } }
        public Intersection Origin { get { return _Origin; } }

        public override AimTargetInfo GetTargetInfo()
        {
            var _info = ToInfo<GeometricTargetInfo>();
            _info.Origin = new CellInfo(Origin);

            #region Func<IGeometryBuilder, BuilderInfo> _buildConvert
            Func<IGeometryBuilder, BuilderInfo> _buildConvert = null;
            _buildConvert = (builder) =>
            {
                if (builder is ConeBuilder)
                {
                    return new ConicBuilderInfo(builder as ConeBuilder);
                }
                else if (builder is CubicBuilder)
                {
                    return new CubicBuilderInfo(builder as CubicBuilder);
                }
                else if (builder is SphereBuilder)
                {
                    return new SphereBuilderInfo(builder as SphereBuilder);
                }
                else if (builder is CylinderBuilder)
                {
                    return new CylinderBuilderInfo(builder as CylinderBuilder);
                }
                else if (builder is MultiBuilder)
                {
                    return new MultiBuilderInfo(builder as MultiBuilder, _buildConvert);
                }
                return null;
            };
            #endregion

            _info.Builder = _buildConvert(Geometry.Builder);
            _info.Location = new CellInfo(Geometry.Location);
            return _info;
        }
    }
}
