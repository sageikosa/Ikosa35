using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Windows;

namespace Uzi.Visualize
{
    public class BuildableMesh
    {
        public BuildableMesh(BuildableMaterial material)
        {
            _Material = material;
            _Buckets =
            [
                new BuildableMeshBucket()
            ];
        }

        #region private data
        private BuildableMaterial _Material;
        private List<BuildableMeshBucket> _Buckets;
        #endregion

        public IEnumerable<Model3D> GetModels()
            => _Buckets.Select(_b => new GeometryModel3D(new MeshGeometry3D()
            {
                Positions = _b.Points,
                Normals = _b.Normals,
                TextureCoordinates = _b.TextureCoordinates,
                TriangleIndices = _b.TriangleIndexes
            }, _Material.Material));

        public bool IsAlpha
            => _Material.IsAlpha;

        private BuildableMeshBucket GetBucket()
        {
            // make sure any mesh bucket doesn't get too big
            var _current = _Buckets[_Buckets.Count - 1];
            if (_current.AtCapacity)
            {
                _current = new BuildableMeshBucket();
                _Buckets.Add(_current);
            }

            return _current;
        }

        #region public void AddCellFace(AnchorFace face, params Vector3D[] bump)
        public void AddCellFace(AnchorFace face, params Vector3D[] bump)
        {
            var _current = GetBucket();
            _current.AddCellFace(face, bump);
        }
        #endregion

        public void AddQuad(Point3D p0, Point3D p1, Point3D p2, Point3D p3, Point uv0, Point uv1, Point uv2, Point uv3, params Vector3D[] bump)
        {
            var _current = GetBucket();
            _current.AddQuad(p0, p1, p2, p3, uv0, uv1, uv2, uv3, bump);
        }

        #region public void AddMeshFace(MeshGeometry3D mesh, ICellLocation location, AnchorFace face, double rotate)
        public void AddMeshFace(MeshGeometry3D mesh, ICellLocation location, AnchorFace face, double rotate, Point3D rotatePoint, Transform3D fixup = null)
        {
            var _current = GetBucket();
            _current.AddMeshFace(mesh, location, face, rotate, rotatePoint, fixup);
        }
        #endregion

        #region public void AddRectangularMesh(Rect rect, Vector textureSize, bool anchorTexture, Transform3D orient, params Vector3D[] bump)
        public void AddRectangularMesh(Rect rect, Vector textureSize, bool anchorTexture, AnchorFace face, bool forInner, params Vector3D[] bump)
        {
            if ((textureSize.X == 0) || (textureSize.Y == 0))
            {
                return;
            }

            var _current = GetBucket();
            _current.AddRectangularMesh(rect, textureSize, anchorTexture, face, forInner, bump);
        }
        #endregion

        public void AddRightTriangularMesh(Rect footPrint, TriangleCorner corner,
            Vector textureSize, bool anchorTexture, AnchorFace face, bool forInner, params Vector3D[] bump)
        {
            var _current = GetBucket();
            _current.AddRightTriangularMesh(footPrint, corner, textureSize, anchorTexture, face, forInner, bump);
        }
    }
}
