using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.Windows.Markup;
using System.IO;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for ModelWindow.xaml
    /// </summary>
    public partial class ModelWindow : Window
    {
        const string PATH = @"C:\Uzi\XamlGeneratorTools\XAML\output\junk";
        //const string PATH = @"C:\Users\jousey\Source\Workspaces\XAML";
        const double SIDETHICK = 0.5d;
        const double TOPTHICK = 0.25d;

        // #FFFF0000 = Top
        DiffuseMaterial _Top = new DiffuseMaterial(Brushes.Red);
        // #FF00FFFF = Bottom
        DiffuseMaterial _Bottom = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)));
        // #FF00FF00 = Front
        DiffuseMaterial _Front = new DiffuseMaterial(Brushes.Lime);
        // #FFFF00FF = Back
        DiffuseMaterial _Back = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255, 255, 0, 255)));
        // #FF0000FF = Left
        DiffuseMaterial _Left = new DiffuseMaterial(Brushes.Blue);
        // #FFFFFF00 = Right
        DiffuseMaterial _Right = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)));
        DiffuseMaterial _Standard = new DiffuseMaterial(Brushes.Gray);
        DiffuseMaterial _Standard2 = new DiffuseMaterial(Brushes.DimGray);
        DiffuseMaterial _Standard3 = new DiffuseMaterial(Brushes.Silver);

        public ModelWindow()
        {
            InitializeComponent();
            //CreateBlock();
            //ClearAll();

            //CreateChair(true, @"railback", 0.5);
            //ClearAll();
            //CreateChair(true, @"halfback", 1.25);
            //ClearAll();
            //CreateChair(true, @"fullback");
            //ClearAll();

            //CreateChair(false, @"railback-al", 0.5);
            //ClearAll();
            //CreateChair(false, @"halfback-al", 1.25);
            //ClearAll();
            //CreateChair(false, @"fullback-al");
            //ClearAll();

            //CreateBookcase(3, false);
            //ClearAll();
            //CreateBookcase(4, true);
            //ClearAll();

            //CreateShelves(1);
            //ClearAll();
            //CreateShelves(2);
            //ClearAll();
            //CreateShelves(3);
            //ClearAll();
            //CreateShelves(4);
            //ClearAll();
            //CreateShelves(5);
            //ClearAll();

            //CreateSingleDoorCabinet(3);
            //ClearAll();

            //CreateDoubleDoorCabinet(3);
            //ClearAll();

            //CreateDesk(-2.5d);    // full-backed desk
            //ClearAll();
            //CreateDesk(0d);       // half-backed desk
            //ClearAll();
            //CreateDesk(null);     // no-backed desk
            //ClearAll();

            //CreateDrawers(1);
            //ClearAll();
            //CreateDrawers(2);
            //ClearAll();
            //CreateDrawers(3);
            //ClearAll();
            //CreateDrawers(4);
            //ClearAll();

            //CreateThrone();
            //ClearAll();

            //CreateTable();
            //ClearAll();

            //CreateBars(6);
            //ClearAll();
            //CreateBars(8);
            //ClearAll();
            //CreateBars(10);
            //ClearAll();
            //CreateOpenTopBox();
            //ClearAll();
            CreateCart();
        }

        private void ClearAll()
        {
            m3grpSet.Children.Clear();
            _Keys.Clear();
        }

        private Queue<string> _Keys = new Queue<string>();

        private void AddKeys(string keyVal, int count)
        {
            for (var _kx = 0; _kx < count; _kx++)
                _Keys.Enqueue(keyVal);
        }

        #region private void SaveFile(string fileName)
        private void SaveFile(string fileName)
        {
            // create first pass
            var _tmp = $@"{fileName}_tmp";
            using (var _file = File.Create(_tmp))
            {
                XamlWriter.Save(m3grpSet, _file);
            }

            // load XAML
            var _contents = File.ReadAllText(_tmp);

            // add namespace
            _contents = _contents.Replace(
                @"/winfx/2006/xaml/presentation"">",
                @"/winfx/2006/xaml/presentation"" xmlns:uzi=""clr-namespace:Uzi.Visualize;assembly=Uzi.Visualize"">");

            // replace all brush references
            void _brushReplace(string color, string effect)
            {
                _contents = _contents.Replace(
                    $@"<DiffuseMaterial Brush=""{color}"" />",
                    $@"<uzi:VisualEffectMaterial Key=""placeholder"" VisualEffect=""{effect}"" />");
            }
            _brushReplace(@"#FFFF0000", @"{uzi:TopSenseEffectExtension}");
            _brushReplace(@"#FF00FFFF", @"{uzi:BottomSenseEffectExtension}");
            _brushReplace(@"#FF00FF00", @"{uzi:FrontSenseEffectExtension}");
            _brushReplace(@"#FFFF00FF", @"{uzi:BackSenseEffectExtension}");
            _brushReplace(@"#FF0000FF", @"{uzi:LeftSenseEffectExtension}");
            _brushReplace(@"#FFFFFF00", @"{uzi:RightSenseEffectExtension}");
            _brushReplace(@"#FF808080", @"{uzi:SenseEffectExtension}");
            _brushReplace(@"#FF696969", @"{uzi:SenseEffectExtension}");
            _brushReplace(@"#FFC0C0C0", @"{uzi:SenseEffectExtension}");

            // replace all angle references
            void _angleReplace(int angle, string key)
            {
                _contents = _contents.Replace(
                    $@"Angle=""{angle}""",
                    $@"Angle=""{key}""");
            }
            _angleReplace(-1, @"{uzi:DoubleProduct A=-90,B={uzi:ExternalVal Key=Open.1,Value=0}}");
            _angleReplace(-2, @"{uzi:DoubleProduct A=-90,B={uzi:ExternalVal Key=Open.2,Value=0}}");

            // 1: split string on placeholder
            var _split = _contents.Split(new[] { @"Key=""placeholder""" }, StringSplitOptions.None);

            // 2: interleave "proper" keys in gaps where placeholder used to be
            var _builder = new StringBuilder();
            foreach (var _part in _split.Take(_split.Length - 1))
            {
                _builder.Append(_part);
                _builder.Append($@"Key=""{_Keys.Dequeue()}""");
            }
            _builder.Append(_split.Last());

            // write final file
            File.WriteAllText(fileName, _builder.ToString());

            // cleanup tmp
            File.Delete(_tmp);
        }
        #endregion

        #region public void CreateBlock()
        public void CreateBlock()
        {
            m3grpSet.Children.Add(CreateLeft(-2.5, 2.5, -2.5, 2.5, 2.5, _Left));
            m3grpSet.Children.Add(CreateRight(-2.5, 2.5, -2.5, 2.5, -2.5, _Right));
            m3grpSet.Children.Add(CreateTop(-2.5, 2.5, -2.5, 2.5, 2.5, _Top));
            m3grpSet.Children.Add(CreateBottom(-2.5, 2.5, -2.5, 2.5, -2.5, _Bottom));
            m3grpSet.Children.Add(CreateFront(-2.5, 2.5, -2.5, 2.5, -2.5, _Front));
            m3grpSet.Children.Add(CreateBack(-2.5, 2.5, -2.5, 2.5, 2.5, _Back));
            AddKeys(@"Left", 1);
            AddKeys(@"Right", 1);
            AddKeys(@"Top", 1);
            AddKeys(@"Bottom", 1);
            AddKeys(@"Front", 1);
            AddKeys(@"Back", 1);
            SaveFile($@"{PATH}\block-test.xaml");
        }
        #endregion

        #region desk
        private void CreateDesk(double? backZLow)
        {
            var _topZ = 2.5d;
            var _topBase = _topZ - TOPTHICK;

            // Top
            var _top = new Model3DGroup();
            _top.Children.Add(CreateTop(-2.5, 2.5, -2.5, 2.5, _topZ, _Top));
            _top.Children.Add(CreateBottom(-2.5, 2.5, -2.5, 2.5, _topBase, _Standard));
            _top.Children.Add(CreateFront(-2.5, 2.5, _topBase, _topZ, -2.5, _Front));
            _top.Children.Add(CreateLeft(-2.5, 2.5, _topBase, _topZ, 2.5, _Left));
            _top.Children.Add(CreateRight(-2.5, 2.5, _topBase, _topZ, -2.5, _Right));
            if (!backZLow.HasValue)
                _top.Children.Add(CreateBack(-2.5, 2.5, _topBase, _topZ, 2.5, _Back));
            m3grpSet.Children.Add(_top);
            AddKeys(@"Top", !backZLow.HasValue ? 6 : 5);

            // left side
            var _left = new Model3DGroup();
            _left.Children.Add(CreateLeft(-2.5, 2.5, -2.5, _topBase, 2.5, _Left));
            _left.Children.Add(CreateRight(-2.5, 2.5, -2.5, _topBase, 2.5 - SIDETHICK, _Standard));
            _left.Children.Add(CreateFront(2.5 - SIDETHICK, 2.5, -2.5, _topBase, -2.5, _Front));
            _left.Children.Add(CreateBottom(2.5 - SIDETHICK, 2.5, -2.5, 2.5, -2.5, _Bottom));
            if (!backZLow.HasValue)
            {
                _left.Children.Add(CreateBack(2.5 - SIDETHICK, 2.5, -2.5, _topBase, 2.5, _Back));
                AddKeys(@"Side", 5);
            }
            else if (backZLow > -2.5)
            {
                _left.Children.Add(CreateBack(2.5 - SIDETHICK, 2.5, -2.5, backZLow ?? _topBase, 2.5, _Back));
                AddKeys(@"Side", 5);
            }
            else
            {
                AddKeys(@"Side", 4);
            }
            m3grpSet.Children.Add(_left);

            // right side
            var _right = new Model3DGroup();
            _right.Children.Add(CreateLeft(-2.5, 2.5, -2.5, _topBase, -2.5 + SIDETHICK, _Standard));
            _right.Children.Add(CreateRight(-2.5, 2.5, -2.5, _topBase, -2.5, _Right));
            _right.Children.Add(CreateFront(-2.5, -2.5 + SIDETHICK, -2.5, _topBase, -2.5, _Front));
            _right.Children.Add(CreateBottom(-2.5, -2.5 + SIDETHICK, -2.5, 2.5, -2.5, _Bottom));
            if (!backZLow.HasValue)
            {
                _right.Children.Add(CreateBack(-2.5, -2.5 + SIDETHICK, -2.5, _topBase, 2.5, _Back));
                AddKeys(@"Side", 5);
            }
            else if (backZLow > -2.5)
            {
                _right.Children.Add(CreateBack(-2.5, -2.5 + SIDETHICK, -2.5, backZLow ?? _topBase, 2.5, _Back));
                AddKeys(@"Side", 5);
            }
            else
            {
                AddKeys(@"Side", 4);
            }
            m3grpSet.Children.Add(_right);

            if (backZLow.HasValue)
            {
                var _back = new Model3DGroup();
                _back.Children.Add(CreateBack(-2.5, 2.5, backZLow ?? -2.5d, 2.5, 2.5, _Back));
                _back.Children.Add(CreateBottom(-2.5, 2.5, 2.5 - SIDETHICK, 2.5, backZLow ?? -2.5d, (backZLow > -2.5) ? _Standard : _Bottom));
                _back.Children.Add(CreateFront(-2.5, 2.5, backZLow ?? -2.5d, _topBase, 2.5 - SIDETHICK, _Standard));
                AddKeys(@"Back", 3);
                m3grpSet.Children.Add(_back);
            }

            SaveFile($@"{PATH}\desk_{backZLow}.xaml");
        }
        #endregion

        #region single door cabinet
        private void CreateSingleDoorCabinet(int shelfCount)
        {
            var _max = 2.5 - SIDETHICK;
            var _min = -2.5 + SIDETHICK;

            CreateBookcase(shelfCount, true, false);

            // door
            var _door = new Model3DGroup();
            _door.Children.Add(CreateBack(_min, _max, _min, _max, _min, _Standard));
            _door.Children.Add(CreateFront(_min, _max, _min, _max, -2.5, _Front));
            _door.Children.Add(CreateLeft(-2.5, _min, _min, _max, _max, _Standard2));
            _door.Children.Add(CreateTop(_min, _max, -2.5, _min, _max, _Standard3));
            _door.Children.Add(CreateBottom(_min, _max, -2.5, _min, _min, _Standard3));
            AddKeys(@"Door.Inside", 1);
            AddKeys(@"Door", 1);
            AddKeys(@"Door.Edge", 1);
            AddKeys(@"Door.TopBottom", 2);
            _door.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), -1), new Point3D(_min, _min, 0));
            m3grpSet.Children.Add(_door);
            // <AxisAngleRotation3D Axis="0,0,1" Angle="{uzi:DoubleProduct A=-90, B={uzi:ExternalVal Key=Open.1, Value=0}}" />

            SaveFile($@"{PATH}\cabinet_1_{shelfCount}.xaml");
        }
        #endregion

        #region double door cabinet
        private void CreateDoubleDoorCabinet(int shelfCount)
        {
            var _max = 2.5 - SIDETHICK;
            var _min = -2.5 + SIDETHICK;

            CreateBookcase(shelfCount, true, false);

            // door
            var _door1 = new Model3DGroup();
            _door1.Children.Add(CreateBack(_min, 0, _min, _max, _min, _Standard));
            _door1.Children.Add(CreateFront(_min, 0, _min, _max, -2.5, _Front));

            _door1.Children.Add(CreateTop(_min, 0, -2.5, _min, _max, _Standard3));
            _door1.Children.Add(CreateBottom(_min, 0, -2.5, _min, _min, _Standard3));
            _door1.Children.Add(CreateLeft(-2.5, _min, _min, _max, 0, _Standard2));
            AddKeys(@"Door1.Inside", 1);
            AddKeys(@"Door1", 1);
            AddKeys(@"Door1.TopBottom", 2);
            AddKeys(@"Door1.Edge", 1);
            _door1.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), -1), new Point3D(_min, _min, 0));
            m3grpSet.Children.Add(_door1);
            // <AxisAngleRotation3D Axis="0,0,1" Angle="{uzi:DoubleProduct A=-90, B={uzi:ExternalVal Key=Open.1, Value=0}}" />

            // door
            var _door2 = new Model3DGroup();
            _door2.Children.Add(CreateBack(0, _max, _min, _max, _min, _Standard));
            _door2.Children.Add(CreateFront(0, _max, _min, _max, -2.5, _Front));

            _door2.Children.Add(CreateTop(0, _max, -2.5, _min, _max, _Standard3));
            _door2.Children.Add(CreateBottom(0, _max, -2.5, _min, _min, _Standard3));
            _door2.Children.Add(CreateRight(-2.5, _min, _min, _max, 0, _Standard2));
            AddKeys(@"Door2.Inside", 1);
            AddKeys(@"Door2", 1);
            AddKeys(@"Door2.TopBottom", 2);
            AddKeys(@"Door2.Edge", 1);
            _door2.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, -1), -1), new Point3D(_max, _min, 0));
            m3grpSet.Children.Add(_door2);
            // <AxisAngleRotation3D Axis="0,0,-1" Angle="{uzi:DoubleProduct A=-90, B={uzi:ExternalVal Key=Open.1, Value=0}}" />

            SaveFile($@"{PATH}\cabinet_2_{shelfCount}.xaml");
        }
        #endregion

        #region chair
        private void CreateChair(bool arms, string name, double chairBackThick = 2.5d)
        {
            var _legThick = 0.25d;

            var _topZ = 2.5d;
            var _topBase = _topZ - TOPTHICK;
            var _topSeat = 0d;
            var _seatBase = _topSeat - TOPTHICK;
            var _topArm = 1.25d;
            var _topLeg = arms ? _topArm - TOPTHICK : _topSeat + 0.025;
            var _chairBackBase = 2.5d - chairBackThick;

            // seat
            var _seat = new Model3DGroup();
            _seat.Children.Add(CreateTop(-2.475, 2.475, -2.475, 2.475, _topSeat, _Standard2));
            _seat.Children.Add(CreateBottom(-2.475, 2.475, -2.475, 2.475, _seatBase, _Standard2));
            _seat.Children.Add(CreateFront(-2.475, 2.475, _seatBase, _topSeat, -2.475, _Front));
            _seat.Children.Add(CreateBack(-2.475, 2.475, _seatBase, _topSeat, 2.475, _Back));
            _seat.Children.Add(CreateLeft(-2.475, 2.475, _seatBase, _topSeat, 2.475, _Left));
            _seat.Children.Add(CreateRight(-2.475, 2.475, _seatBase, _topSeat, -2.475, _Right));
            AddKeys(@"Seat", 1);
            AddKeys(@"SeatStruc", 3);
            AddKeys(@"SeatSide", 2);
            m3grpSet.Children.Add(_seat);

            // chair back
            var _back = new Model3DGroup();
            _back.Children.Add(CreateBack(-2.5 + _legThick, 2.5 - _legThick, _chairBackBase, 2.5, 2.475, _Back));
            _back.Children.Add(CreateFront(-2.5 + _legThick, 2.5 - _legThick, _chairBackBase, 2.5, 2.275, _Standard));
            _back.Children.Add(CreateTop(-2.5 + _legThick, 2.5 - _legThick, 2.275, 2.475, 2.5, _Top));
            _back.Children.Add(CreateBottom(-2.5 + _legThick, 2.5 - _legThick, 2.275, 2.475, _chairBackBase, _Standard2));
            AddKeys(@"SeatBack", 1);
            AddKeys(@"Back", 3);
            m3grpSet.Children.Add(_back);

            // front legs
            var _frontLeg1 = new Model3DGroup();
            _frontLeg1.Children.Add(CreateBottom(-2.5, -2.5 + _legThick, -2.5, -2.5 + _legThick, -2.5, _Bottom));
            _frontLeg1.Children.Add(CreateFront(-2.5, -2.5 + _legThick, -2.5, _topLeg, -2.5, _Front));
            _frontLeg1.Children.Add(CreateBack(-2.5, -2.5 + _legThick, -2.5, _topLeg, -2.5 + _legThick, _Standard));
            _frontLeg1.Children.Add(CreateLeft(-2.5, -2.5 + _legThick, -2.5, _topLeg, -2.5 + _legThick, _Standard));
            _frontLeg1.Children.Add(CreateRight(-2.5, -2.5 + _legThick, -2.5, _topLeg, -2.5, _Right));
            if (!arms)
            {
                _frontLeg1.Children.Add(CreateTop(-2.5, -2.5 + _legThick, -2.5, -2.5 + _legThick, _topLeg, _Standard2));
            }
            AddKeys(@"Leg", arms ? 5 : 6);
            m3grpSet.Children.Add(_frontLeg1);

            var _frontLeg2 = new Model3DGroup();
            _frontLeg2.Children.Add(CreateBottom(2.5 - _legThick, 2.5, -2.5, -2.5 + _legThick, -2.5, _Bottom));
            _frontLeg2.Children.Add(CreateFront(2.5 - _legThick, 2.5, -2.5, _topLeg, -2.5, _Front));
            _frontLeg2.Children.Add(CreateBack(2.5 - _legThick, 2.5, -2.5, _topLeg, -2.5 + _legThick, _Standard));
            _frontLeg2.Children.Add(CreateLeft(-2.5, -2.5 + _legThick, -2.5, _topLeg, 2.5, _Left));
            _frontLeg2.Children.Add(CreateRight(-2.5, -2.5 + _legThick, -2.5, _topLeg, 2.5 - _legThick, _Standard));
            if (!arms)
            {
                _frontLeg1.Children.Add(CreateTop(2.5 - _legThick, 2.5, -2.5, -2.5 + _legThick, _topLeg, _Standard2));
            }
            AddKeys(@"Leg", arms ? 5 : 6);
            m3grpSet.Children.Add(_frontLeg2);

            // back legs
            var _backLeg1 = new Model3DGroup();
            _backLeg1.Children.Add(CreateTop(-2.5, -2.5 + _legThick, 2.5 - _legThick, 2.5, _topZ, _Top));
            _backLeg1.Children.Add(CreateBottom(-2.5, -2.5 + _legThick, 2.5 - _legThick, 2.5, -2.5, _Bottom));
            _backLeg1.Children.Add(CreateFront(-2.5, -2.5 + _legThick, -2.5, _topZ, 2.5 - _legThick, _Standard));
            _backLeg1.Children.Add(CreateBack(-2.5, -2.5 + _legThick, -2.5, _topZ, 2.5, _Back));
            _backLeg1.Children.Add(CreateLeft(2.5 - _legThick, 2.5, -2.5, _topZ, -2.5 + _legThick, _Standard));
            _backLeg1.Children.Add(CreateRight(2.5 - _legThick, 2.5, -2.5, _topZ, -2.5, _Right));
            AddKeys(@"Leg", 6);
            m3grpSet.Children.Add(_backLeg1);

            var _backLeg2 = new Model3DGroup();
            _backLeg2.Children.Add(CreateTop(2.5 - _legThick, 2.5, 2.5 - _legThick, 2.5, _topZ, _Top));
            _backLeg2.Children.Add(CreateBottom(2.5 - _legThick, 2.5, 2.5 - _legThick, 2.5, -2.5, _Bottom));
            _backLeg2.Children.Add(CreateFront(2.5 - _legThick, 2.5, -2.5, _topZ, 2.5 - _legThick, _Standard));
            _backLeg2.Children.Add(CreateBack(2.5 - _legThick, 2.5, -2.5, _topZ, 2.5, _Back));
            _backLeg2.Children.Add(CreateLeft(2.5 - _legThick, 2.5, -2.5, _topZ, 2.5, _Left));
            _backLeg2.Children.Add(CreateRight(2.5 - _legThick, 2.5, -2.5, _topZ, 2.5 - _legThick, _Standard));
            AddKeys(@"Leg", 6);
            m3grpSet.Children.Add(_backLeg2);

            // arms
            if (arms)
            {
                var _arm1 = new Model3DGroup();
                _arm1.Children.Add(CreateTop(-2.5, -2.5 + _legThick, -2.5, 2.5, _topArm, _Standard3));
                _arm1.Children.Add(CreateBottom(-2.5, -2.5 + _legThick, -2.5, 2.5, _topLeg, _Standard3));
                _arm1.Children.Add(CreateLeft(-2.5, 2.5 - _legThick, _topLeg, _topArm, -2.5 + _legThick, _Standard2));
                _arm1.Children.Add(CreateRight(-2.5, 2.5 - _legThick, _topLeg, _topArm, -2.5, _Right));
                _arm1.Children.Add(CreateFront(-2.5, -2.5 + _legThick, _topLeg, _topArm, -2.5, _Front));
                m3grpSet.Children.Add(_arm1);
                AddKeys(@"Arm", 5);

                var _arm2 = new Model3DGroup();
                _arm2.Children.Add(CreateTop(2.5 - _legThick, 2.5, -2.5, 2.5, _topArm, _Standard3));
                _arm2.Children.Add(CreateBottom(2.5 - _legThick, 2.5, -2.5, 2.5, _topLeg, _Standard3));
                _arm2.Children.Add(CreateLeft(-2.5, 2.5 - _legThick, _topLeg, _topArm, 2.5, _Left));
                _arm2.Children.Add(CreateRight(-2.5, 2.5 - _legThick, _topLeg, _topArm, 2.5 - _legThick, _Standard2));
                _arm2.Children.Add(CreateFront(2.5 - _legThick, 2.5, _topLeg, _topArm, -2.5, _Front));
                m3grpSet.Children.Add(_arm2);
                AddKeys(@"Arm", 5);
            }

            // rail backs
            if (chairBackThick < 1)
            {
                var _rail = new Model3DGroup();
                _rail.Children.Add(CreateFront(0 - _legThick * 2, 0 + _legThick * 2, _topSeat, _chairBackBase, 2.525 - _legThick, _Standard2));
                _rail.Children.Add(CreateBack(0 - _legThick * 2, 0 + _legThick * 2, _topSeat, _chairBackBase, 2.475, _Back));
                _rail.Children.Add(CreateLeft(2.525 - _legThick, 2.475, _topSeat, _chairBackBase, 0 + _legThick * 2, _Standard3));
                _rail.Children.Add(CreateRight(2.525 - _legThick, 2.475, _topSeat, _chairBackBase, 0 - _legThick * 2, _Standard));
                AddKeys(@"Back2", 4);
                m3grpSet.Children.Add(_rail);
            }

            SaveFile($@"{PATH}\chair_{name}.xaml");
        }
        #endregion

        #region throne
        private void CreateThrone()
        {
            var _legThick = 0.25d;

            var _topZ = 2.5d;
            var _topBase = _topZ - TOPTHICK;
            var _topSeat = 0d;
            var _seatBase = -2.45d;
            var _topArm = 1.25d;
            var _topLeg = _topArm - TOPTHICK;
            var _chairBackBase = -2.5d;

            // seat
            var _seat = new Model3DGroup();
            _seat.Children.Add(CreateTop(-2.45, 2.45, -2.45, 2.45, _topSeat, _Standard2));
            _seat.Children.Add(CreateBottom(-2.45, 2.45, -2.45, 2.45, _seatBase, _Bottom));
            _seat.Children.Add(CreateFront(-2.45, 2.45, _seatBase, _topSeat, -2.45, _Front));
            AddKeys(@"Seat", 1);
            AddKeys(@"Bottom", 1);
            AddKeys(@"Front", 1);
            m3grpSet.Children.Add(_seat);

            // chair back
            var _back = new Model3DGroup();
            _back.Children.Add(CreateBack(-2.5 + _legThick, 2.5 - _legThick, _chairBackBase, 2.5, 2.45, _Back));
            _back.Children.Add(CreateFront(-2.5 + _legThick, 2.5 - _legThick, _chairBackBase, 2.5, 2.275, _Standard));
            _back.Children.Add(CreateTop(-2.5 + _legThick, 2.5 - _legThick, 2.275, 2.45, 2.5, _Top));
            _back.Children.Add(CreateBottom(-2.5 + _legThick, 2.5 - _legThick, 2.525 - _legThick, 2.45, _chairBackBase, _Bottom));
            AddKeys(@"Back", 1);
            AddKeys(@"SeatBack", 1);
            AddKeys(@"Back", 2);
            m3grpSet.Children.Add(_back);

            // back legs
            var _backLeg1 = new Model3DGroup();
            _backLeg1.Children.Add(CreateTop(-2.5, -2.5 + _legThick, 2.5 - _legThick, 2.5, _topZ, _Top));
            _backLeg1.Children.Add(CreateFront(-2.5, -2.5 + _legThick, -2.5, _topZ, 2.5 - _legThick, _Standard));
            _backLeg1.Children.Add(CreateBack(-2.5, -2.5 + _legThick, -2.5, _topZ, 2.5, _Back));
            _backLeg1.Children.Add(CreateLeft(2.5 - _legThick, 2.5, -2.5, _topZ, -2.5 + _legThick, _Standard));
            _backLeg1.Children.Add(CreateRight(2.5 - _legThick, 2.5, -2.5, _topZ, -2.5, _Right));
            AddKeys(@"Leg", 5);
            m3grpSet.Children.Add(_backLeg1);

            var _backLeg2 = new Model3DGroup();
            _backLeg2.Children.Add(CreateTop(2.5 - _legThick, 2.5, 2.5 - _legThick, 2.5, _topZ, _Top));
            _backLeg2.Children.Add(CreateFront(2.5 - _legThick, 2.5, -2.5, _topZ, 2.5 - _legThick, _Standard));
            _backLeg2.Children.Add(CreateBack(2.5 - _legThick, 2.5, -2.5, _topZ, 2.5, _Back));
            _backLeg2.Children.Add(CreateLeft(2.5 - _legThick, 2.5, -2.5, _topZ, 2.5, _Left));
            _backLeg2.Children.Add(CreateRight(2.5 - _legThick, 2.5, -2.5, _topZ, 2.5 - _legThick, _Standard));
            AddKeys(@"Leg", 5);
            m3grpSet.Children.Add(_backLeg2);

            // sides
            var _arm1 = new Model3DGroup();
            _arm1.Children.Add(CreateTop(-2.5, -2.5 + _legThick, -2.5, 2.5, _topArm, _Standard3));
            _arm1.Children.Add(CreateBottom(-2.5, -2.5 + _legThick, -2.5, 2.5, -2.5, _Bottom));
            _arm1.Children.Add(CreateLeft(-2.5, 2.5 - _legThick, -2.5, _topArm, -2.5 + _legThick, _Standard2));
            _arm1.Children.Add(CreateRight(-2.5, 2.5 - _legThick, -2.5, _topArm, -2.5, _Right));
            _arm1.Children.Add(CreateFront(-2.5, -2.5 + _legThick, -2.5, _topArm, -2.5, _Front));
            m3grpSet.Children.Add(_arm1);
            AddKeys(@"ArmRest", 1);
            AddKeys(@"Side", 3);
            AddKeys(@"ArmFront", 1);

            var _arm2 = new Model3DGroup();
            _arm2.Children.Add(CreateTop(2.5 - _legThick, 2.5, -2.5, 2.5, _topArm, _Standard3));
            _arm2.Children.Add(CreateBottom(2.5 - _legThick, 2.5, -2.5, 2.5, -2.5, _Bottom));
            _arm2.Children.Add(CreateLeft(-2.5, 2.5 - _legThick, -2.5, _topArm, 2.5, _Left));
            _arm2.Children.Add(CreateRight(-2.5, 2.5 - _legThick, -2.5, _topArm, 2.5 - _legThick, _Standard2));
            _arm2.Children.Add(CreateFront(2.5 - _legThick, 2.5, -2.5, _topArm, -2.5, _Front));
            m3grpSet.Children.Add(_arm2);
            AddKeys(@"ArmRest", 1);
            AddKeys(@"Side", 3);
            AddKeys(@"ArmFront", 1);

            SaveFile($@"{PATH}\throne.xaml");
        }
        #endregion

        #region bookcase
        private void CreateBookcase(int shelfCount, bool backWall, bool noDoor = true)
        {
            var _shelfThick = 0.125d;
            var _topBase = 2.5 - SIDETHICK;
            var _bottomCap = -2.5 + SIDETHICK;

            // left side
            var _left = new Model3DGroup();
            _left.Children.Add(CreateLeft(-2.5, 2.5, _bottomCap, _topBase, 2.5, _Left));
            _left.Children.Add(CreateRight(-2.5, 2.5, _bottomCap, _topBase, 2.5 - SIDETHICK, _Standard));
            _left.Children.Add(CreateFront(2.5 - SIDETHICK, 2.5, _bottomCap, _topBase, -2.5, _Front));
            AddKeys(@"Side", 2);
            AddKeys(@"Side.Front", 1);
            if (!backWall)
            {
                _left.Children.Add(CreateBack(2.5 - SIDETHICK, 2.5, _bottomCap, _topBase, 2.5, _Back));
                AddKeys(@"Side.Back", 1);
            }
            m3grpSet.Children.Add(_left);

            // right side
            var _right = new Model3DGroup();
            _right.Children.Add(CreateLeft(-2.5, 2.5, _bottomCap, _topBase, -2.5 + SIDETHICK, _Standard));
            _right.Children.Add(CreateRight(-2.5, 2.5, _bottomCap, _topBase, -2.5, _Right));
            _right.Children.Add(CreateFront(-2.5, -2.5 + SIDETHICK, _bottomCap, _topBase, -2.5, _Front));
            AddKeys(@"Side", 2);
            AddKeys(@"Side.Front", 1);
            if (!backWall)
            {
                _right.Children.Add(CreateBack(-2.5, -2.5 + SIDETHICK, _bottomCap, _topBase, 2.5, _Back));
                AddKeys(@"Side.Back", 1);
            }
            m3grpSet.Children.Add(_right);

            // top
            var _top = new Model3DGroup();
            _top.Children.Add(CreateTop(-2.5, 2.5, -2.5, 2.5, 2.5, _Top));
            _top.Children.Add(CreateBottom(-2.5, 2.5, -2.5, 2.5, _topBase, _Standard));
            _top.Children.Add(CreateFront(-2.5, 2.5, _topBase, 2.5, -2.5, _Front));
            _top.Children.Add(CreateLeft(-2.5, 2.5, _topBase, 2.5, 2.5, _Left));
            _top.Children.Add(CreateRight(-2.5, 2.5, _topBase, 2.5, -2.5, _Right));
            AddKeys(@"Top", 2);
            AddKeys(@"Top.Front", 1);
            AddKeys(@"Top.Side", 2);
            if (!backWall)
            {
                _top.Children.Add(CreateBack(-2.5, 2.5, _topBase, 2.5, 2.5, _Back));
                AddKeys(@"Top.Back", 1);
            }
            m3grpSet.Children.Add(_top);

            // bottom
            var _bottom = new Model3DGroup();
            _bottom.Children.Add(CreateTop(-2.5, 2.5, -2.5, 2.5, _bottomCap, _Standard));
            _bottom.Children.Add(CreateBottom(-2.5, 2.5, -2.5, 2.5, -2.5, _Bottom));
            _bottom.Children.Add(CreateFront(-2.5, 2.5, -2.5, _bottomCap, -2.5, _Front));
            _bottom.Children.Add(CreateLeft(-2.5, 2.5, -2.5, _bottomCap, 2.5, _Left));
            _bottom.Children.Add(CreateRight(-2.5, 2.5, -2.5, _bottomCap, -2.5, _Right));
            AddKeys(@"Bottom", 2);
            AddKeys(@"Bottom.Front", 1);
            AddKeys(@"Bottom.Side", 2);
            if (!backWall)
            {
                _bottom.Children.Add(CreateBack(-2.5, 2.5, -2.5, _bottomCap, 2.5, _Back));
                AddKeys(@"Bottom.Back", 1);
            }
            m3grpSet.Children.Add(_bottom);

            // shelves
            var _shelfLow = -2.5 + SIDETHICK;
            var _shelfHigh = 2.5 - SIDETHICK;
            var _shelfFront = noDoor ? -2.5 : -2.5 + SIDETHICK;
            var _shelfList = StepThrough(-2.5, 2.5, 5d / shelfCount).Skip(1).TakeWhile(_b => _b < 2.5).ToArray();
            foreach (var _z in _shelfList)
            {
                var _shelf = new Model3DGroup();
                _shelf.Children.Add(CreateFront(_shelfLow, _shelfHigh, _z - SIDETHICK, _z, _shelfFront, _Front));
                AddKeys(@"Shelf.Front", 1);
                if (!backWall)
                {
                    _shelf.Children.Add(CreateBack(_shelfLow, _shelfHigh, _z - SIDETHICK, _z, 2.5, _Back));
                    AddKeys(@"Shelf.Back", 1);
                }
                _shelf.Children.Add(CreateTop(_shelfLow, _shelfHigh, _shelfFront, 2.5, _z, _Standard2));
                _shelf.Children.Add(CreateBottom(_shelfLow, _shelfHigh, _shelfFront, 2.5, _z - _shelfThick, _Standard2));
                m3grpSet.Children.Add(_shelf);
                AddKeys(@"Shelf", 2);
            }

            if (backWall)
            {
                var _back = new Model3DGroup();
                _back.Children.Add(CreateBack(-2.5, 2.5, -2.5, 2.5, 2.5, _Back));
                _back.Children.Add(CreateFront(-2.5, 2.5, -2.5, 2.5, 2.5 - SIDETHICK, _Standard));
                AddKeys(@"Back", 2);
                m3grpSet.Children.Add(_back);
            }

            if (noDoor)
            {
                SaveFile($@"{PATH}\bookcase_{shelfCount}.xaml");
            }
        }
        #endregion

        #region shelves
        private void CreateShelves(int shelfCount)
        {
            var _shelfThick = 0.125d;
            // wall mounted shelves
            var _shelfList = StepThrough(-2.5 + _shelfThick, 2.5, (5d - 2 * _shelfThick) / (shelfCount - 1)).ToArray();
            foreach (var _z in _shelfList)
            {
                var _shelf = new Model3DGroup();
                _shelf.Children.Add(CreateFront(-2.5, 2.5, _z - _shelfThick, _z, -2.5, _Front));
                _shelf.Children.Add(CreateBack(-2.5, 2.5, _z - _shelfThick, _z, 2.5, _Back));
                _shelf.Children.Add(CreateTop(-2.5, 2.5, -2.5, 2.5, _z, _Standard2));
                _shelf.Children.Add(CreateBottom(-2.5, 2.5, -2.5, 2.5, _z - _shelfThick, _Standard2));
                _shelf.Children.Add(CreateLeft(-2.5, 2.5, _z - _shelfThick, _z, 2.5, _Left));
                _shelf.Children.Add(CreateRight(-2.5, 2.5, _z - _shelfThick, _z, -2.5, _Right));
                m3grpSet.Children.Add(_shelf);
            }
            AddKeys(@"Shelf", 6 * shelfCount);

            SaveFile($@"{PATH}\free_Shelves_{shelfCount}.xaml");
        }
        #endregion

        #region table
        private void CreateTable()
        {
            var _legThick = 0.25d;
            var _topZ = 2.5d;
            var _bottomZ = _topZ - TOPTHICK;

            // Top
            var _top = new Model3DGroup();
            _top.Children.Add(CreateTop(-2.5, 2.5, -2.5, 2.5, _topZ, _Top));
            _top.Children.Add(CreateBottom(-2.5, 2.5, -2.5, 2.5, _bottomZ, _Standard));
            _top.Children.Add(CreateFront(-2.5, 2.5, _bottomZ, _topZ, -2.5, _Front));
            _top.Children.Add(CreateBack(-2.5, 2.5, _bottomZ, _topZ, 2.5, _Back));
            _top.Children.Add(CreateLeft(-2.5, 2.5, _bottomZ, _topZ, 2.5, _Left));
            _top.Children.Add(CreateRight(-2.5, 2.5, _bottomZ, _topZ, -2.5, _Right));
            AddKeys(@"Top", 6);
            m3grpSet.Children.Add(_top);

            // leg(s)
            var _leg1 = new Model3DGroup();
            _leg1.Children.Add(CreateBottom(-2.5, -2.5 + _legThick, -2.5, -2.5 + _legThick, -2.5, _Bottom));
            _leg1.Children.Add(CreateFront(-2.5, -2.5 + _legThick, -2.5, _bottomZ, -2.5, _Front));
            _leg1.Children.Add(CreateBack(-2.5, -2.5 + _legThick, -2.5, _bottomZ, -2.5 + _legThick, _Standard));
            _leg1.Children.Add(CreateLeft(-2.5, -2.5 + _legThick, -2.5, _bottomZ, -2.5 + _legThick, _Standard));
            _leg1.Children.Add(CreateRight(-2.5, -2.5 + _legThick, -2.5, _bottomZ, -2.5, _Right));
            m3grpSet.Children.Add(_leg1);

            var _leg2 = new Model3DGroup();
            _leg2.Children.Add(CreateBottom(2.5 - _legThick, 2.5, -2.5, -2.5 + _legThick, -2.5, _Bottom));
            _leg2.Children.Add(CreateFront(2.5 - _legThick, 2.5, -2.5, _bottomZ, -2.5, _Front));
            _leg2.Children.Add(CreateBack(2.5 - _legThick, 2.5, -2.5, _bottomZ, -2.5 + _legThick, _Standard));
            _leg2.Children.Add(CreateLeft(-2.5, -2.5 + _legThick, -2.5, _bottomZ, 2.5, _Left));
            _leg2.Children.Add(CreateRight(-2.5, -2.5 + _legThick, -2.5, _bottomZ, 2.5 - _legThick, _Standard));
            m3grpSet.Children.Add(_leg2);

            var _leg3 = new Model3DGroup();
            _leg3.Children.Add(CreateBottom(-2.5, -2.5 + _legThick, 2.5 - _legThick, 2.5, -2.5, _Bottom));
            _leg3.Children.Add(CreateFront(-2.5, -2.5 + _legThick, -2.5, _bottomZ, 2.5 - _legThick, _Standard));
            _leg3.Children.Add(CreateBack(-2.5, -2.5 + _legThick, -2.5, _bottomZ, 2.5, _Back));
            _leg3.Children.Add(CreateLeft(2.5 - _legThick, 2.5, -2.5, _bottomZ, -2.5 + _legThick, _Standard));
            _leg3.Children.Add(CreateRight(2.5 - _legThick, 2.5, -2.5, _bottomZ, -2.5, _Right));
            m3grpSet.Children.Add(_leg3);

            var _leg4 = new Model3DGroup();
            _leg4.Children.Add(CreateBottom(2.5 - _legThick, 2.5, 2.5 - _legThick, 2.5, -2.5, _Bottom));
            _leg4.Children.Add(CreateFront(2.5 - _legThick, 2.5, -2.5, _bottomZ, 2.5 - _legThick, _Standard));
            _leg4.Children.Add(CreateBack(2.5 - _legThick, 2.5, -2.5, _bottomZ, 2.5, _Back));
            _leg4.Children.Add(CreateLeft(2.5 - _legThick, 2.5, -2.5, _bottomZ, 2.5, _Left));
            _leg4.Children.Add(CreateRight(2.5 - _legThick, 2.5, -2.5, _bottomZ, 2.5 - _legThick, _Standard));
            m3grpSet.Children.Add(_leg4);
            AddKeys(@"Leg", 20);

            SaveFile($@"{PATH}\Table_025.xaml");
        }
        #endregion

        #region bars
        private void CreateBars(int barCount)
        {
            //_Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 90));

            // beams front and back
            foreach (var _z in new double[] { -2.5d, -0.4d, 0.3d, 2.4d })
            {
                var _beam = new Model3DGroup();
                _beam.Children.Add(CreateTop(-2.5, 2.5, -2.5, 2.5, _z + 0.1, _Standard));
                _beam.Children.Add(CreateBottom(-2.5, 2.5, -2.5, 2.5, _z, _Standard));
                _beam.Children.Add(CreateFront(-2.5, 2.5, _z, _z + 0.1, -2.5, _Front));
                _beam.Children.Add(CreateBack(-2.5, 2.5, _z, _z + 0.1, 2.5, _Back));
                m3grpSet.Children.Add(_beam);
            }
            AddKeys(@"Bottom", 4);
            AddKeys(@"Middle", 8);
            AddKeys(@"Top", 4);

            // side beams
            foreach (var _x in new double[] { -2.5d, 2.4d })
            {
                var _side = new Model3DGroup();
                _side.Children.Add(CreateRight(-2.5, 2.5, -2.5, 2.5, _x, _Standard));
                _side.Children.Add(CreateLeft(-2.5, 2.5, -2.5, 2.5, _x + 0.1d, _Standard));
                _side.Children.Add(CreateFront(_x, _x + 0.1, -2.5, 2.5, -2.5d, _Front));
                _side.Children.Add(CreateBack(_x, _x + 0.1, -2.5, 2.5, 2.5d, _Back));
                m3grpSet.Children.Add(_side);
            }
            AddKeys(@"Side", 8);

            // TODO: lock?

            var _barList = StepThrough(-2.5, 2.5, 5d / barCount).Skip(1).TakeWhile(_b => _b < 2.5).ToArray();
            var _half = 0.05d;
            foreach (var _x in _barList)
            {
                #region bar fronts
                {
                    var _points = new Point3DCollection
                    {
                        new Point3D(_x - _half, 0d, -2.5d),    //0
                        new Point3D(_x, -1d, -2.5d),           //1
                        new Point3D(_x + _half, 0d, -2.5d),    //2
                        new Point3D(_x - _half, 0d, 2.5d),     //3
                        new Point3D(_x, -1d, 2.5d),            //4
                        new Point3D(_x + _half, 0d, 2.5d)     //5
                    };
                    var _tri = new Int32Collection();
                    AddTriangle(_tri, 0, 1, 4);
                    AddTriangle(_tri, 0, 4, 3);
                    AddTriangle(_tri, 1, 2, 5);
                    AddTriangle(_tri, 1, 5, 4);
                    var _vect = new Vector3DCollection
                    {
                        new Vector3D(-1, 0, 0),
                        new Vector3D(1, 0, 0),
                        new Vector3D(0, -1, 0),
                        new Vector3D(-1, 0, 0),
                        new Vector3D(1, 0, 0),
                        new Vector3D(0, -1, 0)
                    };
                    m3grpSet.Children.Add(new GeometryModel3D(new MeshGeometry3D
                    {
                        Positions = Transform(_points),
                        Normals = _vect,
                        TriangleIndices = _tri
                    }, _Front));
                }
                #endregion

                #region bar backs
                {
                    var _points = new Point3DCollection
                    {
                        new Point3D(_x - _half, 0d, -2.5d),    //0
                        new Point3D(_x, 1d, -2.5d),            //1
                        new Point3D(_x + _half, 0d, -2.5d),    //2
                        new Point3D(_x - _half, 0d, 2.5d),     //3
                        new Point3D(_x, 1d, 2.5d),             //4
                        new Point3D(_x + _half, 0d, 2.5d)     //5
                    };
                    var _tri = new Int32Collection();
                    AddTriangle(_tri, 0, 4, 1);
                    AddTriangle(_tri, 0, 3, 4);
                    AddTriangle(_tri, 1, 5, 2);
                    AddTriangle(_tri, 1, 4, 5);
                    var _vect = new Vector3DCollection
                    {
                        new Vector3D(-1, 0, 0),
                        new Vector3D(1, 0, 0),
                        new Vector3D(0, -1, 0),
                        new Vector3D(-1, 0, 0),
                        new Vector3D(1, 0, 0),
                        new Vector3D(0, -1, 0)
                    };
                    m3grpSet.Children.Add(new GeometryModel3D(new MeshGeometry3D
                    {
                        Positions = Transform(_points),
                        Normals = _vect,
                        TriangleIndices = _tri
                    }, _Back));
                }
                #endregion
            }
            AddKeys(@"Bar", (barCount - 1) * 2);

            SaveFile($@"{PATH}\bars_{barCount}.xaml");
            _Transform = null;
        }
        #endregion

        #region drawers
        private void CreateDrawers(int drawerCount)
        {
            var _topBase = 2.5 - TOPTHICK;
            var _bottomCap = -2.5 + TOPTHICK;
            var _dThick = 1d / 24d;

            // left side
            var _left = new Model3DGroup();
            _left.Children.Add(CreateLeft(-2.5, 2.5, _bottomCap, _topBase, 2.5, _Left));
            _left.Children.Add(CreateRight(-2.5, 2.5, _bottomCap, _topBase, 2.5 - SIDETHICK, _Standard));
            _left.Children.Add(CreateFront(2.5 - SIDETHICK, 2.5, _bottomCap, _topBase, -2.5, _Front));
            AddKeys(@"Side", 2);
            AddKeys(@"Side.Front", 1);
            m3grpSet.Children.Add(_left);

            // right side
            var _right = new Model3DGroup();
            _right.Children.Add(CreateLeft(-2.5, 2.5, _bottomCap, _topBase, -2.5 + SIDETHICK, _Standard));
            _right.Children.Add(CreateRight(-2.5, 2.5, _bottomCap, _topBase, -2.5, _Right));
            _right.Children.Add(CreateFront(-2.5, -2.5 + SIDETHICK, _bottomCap, _topBase, -2.5, _Front));
            AddKeys(@"Side", 2);
            AddKeys(@"Side.Front", 1);
            m3grpSet.Children.Add(_right);

            // top
            var _top = new Model3DGroup();
            _top.Children.Add(CreateTop(-2.5, 2.5, -2.5, 2.5, 2.5, _Top));
            _top.Children.Add(CreateBottom(-2.5, 2.5, -2.5, 2.5, _topBase, _Standard));
            _top.Children.Add(CreateFront(-2.5, 2.5, _topBase, 2.5, -2.5, _Front));
            _top.Children.Add(CreateLeft(-2.5, 2.5, _topBase, 2.5, 2.5, _Left));
            _top.Children.Add(CreateRight(-2.5, 2.5, _topBase, 2.5, -2.5, _Right));
            AddKeys(@"Top", 2);
            AddKeys(@"Top.Front", 1);
            AddKeys(@"Top.Side", 2);
            m3grpSet.Children.Add(_top);

            // bottom
            var _bottom = new Model3DGroup();
            _bottom.Children.Add(CreateTop(-2.5, 2.5, -2.5, 2.5, _bottomCap, _Standard));
            _bottom.Children.Add(CreateBottom(-2.5, 2.5, -2.5, 2.5, -2.5, _Bottom));
            _bottom.Children.Add(CreateFront(-2.5, 2.5, -2.5, _bottomCap, -2.5, _Front));
            _bottom.Children.Add(CreateLeft(-2.5, 2.5, -2.5, _bottomCap, 2.5, _Left));
            _bottom.Children.Add(CreateRight(-2.5, 2.5, -2.5, _bottomCap, -2.5, _Right));
            AddKeys(@"Bottom", 2);
            AddKeys(@"Bottom.Front", 1);
            AddKeys(@"Bottom.Side", 2);
            m3grpSet.Children.Add(_bottom);

            // back
            var _back = new Model3DGroup();
            _back.Children.Add(CreateBack(-2.5, 2.5, -2.5, 2.5, 2.5, _Back));
            _back.Children.Add(CreateFront(-2.5, 2.5, -2.5, 2.5, 2.5 - SIDETHICK, _Standard2));
            AddKeys(@"Back", 2);
            m3grpSet.Children.Add(_back);

            // drawers
            //Model3DGroup _last = null;
            var _dHeight = (5d - (TOPTHICK * 2)) / drawerCount;
            var _dXL = -2.5 + SIDETHICK;
            var _dXH = 2.5 - SIDETHICK;
            for (var _dx = 0; _dx < drawerCount; _dx++)
            {
                var _dBottom = (-2.5 + TOPTHICK) + (_dx * _dHeight);
                var _dTop = _dBottom + _dHeight;
                var _dBack = 2.5 - SIDETHICK;

                var _drawer = new Model3DGroup();
                // bottom
                _drawer.Children.Add(CreateBottom(_dXL, _dXH, -2.5, _dBack, _dBottom, _Standard));
                _drawer.Children.Add(CreateTop(_dXL, _dXH, -2.5, _dBack, _dBottom + _dThick, _Standard2));
                AddKeys(@"Drawer.Base", 2);
                // back
                _drawer.Children.Add(CreateBack(_dXL, _dXH, _dBottom, _dTop, _dBack, _Standard));
                _drawer.Children.Add(CreateFront(_dXL, _dXH, _dBottom, _dTop, _dBack - _dThick, _Standard));
                AddKeys(@"Drawer.Back", 1);
                AddKeys(@"Drawer.Inside", 1);
                // left
                _drawer.Children.Add(CreateLeft(-2.5, _dBack, _dBottom, _dTop, _dXH, _Standard));
                _drawer.Children.Add(CreateRight(-2.5, _dBack, _dBottom, _dTop, _dXH - _dThick, _Standard));
                // right
                _drawer.Children.Add(CreateRight(-2.5, _dBack, _dBottom, _dTop, _dXL, _Standard));
                _drawer.Children.Add(CreateLeft(-2.5, _dBack, _dBottom, _dTop, _dXL + _dThick, _Standard));
                AddKeys(@"Drawer.Side", 4);
                // front
                _drawer.Children.Add(CreateFront(_dXL, _dXH, _dBottom, _dTop, -2.5, _Front));
                _drawer.Children.Add(CreateBack(_dXL, _dXH, _dBottom, _dTop, -2.5 + _dThick, _Standard));
                AddKeys(@"Drawer.Front", 1);
                AddKeys(@"Drawer.Inside", 1);

                // top of drawer panels?
                _drawer.Children.Add(CreateTop(_dXL, _dXH, _dBack - _dThick, _dBack, _dTop, _Standard3));
                _drawer.Children.Add(CreateTop(_dXL, _dXH, -2.5, -2.5 + _dThick, _dTop, _Standard3));
                _drawer.Children.Add(CreateTop(_dXL, _dXL + _dThick, -2.5, _dBack, _dTop, _Standard3));
                _drawer.Children.Add(CreateTop(_dXH - _dThick, _dXH, -2.5, _dBack, _dTop, _Standard3));
                AddKeys(@"Drawer.Top", 4);
                _drawer.Transform = new TranslateTransform3D();
                // <TranslateTransform3D OffsetY = "{uzi:DoubleProduct A=-4,B={uzi:ExternalVal Key=Open.3,Value=0}}" />

                m3grpSet.Children.Add(_drawer);
                //_last = _drawer;
            }

            SaveFile($@"{PATH}\drawers_{drawerCount}.xaml");
        }
        #endregion

        #region open top box
        private void CreateOpenTopBox()
        {
            var _backBase = 2.5 - SIDETHICK;
            var _frontBase = -2.5 + SIDETHICK;

            // left side
            var _left = new Model3DGroup();
            _left.Children.Add(CreateLeft(-2.5, 2.5, -2.5, 2.5, 2.5, _Left));
            _left.Children.Add(CreateRight(_frontBase, _backBase, -2.5, 2.5, 2.5 - SIDETHICK, _Standard));
            _left.Children.Add(CreateTop(2.5 - SIDETHICK, 2.5, _frontBase, _backBase, 2.5, _Top));
            AddKeys(@"Side", 2);
            AddKeys(@"Side.Top", 1);
            m3grpSet.Children.Add(_left);

            // right side
            var _right = new Model3DGroup();
            _right.Children.Add(CreateLeft(_frontBase, _backBase, -2.5, 2.5, -2.5 + SIDETHICK, _Standard));
            _right.Children.Add(CreateRight(-2.5, 2.5, -2.5, 2.5, -2.5, _Right));
            _right.Children.Add(CreateTop(-2.5, -2.5 + SIDETHICK, _frontBase, _backBase, 2.5, _Top));
            AddKeys(@"Side", 2);
            AddKeys(@"Side.Top", 1);
            m3grpSet.Children.Add(_right);

            // back
            var _back = new Model3DGroup();
            _back.Children.Add(CreateBack(-2.5, 2.5, -2.5, 2.5, 2.5, _Back));
            _back.Children.Add(CreateFront(-2.5, 2.5, -2.5, 2.5, _backBase, _Standard2));
            _back.Children.Add(CreateTop(-2.5, 2.5, _backBase, 2.5, 2.5, _Top));
            AddKeys(@"Back", 2);
            AddKeys(@"Back.Top", 1);
            m3grpSet.Children.Add(_back);

            // front
            var _front = new Model3DGroup();
            _front.Children.Add(CreateFront(-2.5, 2.5, -2.5, 2.5, -2.5, _Front));
            _front.Children.Add(CreateBack(-2.5, 2.5, -2.5, 2.5, _frontBase, _Standard2));
            _front.Children.Add(CreateTop(-2.5, 2.5, -2.5, _frontBase, 2.5, _Top));
            AddKeys(@"Front", 2);
            AddKeys(@"Front.Top", 1);
            m3grpSet.Children.Add(_front);

            var _bottom = new Model3DGroup();
            _bottom.Children.Add(CreateBottom(-2.5, 2.5, -2.5, 2.5, -2.5, _Bottom));
            _bottom.Children.Add(CreateTop(-2.5, 2.5, -2.5, 2.5, _frontBase, _Standard3));
            AddKeys(@"Bottom", 2);
            m3grpSet.Children.Add(_bottom);

            SaveFile($@"{PATH}\open-top-box.xaml");
        }
        #endregion

        #region cart
        private void CreateCart()
        {
            // ylow = front
            var _backBase = 2.5 - SIDETHICK;
            var _frontBase = -2.5 + SIDETHICK;
            var _pts = new Point3D[]
            {
                // zlow outer [0..3]
                new Point3D(-2, -2, -2),
                new Point3D(2, -2, -2),
                new Point3D(2, 2, -2),
                new Point3D(-2, 2, -2),

                // zlow inner [4..7]
                new Point3D(-1.5, -1.5, -1.5),
                new Point3D(1.5, -1.5, -1.5),
                new Point3D(1.5, 1.5, -1.5),
                new Point3D(-1.5, 1.5, -1.5),

                // zhigh outer [8..11]
                new Point3D(-2.5, -2.5, 2.5),
                new Point3D(2.5, -2.5, 2.5),
                new Point3D(2.5, 2.5, 2.5),
                new Point3D(-2.5, 2.5, 2.5),

                // zhigh inner [12..15]
                new Point3D(-2, -2, 2.5),
                new Point3D(2, -2, 2.5),
                new Point3D(2, 2, 2.5),
                new Point3D(-2, 2, 2.5),
            };

            // left side
            var _left = new Model3DGroup();
            _left.Children.Add(CreateQuad(_pts[1], _pts[2], _pts[10], _pts[9], _Left));
            _left.Children.Add(CreateQuad(_pts[6], _pts[5], _pts[13], _pts[14], _Standard));
            _left.Children.Add(CreateTop(2.5 - SIDETHICK, 2.5, _frontBase, _backBase, 2.5, _Top));
            AddKeys(@"Side", 2);
            AddKeys(@"Side.Top", 1);
            m3grpSet.Children.Add(_left);

            // right side
            var _right = new Model3DGroup();
            _right.Children.Add(CreateQuad(_pts[4], _pts[7], _pts[15], _pts[12], _Standard));
            _right.Children.Add(CreateQuad(_pts[3], _pts[0], _pts[8], _pts[11], _Right));
            _right.Children.Add(CreateTop(-2.5, -2.5 + SIDETHICK, _frontBase, _backBase, 2.5, _Top));
            AddKeys(@"Side", 2);
            AddKeys(@"Side.Top", 1);
            m3grpSet.Children.Add(_right);

            // back
            var _back = new Model3DGroup();
            _back.Children.Add(CreateQuad(_pts[2], _pts[3], _pts[11], _pts[10], _Back));
            _back.Children.Add(CreateQuad(_pts[7], _pts[6], _pts[14], _pts[15], _Standard2));
            _back.Children.Add(CreateTop(-2.5, 2.5, _backBase, 2.5, 2.5, _Top));
            AddKeys(@"Back", 2);
            AddKeys(@"Back.Top", 1);
            m3grpSet.Children.Add(_back);

            // front
            var _front = new Model3DGroup();
            _front.Children.Add(CreateQuad(_pts[0], _pts[1], _pts[9], _pts[8], _Front));
            _front.Children.Add(CreateQuad(_pts[5], _pts[4], _pts[12], _pts[13], _Standard2));
            _front.Children.Add(CreateTop(-2.5, 2.5, -2.5, _frontBase, 2.5, _Top));
            AddKeys(@"Front", 2);
            AddKeys(@"Front.Top", 1);
            m3grpSet.Children.Add(_front);

            var _bottom = new Model3DGroup();
            _bottom.Children.Add(CreateQuad(_pts[3], _pts[2], _pts[1], _pts[0], _Bottom));
            _bottom.Children.Add(CreateQuad(_pts[4], _pts[5], _pts[6], _pts[7], _Standard3));
            AddKeys(@"Bottom", 2);
            m3grpSet.Children.Add(_bottom);

            SaveFile($@"{PATH}\cart.xaml");
        }
        #endregion

        private void AddTriangle(Int32Collection list, int a, int b, int c)
        {
            list.Add(a);
            list.Add(b);
            list.Add(c);
        }

        private Transform3D _Transform = null;

        private Point3DCollection Transform(Point3DCollection points)
        {
            if (_Transform == null)
                return points;

            var _pts = points.ToArray();
            _Transform.Transform(_pts);
            return new Point3DCollection(_pts);
        }

        #region parts
        private IEnumerable<double> StepThrough(double min, double max, double step)
        {
            for (var _outVal = min; _outVal < max; _outVal += step)
                yield return _outVal;
            yield break;
        }

        private IEnumerable<Vector3D> Repeat(Vector3D vector, int count)
        {
            for (var _cx = 0; _cx < count; _cx++)
                yield return vector;
            yield break;
        }

        private Model3D CreateTop(double lowX, double highX, double lowY, double highY, double zValue, DiffuseMaterial material)
        {
            var _normals = new Vector3DCollection(Repeat(new Vector3D(0, 0, 1), 4));
            var _triangles = new Int32Collection();
            AddTriangle(_triangles, 0, 1, 2);
            AddTriangle(_triangles, 0, 2, 3);

            var _positions = new Point3DCollection
            {
                new Point3D(lowX, lowY, zValue),
                new Point3D(highX, lowY, zValue),
                new Point3D(highX, highY, zValue),
                new Point3D(lowX, highY, zValue)
            };
            var _tCoord = new PointCollection
            {
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0),
                new Point(0, 0)
            };
            return new GeometryModel3D(new MeshGeometry3D
            {
                Positions = Transform(_positions),
                Normals = _normals.Clone(),
                TriangleIndices = _triangles.Clone(),
                TextureCoordinates = _tCoord.Clone()
            }, material);
        }

        private Model3D CreateBottom(double lowX, double highX, double lowY, double highY, double zValue, DiffuseMaterial material)
        {
            var _normals = new Vector3DCollection(Repeat(new Vector3D(0, 0, -1), 4));
            var _triangles = new Int32Collection();
            AddTriangle(_triangles, 0, 2, 1);
            AddTriangle(_triangles, 0, 3, 2);

            var _positions = new Point3DCollection
            {
                new Point3D(lowX, lowY, zValue),
                new Point3D(highX, lowY, zValue),
                new Point3D(highX, highY, zValue),
                new Point3D(lowX, highY, zValue)
            };
            var _tCoord = new PointCollection
            {
                new Point(1, 1),
                new Point(0, 1),
                new Point(0, 0),
                new Point(1, 0)
            };
            return new GeometryModel3D(new MeshGeometry3D
            {
                Positions = Transform(_positions),
                Normals = _normals.Clone(),
                TriangleIndices = _triangles.Clone(),
                TextureCoordinates = _tCoord.Clone()
            }, material);
        }

        private Model3D CreateFront(double lowX, double highX, double lowZ, double highZ, double yValue, DiffuseMaterial material)
        {
            var _normals = new Vector3DCollection(Repeat(new Vector3D(0, -1, 0), 4));
            var _triangles = new Int32Collection();
            AddTriangle(_triangles, 0, 1, 2);
            AddTriangle(_triangles, 0, 2, 3);

            var _positions = new Point3DCollection
            {
                new Point3D(lowX, yValue, lowZ),
                new Point3D(highX, yValue, lowZ),
                new Point3D(highX, yValue, highZ),
                new Point3D(lowX, yValue, highZ)
            };
            var _tCoord = new PointCollection
            {
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0),
                new Point(0, 0)
            };
            return new GeometryModel3D(new MeshGeometry3D
            {
                Positions = Transform(_positions),
                Normals = _normals.Clone(),
                TriangleIndices = _triangles.Clone(),
                TextureCoordinates = _tCoord.Clone()
            }, material);
        }

        private Model3D CreateBack(double lowX, double highX, double lowZ, double highZ, double yValue, DiffuseMaterial material)
        {
            var _normals = new Vector3DCollection(Repeat(new Vector3D(0, 1, 0), 4));
            var _triangles = new Int32Collection();
            AddTriangle(_triangles, 0, 2, 1);
            AddTriangle(_triangles, 0, 3, 2);

            var _positions = new Point3DCollection
            {
                new Point3D(lowX, yValue, lowZ),
                new Point3D(highX, yValue, lowZ),
                new Point3D(highX, yValue, highZ),
                new Point3D(lowX, yValue, highZ)
            };
            var _tCoord = new PointCollection
            {
                new Point(1, 1),
                new Point(0, 1),
                new Point(0, 0),
                new Point(1, 0)
            };
            return new GeometryModel3D(new MeshGeometry3D
            {
                Positions = Transform(_positions),
                Normals = _normals.Clone(),
                TriangleIndices = _triangles.Clone(),
                TextureCoordinates = _tCoord.Clone()
            }, material);
        }

        private Model3D CreateLeft(double lowY, double highY, double lowZ, double highZ, double xValue, DiffuseMaterial material)
        {
            var _normals = new Vector3DCollection(Repeat(new Vector3D(1, 0, 0), 4));
            var _triangles = new Int32Collection();
            AddTriangle(_triangles, 0, 1, 2);
            AddTriangle(_triangles, 0, 2, 3);

            var _positions = new Point3DCollection
            {
                new Point3D(xValue, lowY, lowZ),
                new Point3D(xValue, highY, lowZ),
                new Point3D(xValue, highY, highZ),
                new Point3D(xValue, lowY, highZ)
            };
            var _tCoord = new PointCollection
            {
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0),
                new Point(0, 0)
            };
            return new GeometryModel3D(new MeshGeometry3D
            {
                Positions = Transform(_positions),
                Normals = _normals.Clone(),
                TriangleIndices = _triangles.Clone(),
                TextureCoordinates = _tCoord.Clone()
            }, material);
        }

        private Model3D CreateRight(double lowY, double highY, double lowZ, double highZ, double xValue, DiffuseMaterial material)
        {
            var _normals = new Vector3DCollection(Repeat(new Vector3D(-1, 0, 0), 4));
            var _triangles = new Int32Collection();
            AddTriangle(_triangles, 0, 2, 1);
            AddTriangle(_triangles, 0, 3, 2);

            var _positions = new Point3DCollection
            {
                new Point3D(xValue, lowY, lowZ),
                new Point3D(xValue, highY, lowZ),
                new Point3D(xValue, highY, highZ),
                new Point3D(xValue, lowY, highZ)
            };
            var _tCoord = new PointCollection
            {
                new Point(1, 1),
                new Point(0, 1),
                new Point(0, 0),
                new Point(1, 0)
            };
            return new GeometryModel3D(new MeshGeometry3D
            {
                Positions = Transform(_positions),
                Normals = _normals.Clone(),
                TriangleIndices = _triangles.Clone(),
                TextureCoordinates = _tCoord.Clone()
            }, material);
        }

        private Model3D CreateQuad(Point3D pt1, Point3D pt2, Point3D pt3, Point3D pt4, DiffuseMaterial material)
        {
            var _normals = new Vector3DCollection(Repeat(new Vector3D(1, 0, 0), 4));
            var _triangles = new Int32Collection();
            AddTriangle(_triangles, 0, 1, 2);
            AddTriangle(_triangles, 0, 2, 3);

            var _positions = new Point3DCollection
            {
                pt1,
                pt2,
                pt3,
                pt4
            };
            var _tCoord = new PointCollection
            {
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0),
                new Point(0, 0)
            };
            return new GeometryModel3D(new MeshGeometry3D
            {
                Positions = Transform(_positions),
                Normals = _normals.Clone(),
                TriangleIndices = _triangles.Clone(),
                TextureCoordinates = _tCoord.Clone()
            }, material);
        }
        #endregion
    }
}
