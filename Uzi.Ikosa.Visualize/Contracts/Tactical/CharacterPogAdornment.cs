using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public class CharacterPogAdornment : ModelAdornment
    {
        [DataMember]
        public bool IsSelf { get; set; }
        [DataMember]
        public bool IsTeam { get; set; }
        [DataMember]
        public bool HasInitiative { get; set; }

        #region static ctor()
        private static readonly Model3D[] _Pogs = new Model3D[6];

        private static GeometryModel3D BuildSolidPog(double height, int sides, Color top, Color side, Color center, Color bottom)
        {
            var _builder = new MeshBuilder();
            _builder.AddCone(new Point3D(), new Vector3D(0, 0, 1), 2.25, 2, height, true, true, sides + 1,
                Transform3D.Identity, Transform3D.Identity);
            var _mesh = _builder.ToMesh(true);

            // top and bottom caps each account for 20% of the gradient run via texture maps
            var _stops = new GradientStopCollection(7)
            {
                new GradientStop(top, 0),
                new GradientStop(top, 0.2),
                new GradientStop(side, 0.2),
                new GradientStop(center, 0.5),
                new GradientStop(side, 0.8),
                new GradientStop(bottom, 0.8),
                new GradientStop(bottom, 1)
            };
            var _mat = new DiffuseMaterial(new LinearGradientBrush(_stops, new Point(0.5, 0), new Point(0.5, 1)));
            _mat.Freeze();
            var _pog = new GeometryModel3D(_mesh, _mat);
            _pog.Freeze();
            return _pog;
        }

        private static Model3DGroup BuildSolidPointeredPog(double height, int sides, Color top, Color side, Color center, Color bottom, Color pointer, Color tail)
        {
            var _group = new Model3DGroup();

            // pointer
            var _builder = new MeshBuilder();
            _builder.AddArrow(new Point3D(0, -2.4, height), new Point3D(0, 2.4, height), height * 1.5, 2, 7);
            var _mesh = _builder.ToMesh(true);

            // material
            var _stops = new GradientStopCollection(7)
            {
                new GradientStop(tail, 0),
                new GradientStop(tail, 0.2),
                new GradientStop(pointer, 0.8),
                new GradientStop(pointer, 1)
            };
            var _mat = new DiffuseMaterial(new LinearGradientBrush(_stops, new Point(0.5, 0), new Point(0.5, 1)));
            _mat.Freeze();

            // pointer
            var _pointer = new GeometryModel3D(_mesh, _mat);
            _pointer.Freeze();
            _group.Children.Add(_pointer);

            _group.Children.Add(BuildSolidPog(height, sides, top, side, center, bottom));

            // freeze!
            _group.Freeze();
            return _group;
        }

        static CharacterPogAdornment()
        {
            // team
            _Pogs[0] = BuildSolidPog(0.2, 8, Colors.Silver, Colors.LightGray, Colors.White, Colors.Silver);
            _Pogs[1] = BuildSolidPog(0.2, 8, Colors.Silver, Colors.LightGreen, Colors.Green, Colors.Silver);

            // self
            _Pogs[2] = BuildSolidPointeredPog(0.225, 10, Colors.LightSteelBlue, Colors.LightSkyBlue, Colors.White, Colors.LightSteelBlue, Colors.DarkRed, Colors.DarkBlue);
            _Pogs[3] = BuildSolidPointeredPog(0.225, 10, Colors.LightSteelBlue, Colors.PaleGreen, Colors.Green, Colors.LightSteelBlue, Colors.Red, Colors.Blue);

            // etc...
            _Pogs[4] = BuildSolidPog(0.166, 5, Colors.DimGray, Colors.SlateGray, Colors.Black, Colors.DimGray);
            _Pogs[5] = BuildSolidPog(0.166, 5, Colors.DimGray, Colors.Black, Colors.Green, Colors.DimGray);
        }
        #endregion

        public override void DrawAdornment(Model3DGroup group)
        {
            if (IsSelf)
            {
                if (!HasInitiative)
                    group.Children.Add(_Pogs[2].Clone());
                else
                    group.Children.Add(_Pogs[3].Clone());
            }
            else if (IsTeam)
            {
                if (!HasInitiative)
                    group.Children.Add(_Pogs[0].Clone());
                else
                    group.Children.Add(_Pogs[1].Clone());
            }
            else
            {
                if (!HasInitiative)
                    group.Children.Add(_Pogs[4].Clone());
                else
                    group.Children.Add(_Pogs[5].Clone());
            }
        }
    }
}
