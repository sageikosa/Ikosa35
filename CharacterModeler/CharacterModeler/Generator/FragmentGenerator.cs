using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Linq;

namespace CharacterModeler.Generator
{
    public abstract class FragmentGenerator
    {
        protected abstract IEnumerable<XObject> WriteFragmentParts(XNamespace winfx, XNamespace uzi);

        public void WriteXml(XmlWriter writer)
        {
            // namespaces for fragment
            var _uspace = @"clr-namespace:Uzi.Visualize;assembly=Uzi.Visualize";
            var _wfspace = @"http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XNamespace _uzi = _uspace;
            XNamespace _winfx = _wfspace;

            var _children = WriteFragmentParts(_winfx, _uzi).ToList();

            // add uzi namespace!
            _children.Insert(0, new XAttribute(XNamespace.Xmlns + @"uzi", _uspace));

            // create group
            XElement _group = new XElement(_winfx + @"Model3DGroup", _children.ToArray());
            _group.WriteTo(writer);
        }

        public Func<Vector3D> GetOrigin()
        {
            return () => new Vector3D();
        }

        public Func<Vector3D> GetOffset(Func<Vector3D> source, Func<Vector3D> offset)
        {
            return () => source() + offset();
        }

        protected IEnumerable<XObject> WriteFromFlatList(XNamespace winfx, XNamespace uzi, params FragmentPart[] parts)
        {
            return parts.Select(_part => _part.GenerateElement(winfx, uzi));
        }
    }
}
