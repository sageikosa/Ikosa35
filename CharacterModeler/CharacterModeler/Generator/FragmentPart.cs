using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public abstract class FragmentPart
    {
        protected FragmentPart(Func<Vector3D> origin)
        {
            Origin = origin;
        }

        public Func<Vector3D> Origin { get; protected set; }

        protected TranslateTransform3D PointMover() { return new TranslateTransform3D(Origin()); }

        public Point3D MyPoint() { return new Point3D() + Origin(); }

        protected XAttribute GetMaterialAttribute(string tagName, string key)
        {
            return new XAttribute(tagName,
                string.Format(@"{{uzi:VisualEffectMaterial Key={0}, VisualEffect={{uzi:SenseEffectExtension}} }}", key));
        }

        #region protected string GetVector3DVal(Vector3D vector)
        protected string GetVector3DVal(Vector3D vector)
        {
            var _build = new StringBuilder();
            if (vector.X != 0)
                _build.AppendFormat(@"X={0}", vector.X);
            if (vector.Y != 0)
                _build.AppendFormat(@"{1}Y={0}", vector.Y, _build.Length > 0 ? @"," : string.Empty);
            if (vector.Z != 0)
                _build.AppendFormat(@"{1}Z={0}", vector.Z, _build.Length > 0 ? @"," : string.Empty);

            _build.Insert(0, @"{uzi:Vector3DVal ");
            _build.Append(@" }");
            return _build.ToString();
        }
        #endregion

        #region protected string GetPoint3DVal(Point3D point)
        protected string GetPoint3DVal(Point3D point)
        {
            var _build = new StringBuilder();
            if (point.X != 0)
                _build.AppendFormat(@"X={0}", point.X);
            if (point.Y != 0)
                _build.AppendFormat(@"{1}Y={0}", point.Y, _build.Length > 0 ? @"," : string.Empty);
            if (point.Z != 0)
                _build.AppendFormat(@"{1}Z={0}", point.Z, _build.Length > 0 ? @"," : string.Empty);

            _build.Insert(0, @"{uzi:Point3DVal ");
            _build.Append(@" }");
            return _build.ToString();
        }
        #endregion

        #region protected XElement ElementFromModel(XNamespace winfx, XNamespace uzi, GeometryModel3D model, string material, string backMaterial)
        protected XElement ElementFromModel(XNamespace winfx, XNamespace uzi, GeometryModel3D model, string material, string backMaterial)
        {
            // get XML representation of all geometry models
            // NOTE: "{{" and "}}" for embedded curly braces in format string
            var _meshGeometry3D = model.Geometry as MeshGeometry3D;

            // build XML for MeshGeometry3D
            var _texture = new XAttribute(@"TextureCoordinates", _meshGeometry3D.TextureCoordinates.ToString());
            var _positions = new XAttribute(@"Positions", _meshGeometry3D.Positions.ToString());
            var _triangles = new XAttribute(@"TriangleIndices", _meshGeometry3D.TriangleIndices.ToString());
            var _mesh = new XElement(winfx + @"MeshGeometry3D", _texture, _positions, _triangles);

            #region XAttribute for material (if defined)
            Func<string, string, XAttribute> _materialAttribute = (tagName, materialKey) =>
            {
                if (!string.IsNullOrEmpty(materialKey))
                {
                    return new XAttribute(tagName,
                                 string.Format(@"{{uzi:VisualEffectMaterial Key={0}, VisualEffect={{uzi:SenseEffectExtension}} }}",
                                 materialKey));
                }
                return null;
            };
            #endregion

            // build GeometryModel3D
            var _geom = new XElement(winfx + @"GeometryModel3D.Geometry", _mesh);
            var _material = _materialAttribute(@"Material", material);
            var _back = _materialAttribute(@"BackMaterial", backMaterial);

            if (_back != null)
                return new XElement(winfx + @"GeometryModel3D", _material, _back, _geom);
            else
                return new XElement(winfx + @"GeometryModel3D", _material, _geom);
        }
        #endregion

        /// <summary>Generate XElement for serialization</summary>
        public abstract XElement GenerateElement(XNamespace winfx, XNamespace uzi);

        /// <summary>Renders the Model3D fro immediate display</summary>
        public abstract Model3D RenderModel();

        public virtual Func<Vector3D> ConnectionPoint(string key)
        {
            return () => Origin();
        }

        public static Func<Vector3D> GetOffset(Func<Vector3D> source, Func<Vector3D> offset)
        {
            return () => source() + offset();
        }
    }
}
