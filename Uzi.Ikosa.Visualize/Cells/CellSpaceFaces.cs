using System;
using System.Windows.Media.Media3D;
using System.Windows;

namespace Uzi.Visualize
{
    public static class CellSpaceFaces
    {
        #region construction
        /// <summary>Static Frozen cubic-face covering mesh</summary>
        static MeshGeometry3D _Mesh = null;

        static CellSpaceFaces()
        {
            // static (and frozen) Mesh
            _Mesh = HedralGenerator.RectangularMesh(new Rect(0, 0, 5, 5), 2, 2, new Vector(5d, 5d));
            _Mesh.Freeze();
        }
        #endregion

        /// <summary>Square 5x5 mesh</summary>
        public static MeshGeometry3D Mesh { get { return _Mesh; } }

        #region public virtual void AddOuterSurface(uint param, ICellSpace cellSpace, BuildablePair buildable, int z, int y, int x, AnchorFace face, VisualEffect effect, Transform3D bump)
        public static void AddOuterSurface(uint param, ICellSpace cellSpace, BuildableGroup buildable,
            int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump)
        {
            Func<BuildableMaterial> _material = () => cellSpace.GetOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect);
            buildable.Context.GetBuildableMesh(cellSpace.GetBuildableMeshKey(face, effect), _material)
                .AddCellFace(face, new Vector3D(x * 5.0d, y * 5.0d, z * 5.0d));
        }
        #endregion

        #region public virtual void AddPlusOuterSurface(uint param, IPlusCellSpace plusSpace, BuildablePair buildable, int z, int y, int x, AnchorFace face, VisualEffect effect, Transform3D bump)
        public static void AddPlusOuterSurface(uint param, IPlusCellSpace plusSpace, BuildableGroup buildable,
            int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump)
        {
            Func<BuildableMaterial> _material = () => plusSpace.GetPlusOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect);
            buildable.Context.GetBuildableMesh(plusSpace.GetPlusBuildableMeshKey(face, effect), _material)
                .AddCellFace(face, new Vector3D(x * 5.0d, y * 5.0d, z * 5.0d));
        }
        #endregion

        #region public static void AddGeometry(Model3DGroup model, GeometryModel3D geom, params Transform3D[] trans)
        /// <summary>Adds a non-null geom to the model after applying the transformations in order.  Non-null are not added</summary>
        public static void AddGeometry(Model3DGroup model, GeometryModel3D geom, params Transform3D[] trans)
        {
            if (geom != null)
            {
                if ((trans?.Length ?? 0) > 0)
                {
                    Transform3DGroup _transform = new Transform3DGroup();
                    foreach (Transform3D _trans in trans)
                    {
                        if (_trans != null)
                        {
                            _transform.Children.Add(_trans);
                        }
                    }
                    _transform.Freeze();
                    geom.Transform = _transform;
                    geom.Freeze();
                }
                model.Children.Add(geom);
            }
        }
        #endregion
    }
}
