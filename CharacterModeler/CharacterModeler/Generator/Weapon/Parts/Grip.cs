using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public class Grip : CylindricalPart, ITopRadiusProvider
    {
        public Grip(Func<Vector3D> origin, int meshKey)
            : base(origin, meshKey)
        {
        }

        #region ITopDimensionProvider Members
        public double Thickness { get { return Radius * 2; } }
        public double Width { get { return Radius * 2; } }
        #endregion

        protected override Point3D StartPoint()
        {
            return MyPoint();
        }

        protected override Point3D EndPoint()
        {
            return MyPoint() + new Vector3D(0, 0, Length);
        }

        protected override string MaterialKey { get { return @"Grip"; } }

        public override Model3D RenderModel()
        {
            throw new NotImplementedException();
        }

        public override Func<Vector3D> ConnectionPoint(string key)
        {
            switch (key)
            {
                case @"Top":
                    return () => Origin() + new Vector3D(0, 0, Length);
            }
            return base.ConnectionPoint(key);
        }
    }
}