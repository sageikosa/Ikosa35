using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Uzi.Visualize;
using System.ComponentModel;

namespace CharacterModeler
{
    public class HeadModel
    {
        public HeadModel()
        {
            // TODO: ranges
            PointOffset = 0.0d;
            CapHeight = 0.05d;
            CapFrontWidth = 0.4d;
            CapWidth = 0.6d;
            CapBackWidth = 0.4d;
            CapLength = 0.6928d;
            CapOffset = 0d;
            CapCenterOffset = 0d;

            TonsureHeight = 0.17d;
            TonsureFrontWidth = 0.8d;
            TonsureWidth = 1.2d;
            TonsureBackWidth = 0.8d;

            TonsureLength = 1.3856d;
            TonsureOffset = 0d;
            TonsureCenterOffset = 0d;

            NoseOffset = 0.0536d;
            HeadBackOffset = -0.0536d;

            FaceHeight = 0.63d;
            HeadBackHeight = 0.43d;
            ChinWidth = 0.72d;
            BaseWidth = 0.8d;
            HeadBackWidth = 0.6d;

            BaseLength = 1.24d;
            BaseOffset = 0.04d;
            BaseCenterOffset = -0.24d;
            BaseCenterPointOffset = -0.15d;

            HelmetHeight = 0.48d;
            HelmetBrowHeight = 0.08d;
            HelmetBrowWidth = 0.9d;
            HelmetWidth = 1.2d;
            HelmetBackWidth = 0.9d;

            HelmetLength = 1.5d;
            HelmetOffset = 0d;
            HelmetCenterOffset = -0.125d;

            HairLength = 0.6d;
            HairSpread = 0.15d;
            HairFullness = 0.16d;
            HairSway = 0d;

            HairCenterLength = 0.15d;
            HairCenterFullness = 0.05d;
            HairCenterSway = 0d;

            HairSideLength = 0.6d;
            HairSideFullness = 0.125d;
            HairSideSway = 0d;
            HairSideSweep = 0d;
        }

        // material resolver to use
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IResolveMaterial MaterialResolver { get; set; }

        // cap
        public double PointOffset { get; set; }
        public double CapHeight { get; set; }

        public double CapFrontWidth { get; set; }
        public double CapWidth { get; set; }
        public double CapBackWidth { get; set; }

        public double CapLength { get; set; }
        public double CapOffset { get; set; }
        public double CapCenterOffset { get; set; }

        public bool IsCapSegmented { get; set; }
        public bool IsCapSectored { get; set; }

        // tonsure
        public double TonsureHeight { get; set; }
        public double TonsureFrontWidth { get; set; }
        public double TonsureWidth { get; set; }
        public double TonsureBackWidth { get; set; }

        public double TonsureLength { get; set; }
        public double TonsureOffset { get; set; }
        public double TonsureCenterOffset { get; set; }

        public bool IsTonsureSegmented { get; set; }
        public bool IsTonsureSectored { get; set; }
        public bool IsTonsureTwistedLeft { get; set; }

        // main face and sides
        public double NoseOffset { get; set; }
        public double HeadBackOffset { get; set; }

        public double FaceHeight { get; set; }
        public double HeadBackHeight { get; set; }
        public double ChinWidth { get; set; }
        public double BaseWidth { get; set; }
        public double HeadBackWidth { get; set; }

        public bool IsHeadSectored { get; set; }

        // very bottom
        public double BaseLength { get; set; }
        public double BaseOffset { get; set; }
        public double BaseCenterOffset { get; set; }
        public double BaseCenterPointOffset { get; set; }

        // helmet
        public bool HasHelmet { get; set; }

        public double HelmetHeight { get; set; }
        public double HelmetBrowHeight { get; set; }
        public double HelmetBrowWidth { get; set; }
        public double HelmetWidth { get; set; }
        public double HelmetBackWidth { get; set; }

        public double HelmetLength { get; set; }
        public double HelmetOffset { get; set; }
        public double HelmetCenterOffset { get; set; }

        public bool IsHelmetSegmented { get; set; }
        public bool IsHelmetSectored { get; set; }

        // hair
        public bool HasHair { get; set; }

        // back side points
        public double HairLength { get; set; }
        public double HairSpread { get; set; }
        public double HairFullness { get; set; }
        public double HairSway { get; set; }

        // center of back point
        public double HairCenterLength { get; set; }
        public double HairCenterFullness { get; set; }
        public double HairCenterSway { get; set; }

        public double HairSideLength { get; set; }
        public double HairSideFullness { get; set; }
        public double HairSideSway { get; set; }
        public double HairSideSweep { get; set; }

        // TODO: mount points: horns, braids, tassles

        private static IEnumerable<Any> GetEnumerable<Any>(params Any[] any)
        {
            return any;
        }

        private Point3D MidPoint(Point3D pt1, Point3D pt2)
        {
            return (pt1 + ((pt2 - pt1) / 2));
        }

        #region public Point3D[] GetPoints()
        public Point3D[] GetPoints()
        {
            var _points = new Point3D[35];

            // cap point
            _points[0] = new Point3D(0d, CapOffset + PointOffset, FaceHeight + TonsureHeight + CapHeight);

            // cap hexagon
            _points[1] = new Point3D(CapFrontWidth / +2d, CapOffset + CapLength / 2d, FaceHeight + TonsureHeight);
            _points[2] = new Point3D(CapFrontWidth / -2d, CapOffset + CapLength / 2d, FaceHeight + TonsureHeight);
            _points[3] = new Point3D(CapWidth / -2d, CapOffset + CapCenterOffset, FaceHeight + TonsureHeight);
            _points[4] = new Point3D(CapBackWidth / -2d, CapOffset - CapLength / 2d, FaceHeight + TonsureHeight);
            _points[5] = new Point3D(CapBackWidth / +2d, CapOffset - CapLength / 2d, FaceHeight + TonsureHeight);
            _points[6] = new Point3D(CapWidth / +2d, CapOffset + CapCenterOffset, FaceHeight + TonsureHeight);

            // tonsure hexagon
            _points[7] = new Point3D(TonsureFrontWidth / +2d, TonsureOffset + TonsureLength / 2d, FaceHeight);
            _points[8] = new Point3D(TonsureFrontWidth / -2d, TonsureOffset + TonsureLength / 2d, FaceHeight);
            _points[9] = new Point3D(TonsureWidth / -2d, TonsureOffset + TonsureCenterOffset, FaceHeight);
            _points[10] = new Point3D(TonsureBackWidth / -2d, TonsureOffset - TonsureLength / 2d, FaceHeight);
            _points[11] = new Point3D(TonsureBackWidth / +2d, TonsureOffset - TonsureLength / 2d, FaceHeight);
            _points[12] = new Point3D(TonsureWidth / +2d, TonsureOffset + TonsureCenterOffset, FaceHeight);

            // base hexagon
            _points[13] = new Point3D(ChinWidth / +2d, BaseOffset + BaseLength / 2d, 0d);
            _points[14] = new Point3D(ChinWidth / -2d, BaseOffset + BaseLength / 2d, 0d);
            _points[15] = new Point3D(BaseWidth / -2d, BaseOffset + BaseCenterOffset, 0d);
            _points[16] = new Point3D(HeadBackWidth / -2d, BaseOffset - BaseLength / 2d, FaceHeight - HeadBackHeight);
            _points[17] = new Point3D(HeadBackWidth / +2d, BaseOffset - BaseLength / 2d, FaceHeight - HeadBackHeight);
            _points[18] = new Point3D(BaseWidth / +2d, BaseOffset + BaseCenterOffset, 0d);

            // other base points (base, back o'head, nose)
            _points[19] = new Point3D(0d, BaseOffset + BaseCenterOffset + BaseCenterPointOffset, 0d);
            _points[20] = new Point3D(0d,
                (((TonsureOffset - TonsureLength / 2d) + (BaseOffset - BaseLength / 2d)) / 2d) + HeadBackOffset,
                FaceHeight - (HeadBackHeight / 2));
            _points[21] = new Point3D(0d,
                (((TonsureOffset + TonsureLength / 2d) + (BaseOffset + BaseLength / 2d)) / 2d) + NoseOffset,
                FaceHeight / 2d);

            // helmet hexagon
            _points[22] = new Point3D(HelmetBrowWidth / +2d, HelmetOffset + HelmetLength / 2d, FaceHeight - HelmetBrowHeight);
            _points[23] = new Point3D(HelmetBrowWidth / -2d, HelmetOffset + HelmetLength / 2d, FaceHeight - HelmetBrowHeight);
            _points[24] = new Point3D(HelmetWidth / -2d, HelmetOffset + HelmetCenterOffset, FaceHeight - HelmetHeight);
            _points[25] = new Point3D(HelmetBackWidth / -2d, HelmetOffset - HelmetLength / 2d, FaceHeight - HelmetHeight);
            _points[26] = new Point3D(HelmetBackWidth / +2d, HelmetOffset - HelmetLength / 2d, FaceHeight - HelmetHeight);
            _points[27] = new Point3D(HelmetWidth / +2d, HelmetOffset + HelmetCenterOffset, FaceHeight - HelmetHeight);

            // offset points
            _points[28] = _points[9] + (new Vector3D(0 - HairSideFullness + HairSideSway, HairSideSweep, 0 - HairSideLength));
            _points[34] = _points[12] + (new Vector3D(HairSideFullness + HairSideSway, HairSideSweep, 0 - HairSideLength));
            _points[30] = _points[10] + (new Vector3D((0 - HairSpread) + HairSway, 0 - HairFullness, 0 - HairLength));
            _points[32] = _points[11] + (new Vector3D(HairSpread + HairSway, 0 - HairFullness, 0 - HairLength));

            // interpolated points
            _points[29] = MidPoint(_points[28], _points[30]);
            _points[31] = MidPoint(_points[30], _points[32]) + (new Vector3D(HairCenterSway, 0 - HairCenterFullness, 0 - HairCenterLength));
            _points[33] = MidPoint(_points[32], _points[34]);

            return _points;
        }
        #endregion

        public Model3D RenderModel
        {
            get { return GetModel(GetRenderPalette()); }
        }

        #region public Model3D GetModel(Dictionary<string, Material> palette)
        public Model3D GetModel(Dictionary<string, Material> palette)
        {
            var _model = new Model3DGroup();
            var _pts = GetPoints();

            #region Cap
            if (IsCapSectored)
            {
                Func<int, int, int, Material, GeometryModel3D> _getSector = (a, b, c, material) =>
                {
                    return new GeometryModel3D(new MeshGeometry3D
                    {
                        Positions = new Point3DCollection(GetEnumerable(_pts[a], _pts[b], _pts[c])),
                        TriangleIndices = new Int32Collection(GetEnumerable(0, 1, 2)),
                        TextureCoordinates = new PointCollection(GetEnumerable(new Point(0, 1), new Point(1, 1), new Point(0.5, 0)))
                    }, material);
                };

                _model.Children.Add(_getSector(1, 2, 0, palette[@"CapFront"]));
                _model.Children.Add(_getSector(2, 3, 0, palette[@"CapFrontLeft"]));
                _model.Children.Add(_getSector(3, 4, 0, palette[@"CapBackLeft"]));
                _model.Children.Add(_getSector(4, 5, 0, palette[@"CapBack"]));
                _model.Children.Add(_getSector(5, 6, 0, palette[@"CapBackRight"]));
                _model.Children.Add(_getSector(6, 1, 0, palette[@"CapFrontRight"]));
            }
            else if (IsCapSegmented)
            {
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[1], _pts[2], _pts[0],
                        _pts[2], _pts[3], _pts[0],
                        _pts[3], _pts[4], _pts[0],
                        _pts[4], _pts[5], _pts[0],
                        _pts[5], _pts[6], _pts[0],
                        _pts[6], _pts[1], _pts[0])),
                    TriangleIndices = new Int32Collection(GetEnumerable(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 1), new Point(1, 1), new Point(0.5, 0),
                        new Point(0, 1), new Point(1, 1), new Point(0.5, 0),
                        new Point(0, 1), new Point(1, 1), new Point(0.5, 0),
                        new Point(0, 1), new Point(1, 1), new Point(0.5, 0),
                        new Point(0, 1), new Point(1, 1), new Point(0.5, 0),
                        new Point(0, 1), new Point(1, 1), new Point(0.5, 0)
                        ))
                }, palette[@"Cap"]));
            }
            else // banded cap
            {
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(_pts[1], _pts[2], _pts[3], _pts[4], _pts[5], _pts[6], _pts[1], _pts[0])),
                    TriangleIndices = new Int32Collection(GetEnumerable(0, 1, 7, 1, 2, 7, 2, 3, 7, 3, 4, 7, 4, 5, 7, 5, 6, 7)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 1), new Point(0.1667, 1), new Point(0.3333, 1), new Point(0.5, 1),
                        new Point(0.6667, 1), new Point(0.8333, 1), new Point(1, 1), new Point(0.5, 0)
                        ))
                }, palette[@"Cap"]));
            }
            #endregion

            #region Tonsure
            if (IsTonsureSectored)
            {
                #region sectored tonsure
                // sectored tonsure
                Func<int, int, int, int, Material, GeometryModel3D> _getSector = (a, b, c, d, material) =>
                {
                    return new GeometryModel3D(new MeshGeometry3D
                    {
                        Positions = new Point3DCollection(GetEnumerable(_pts[a], _pts[b], _pts[c], _pts[d])),
                        TriangleIndices = IsTonsureTwistedLeft
                        ? new Int32Collection(GetEnumerable(0, 1, 2, 1, 3, 2))
                        : new Int32Collection(GetEnumerable(0, 1, 3, 0, 3, 2)),
                        TextureCoordinates = new PointCollection(GetEnumerable(new Point(0, 1), new Point(1, 1), new Point(0, 0), new Point(1, 0)))
                    }, material);
                };

                _model.Children.Add(_getSector(7, 8, 1, 2, palette[@"TonsureFront"]));
                _model.Children.Add(_getSector(8, 9, 2, 3, palette[@"TonsureFrontLeft"]));
                _model.Children.Add(_getSector(9, 10, 3, 4, palette[@"TonsureBackLeft"]));
                _model.Children.Add(_getSector(10, 11, 4, 5, palette[@"TonsureBack"]));
                _model.Children.Add(_getSector(11, 12, 5, 6, palette[@"TonsureBackRight"]));
                _model.Children.Add(_getSector(12, 7, 6, 1, palette[@"TonsureFrontRight"]));
                #endregion
            }
            else if (IsTonsureSegmented)
            {
                #region segmented tonsure
                // tonsure front
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(_pts[7], _pts[8], _pts[1], _pts[2])),
                    TriangleIndices = IsTonsureTwistedLeft
                    ? new Int32Collection(GetEnumerable(0, 1, 2, 1, 3, 2))
                    : new Int32Collection(GetEnumerable(0, 1, 3, 0, 3, 2)),
                    TextureCoordinates = new PointCollection(GetEnumerable(new Point(0, 1), new Point(1, 1), new Point(0, 0), new Point(1, 0)))
                }, palette[@"TonsureFront"]));

                // tonsure sides
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[8], _pts[9], _pts[2], _pts[3], _pts[9], _pts[10], _pts[3], _pts[4],
                        _pts[11], _pts[12], _pts[5], _pts[6], _pts[12], _pts[7], _pts[6], _pts[1]
                        )),
                    TriangleIndices = IsTonsureTwistedLeft
                    ? new Int32Collection(GetEnumerable(0, 1, 2, 1, 3, 2, 4, 5, 6, 5, 7, 6, 8, 9, 10, 9, 11, 10, 12, 13, 14, 13, 15, 14))
                    : new Int32Collection(GetEnumerable(0, 1, 3, 0, 3, 2, 4, 5, 7, 4, 7, 6, 8, 9, 11, 8, 11, 10, 12, 13, 15, 12, 15, 14)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 1), new Point(1, 1), new Point(0, 0), new Point(1, 0),
                        new Point(0, 1), new Point(1, 1), new Point(0, 0), new Point(1, 0),
                        new Point(0, 1), new Point(1, 1), new Point(0, 0), new Point(1, 0),
                        new Point(0, 1), new Point(1, 1), new Point(0, 0), new Point(1, 0)
                        ))
                }, palette[@"TonsureSides"]));

                // tonsure back
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(_pts[10], _pts[11], _pts[4], _pts[5])),
                    TriangleIndices = IsTonsureTwistedLeft
                    ? new Int32Collection(GetEnumerable(0, 1, 2, 1, 3, 2))
                    : new Int32Collection(GetEnumerable(0, 1, 3, 0, 3, 2)),
                    TextureCoordinates = new PointCollection(GetEnumerable(new Point(0, 1), new Point(1, 1), new Point(0, 0), new Point(1, 0)))
                }, palette[@"TonsureBack"]));
                #endregion
            }
            else // banded
            {
                #region banded tonsure
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[7], _pts[8], _pts[9], _pts[10], _pts[11], _pts[12], _pts[7],
                        _pts[1], _pts[2], _pts[3], _pts[4], _pts[5], _pts[6], _pts[1]
                        )),
                    TriangleIndices = IsTonsureTwistedLeft
                    ? new Int32Collection(GetEnumerable(0, 1, 7, 1, 8, 7, 1, 2, 8, 2, 9, 8, 2, 3, 9, 3, 10, 9, 3, 4, 10, 4, 11, 10, 4, 5, 11, 5, 12, 11, 5, 6, 12, 6, 13, 12))
                    : new Int32Collection(GetEnumerable(0, 1, 8, 0, 8, 7, 1, 2, 9, 1, 9, 8, 2, 3, 10, 2, 10, 9, 3, 4, 11, 3, 11, 10, 4, 5, 12, 4, 12, 11, 5, 6, 13, 5, 13, 12)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 1), new Point(0.1667, 1), new Point(0.3333, 1), new Point(0.5, 1), new Point(0.6667, 1), new Point(0.8333, 1), new Point(1, 1),
                        new Point(0, 0), new Point(0.1667, 0), new Point(0.3333, 0), new Point(0.5, 0), new Point(0.6667, 0), new Point(0.8333, 0), new Point(1, 0)
                        ))
                }, palette[@"Tonsure"]));
                #endregion
            }
            #endregion

            #region face
            // face
            _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
            {
                Positions = new Point3DCollection(GetEnumerable(
                    _pts[13], _pts[14], _pts[8], _pts[7], _pts[21])),
                TriangleIndices = new Int32Collection(GetEnumerable(
                    0, 1, 4, 1, 2, 4, 2, 3, 4, 3, 0, 4)),
                TextureCoordinates = new PointCollection(GetEnumerable(
                    new Point(0, 1), new Point(1, 1), new Point(1, 0), new Point(0, 0), new Point(0.5, 0.5)))
            }, palette[@"Face"]));
            #endregion

            if (IsHeadSectored)
            {
                #region sectored head
                // chin
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[13], _pts[14], _pts[15], _pts[18], _pts[19])),
                    TriangleIndices = new Int32Collection(GetEnumerable(
                        0, 4, 1, 1, 4, 2, 3, 4, 0)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0, 1), new Point(0.5, 1)))
                }, palette[@"Chin"]));

                // skullbase
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[15], _pts[16], _pts[17], _pts[18], _pts[19])),
                    TriangleIndices = new Int32Collection(GetEnumerable(
                        0, 4, 1, 1, 4, 2, 2, 4, 3)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 1), new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0.5, 1)))
                }, palette[@"SkullBase"]));

                // front left
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[14], _pts[15], _pts[8], _pts[9])),
                    TriangleIndices = new Int32Collection(GetEnumerable(
                        0, 1, 3, 0, 3, 2)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 1), new Point(1, 1), new Point(0, 0), new Point(1, 0)))
                }, palette[@"HeadFrontLeft"]));

                // back left
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[15], _pts[16], _pts[9], _pts[10])),
                    TriangleIndices = new Int32Collection(GetEnumerable(0, 1, 3, 0, 3, 2 )),//0, 1, 2, 1, 3, 2
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 1), new Point(1, 1), new Point(0, 0), new Point(1, 0)))
                }, palette[@"HeadBackLeft"]));

                // back
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[16], _pts[17], _pts[10], _pts[11], _pts[20])),
                    TriangleIndices = new Int32Collection(GetEnumerable(
                        0, 1, 4, 1, 3, 4, 3, 2, 4, 2, 0, 4)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 1), new Point(1, 1), new Point(0, 0), new Point(1, 0), new Point(0.5, 0.5)))
                }, palette[@"HeadBack"]));

                // back right
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[17], _pts[18], _pts[11], _pts[12])),
                    TriangleIndices =  new Int32Collection(GetEnumerable(0, 1, 2, 1, 3, 2)),
                    //: new Int32Collection(GetEnumerable(0, 1, 3, 0, 3, 2)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 1), new Point(1, 1), new Point(0, 0), new Point(1, 0)))
                }, palette[@"HeadBackRight"]));

                // front right
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[18], _pts[13], _pts[12], _pts[7])),
                    TriangleIndices = new Int32Collection(GetEnumerable(
                        0, 1, 2, 1, 3, 2)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 1), new Point(1, 1), new Point(0, 0), new Point(1, 0)))
                }, palette[@"HeadFrontRight"]));
                #endregion
            }
            else // "banded"
            {
                #region banded head
                // chin and skullbase
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[13], _pts[14], _pts[15], _pts[16], _pts[17], _pts[18], _pts[19])),
                    TriangleIndices = new Int32Collection(GetEnumerable(
                        0, 6, 1, 1, 6, 2, 2, 6, 3, 3, 6, 4, 4, 6, 5, 5, 6, 0)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 0), new Point(1, 0), new Point(1, 0.5), new Point(1, 1), new Point(0, 1), new Point(0, 0.5), new Point(0.5, 0.5)))
                }, palette[@"HeadBase"]));

                // head band
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[14], _pts[15], _pts[16], _pts[17], _pts[18], _pts[13], _pts[8], _pts[9], _pts[10], _pts[11], _pts[12], _pts[7], _pts[20])),
                    TriangleIndices = new Int32Collection(GetEnumerable(
                        0, 1, 7, 0, 7, 6, 1, 2, 8, 1, 8, 7, 3, 4, 9, 4, 10, 9, 4, 5, 10, 5, 11, 10, 2, 12, 8, 3, 12, 2, 9, 12, 3, 8, 12, 9)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 1), new Point(0.2, 1), new Point(0.4, 1), new Point(0.6, 1), new Point(0.8, 1), new Point(1, 1),
                        new Point(0, 0), new Point(0.2, 0), new Point(0.4, 0), new Point(0.6, 0), new Point(0.8, 0), new Point(1, 0), new Point(0.5, 0.5)))
                }, palette[@"HeadBand"]));
                #endregion
            }

            if (HasHelmet)
            {
                #region banded helmet
                // brow
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[22], _pts[23], _pts[7], _pts[8])),
                    TriangleIndices = new Int32Collection(GetEnumerable(
                        0, 1, 2, 1, 3, 2)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 1), new Point(1, 1), new Point(0, 0), new Point(1, 0)))
                }, palette[@"HelmetBrow"]) { BackMaterial = palette[@"HelmetInnerBrow"] });

                // band
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[23], _pts[24], _pts[25], _pts[26], _pts[27], _pts[22],
                        _pts[8], _pts[9], _pts[10], _pts[11], _pts[12], _pts[7])),
                    TriangleIndices = new Int32Collection(GetEnumerable(
                        0, 1, 6, 1, 7, 6, 1, 2, 7, 2, 8, 7, 2, 3, 8, 3, 9, 8,  3, 4, 10,  3, 10, 9,  4, 11, 10,  4, 5, 11)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 1), new Point(0.2, 1), new Point(0.4, 1), new Point(0.6, 1), new Point(0.8, 1), new Point(1, 1),
                        new Point(0, 0), new Point(0.2, 0), new Point(0.4, 0), new Point(0.6, 0), new Point(0.8, 0), new Point(1, 0)))
                }, palette[@"HelmetBand"]) { BackMaterial = palette[@"HelmetInnerBand"] });
                #endregion
            }

            if (HasHair)
            {
                #region Hair
                // outer hair shell parts
                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[8], _pts[9], _pts[28], // 0 1 2
                        _pts[9], _pts[10], _pts[28], _pts[29], _pts[30], // 3 4 5 6 7
                        _pts[10], _pts[11], _pts[30], _pts[31], _pts[32],// 8 9  10 11 12
                        _pts[11], _pts[12], _pts[32], _pts[33], _pts[34],// 13 14  15 16 17
                        _pts[12], _pts[7], _pts[34])), // 18 19 20
                    TriangleIndices = new Int32Collection(GetEnumerable(
                        0, 2, 1, 3, 5, 6, 3, 6, 4, 4, 6, 7, 8, 10, 11, 8, 11, 9, 9, 11, 12, 13, 15, 16, 13, 16, 14, 14, 16, 17, 18, 20, 19)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 0), new Point(1, 0), new Point(1, 1),
                        new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(0.5, 1), new Point(1, 1),
                        new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(0.5, 1), new Point(1, 1),
                        new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(0.5, 1), new Point(1, 1),
                        new Point(0, 0), new Point(1, 0), new Point(0, 1)
                        ))
                }, palette[@"Hair"]));

                _model.Children.Add(new GeometryModel3D(new MeshGeometry3D
                {
                    Positions = new Point3DCollection(GetEnumerable(
                        _pts[7], _pts[34], _pts[18], 
                        _pts[8], _pts[15], _pts[28], 
                        _pts[15], _pts[30], _pts[28],  
                        _pts[18], _pts[34], _pts[32],
                        _pts[15], _pts[31], _pts[30],
                        _pts[18], _pts[32], _pts[31],
                        _pts[15], _pts[18], _pts[31]
                        )),
                    TriangleIndices = new Int32Collection(GetEnumerable(
                        0, 1, 2, 3, 4, 5, 6,7,8, 9,10,11, 12,13,14, 15,16,17, 18,19,20)),
                    TextureCoordinates = new PointCollection(GetEnumerable(
                        new Point(0, 0), new Point(0, 1), new Point(1, 1), 
                        new Point(1, 0), new Point(1, 1), new Point(0, 1),
                        new Point(1,1), new Point(0,1), new Point(0,0),
                        new Point(1,0), new Point(0,0), new Point(0,1),
                        new Point(1,0), new Point(1,1), new Point(0,1),
                        new Point(1,0), new Point(0,0), new Point(1,1),
                        new Point(1,0), new Point(0,0), new Point(0.5,1)
                        ))
                }, palette[@"Hair"]));
                #endregion
            }
            // TODO: TODO: hair...

            return _model;
        }
        #endregion

        #region private Dictionary<string, Material> GetRenderPalette()
        private Dictionary<string, Material> GetRenderPalette()
        {
            Func<string, Material> _getMaterial = (key) =>
                {
                    if (MaterialResolver != null)
                    {
                        var _material = MaterialResolver.GetMaterial(key, VisualEffect.Normal);
                        if (_material != null)
                            return _material;
                    }
                    return BrushDefinition.MissingMaterial;
                };

            var _palette = new Dictionary<string, Material>();
            _palette.Add(@"Cap",           _getMaterial(@"Cap"));
            _palette.Add(@"CapFront",      _getMaterial(@"CapFront"));
            _palette.Add(@"CapFrontLeft",  _getMaterial(@"CapFrontLeft"));
            _palette.Add(@"CapBackLeft",   _getMaterial(@"CapBackLeft"));
            _palette.Add(@"CapBack",       _getMaterial(@"CapBack"));
            _palette.Add(@"CapBackRight",  _getMaterial(@"CapBackRight"));
            _palette.Add(@"CapFrontRight", _getMaterial(@"CapFrontRight"));

            _palette.Add(@"Tonsure",           _getMaterial(@"Tonsure"));
            _palette.Add(@"TonsureFront",      _getMaterial(@"TonsureFront"));
            _palette.Add(@"TonsureFrontLeft",  _getMaterial(@"TonsureFrontLeft"));
            _palette.Add(@"TonsureBackLeft",   _getMaterial(@"TonsureBackLeft"));
            _palette.Add(@"TonsureBack",       _getMaterial(@"TonsureBack"));
            _palette.Add(@"TonsureBackRight",  _getMaterial(@"TonsureBackRight"));
            _palette.Add(@"TonsureFrontRight", _getMaterial(@"TonsureFrontRight"));
            _palette.Add(@"TonsureSides",      _getMaterial(@"TonsureSides"));

            // sectored head
            _palette.Add(@"Face",           _getMaterial(@"Face"));
            _palette.Add(@"Chin",           _getMaterial(@"Chin"));
            _palette.Add(@"SkullBase",      _getMaterial(@"SkullBase"));
            _palette.Add(@"HeadFrontLeft",  _getMaterial(@"HeadFrontLeft"));
            _palette.Add(@"HeadBackLeft",   _getMaterial(@"HeadBackLeft"));
            _palette.Add(@"HeadBack",       _getMaterial(@"HeadBack"));
            _palette.Add(@"HeadBackRight",  _getMaterial(@"HeadBackRight"));
            _palette.Add(@"HeadFrontRight", _getMaterial(@"HeadFrontRight"));

            // banded head
            _palette.Add(@"HeadBase", _getMaterial(@"HeadBase"));
            _palette.Add(@"HeadBand", _getMaterial(@"HeadBand"));

            // banded helmet
            _palette.Add(@"HelmetBrow",      _getMaterial(@"HelmetBrow"));
            _palette.Add(@"HelmetInnerBrow", _getMaterial(@"HelmetInnerBrow"));
            _palette.Add(@"HelmetBand",      _getMaterial(@"HelmetBand"));
            _palette.Add(@"HelmetInnerBand", _getMaterial(@"HelmetInnerBand"));

            // hair
            _palette.Add(@"Hair", _getMaterial(@"Hair"));
            return _palette;
        }
        #endregion

        #region private Dictionary<string, Material> GetSerializationPalette()
        private Dictionary<string, Material> GetSerializationPalette()
        {
            Func<Color, Material> _getMaterial = (color) => new DiffuseMaterial(new SolidColorBrush(color));

            var _palette = new Dictionary<string, Material>();

            _palette.Add(@"Cap",           _getMaterial(Color.FromArgb(255, 1, 0, 0)));
            _palette.Add(@"CapFront",      _getMaterial(Color.FromArgb(255, 1, 1, 0)));
            _palette.Add(@"CapFrontLeft",  _getMaterial(Color.FromArgb(255, 1, 1, 1)));
            _palette.Add(@"CapBackLeft",   _getMaterial(Color.FromArgb(255, 1, 2, 1)));
            _palette.Add(@"CapBack",       _getMaterial(Color.FromArgb(255, 1, 2, 0)));
            _palette.Add(@"CapBackRight",  _getMaterial(Color.FromArgb(255, 1, 2, 2)));
            _palette.Add(@"CapFrontRight", _getMaterial(Color.FromArgb(255, 1, 1, 2)));

            _palette.Add(@"Tonsure",           _getMaterial(Color.FromArgb(255, 2, 0, 0)));
            _palette.Add(@"TonsureFront",      _getMaterial(Color.FromArgb(255, 2, 1, 0)));
            _palette.Add(@"TonsureFrontLeft",  _getMaterial(Color.FromArgb(255, 2, 1, 1)));
            _palette.Add(@"TonsureBackLeft",   _getMaterial(Color.FromArgb(255, 2, 2, 1)));
            _palette.Add(@"TonsureBack",       _getMaterial(Color.FromArgb(255, 2, 2, 0)));
            _palette.Add(@"TonsureBackRight",  _getMaterial(Color.FromArgb(255, 2, 2, 2)));
            _palette.Add(@"TonsureFrontRight", _getMaterial(Color.FromArgb(255, 2, 1, 2)));
            _palette.Add(@"TonsureSides",      _getMaterial(Color.FromArgb(255, 2, 3, 3)));

            // sectored head
            _palette.Add(@"Face",           _getMaterial(Color.FromArgb(255, 3, 1, 0)));
            _palette.Add(@"Chin",           _getMaterial(Color.FromArgb(255, 4, 1, 0)));
            _palette.Add(@"SkullBase",      _getMaterial(Color.FromArgb(255, 4, 2, 0)));
            _palette.Add(@"HeadFrontLeft",  _getMaterial(Color.FromArgb(255, 3, 1, 1)));
            _palette.Add(@"HeadBackLeft",   _getMaterial(Color.FromArgb(255, 3, 2, 1)));
            _palette.Add(@"HeadBack",       _getMaterial(Color.FromArgb(255, 3, 2, 0)));
            _palette.Add(@"HeadBackRight",  _getMaterial(Color.FromArgb(255, 3, 2, 2)));
            _palette.Add(@"HeadFrontRight", _getMaterial(Color.FromArgb(255, 3, 1, 2)));

            // banded head
            _palette.Add(@"HeadBase", _getMaterial(Color.FromArgb(255, 4, 3, 0)));
            _palette.Add(@"HeadBand", _getMaterial(Color.FromArgb(255, 3, 0, 0)));

            // banded helmet
            _palette.Add(@"HelmetBrow",      _getMaterial(Color.FromArgb(255, 5, 1, 0)));
            _palette.Add(@"HelmetInnerBrow", _getMaterial(Color.FromArgb(255, 6, 1, 0)));
            _palette.Add(@"HelmetBand",      _getMaterial(Color.FromArgb(255, 5, 3, 3)));
            _palette.Add(@"HelmetInnerBand", _getMaterial(Color.FromArgb(255, 6, 3, 3)));

            // hair
            _palette.Add(@"Hair", _getMaterial(Color.FromArgb(255, 7, 3, 3)));

            // TODO: serialization materials
            return _palette;
        }
        #endregion

        #region public void WriteXml(XmlWriter writer)
        public void WriteXml(XmlWriter writer)
        {
            var _palette = GetSerializationPalette();
            var _grp = GetModel(_palette) as Model3DGroup;

            #region reverse lookup of palette by color
            Func<Material, string> _lookup = (material) =>
                {
                    var _dm = material as DiffuseMaterial;
                    if (_dm != null)
                    {
                        var _scb = _dm.Brush as SolidColorBrush;
                        if (_scb != null)
                        {
                            var _reverse = (from _kvp in _palette
                                            let _diffuse = _kvp.Value as DiffuseMaterial
                                            where (_diffuse != null)
                                            let _brush = _diffuse.Brush as SolidColorBrush
                                            where (_brush != null) && (_brush.Color == _scb.Color)
                                            select _kvp.Key).FirstOrDefault();
                            if (!string.IsNullOrEmpty(_reverse))
                                return _reverse;
                        }
                    }
                    return null;
                };
            #endregion

            // namespaces for fragment
            var _uspace = @"clr-namespace:Uzi.Visualize;assembly=Uzi.Visualize";
            var _wfspace = @"http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XNamespace _uzi = _uspace;
            XNamespace _winfx = _wfspace;

            #region XAttribute for material (if defined)
            Func<string, Material, XAttribute> _materialAttribute = (tagName, material) =>
            {
                if (material != null)
                {
                    return new XAttribute(tagName,
                                 string.Format(@"{{uzi:VisualEffectMaterial Key={0}, VisualEffect={{uzi:SenseEffectExtension}} }}",
                                 _lookup(material)));
                }
                return null;
            };
            #endregion

            // get XML representation of all geometry models
            // NOTE: "{{" and "}}" for embedded curly braces in format string
            var _children = (from _geometryModel3D in _grp.Children.OfType<GeometryModel3D>()
                             let _meshGeometry3D = _geometryModel3D.Geometry as MeshGeometry3D
                             where (_meshGeometry3D != null)

                             // build XML for MeshGeometry3D
                             let _texture = new XAttribute(@"TextureCoordinates", _meshGeometry3D.TextureCoordinates.ToString())
                             let _positions = new XAttribute(@"Positions", _meshGeometry3D.Positions.ToString())
                             let _triangles = new XAttribute(@"TriangleIndices", _meshGeometry3D.TriangleIndices.ToString())
                             let _mesh = new XElement(_winfx + @"MeshGeometry3D", _texture, _positions, _triangles)

                             // build GeometryModel3D
                             let _geom = new XElement(_winfx + @"GeometryModel3D.Geometry", _mesh)
                             let _material = _materialAttribute(@"Material", _geometryModel3D.Material)
                             let _back = _materialAttribute(@"BackMaterial", _geometryModel3D.BackMaterial)
                             select (_back != null)
                             ? new XElement(_winfx + @"GeometryModel3D", _material, _back, _geom)
                             : new XElement(_winfx + @"GeometryModel3D", _material, _geom))
                             .ToList<XObject>();

            // add uzi namespace!
            _children.Insert(0, new XAttribute(XNamespace.Xmlns + @"uzi", _uspace));

            // create group
            XElement _group = new XElement(_winfx + @"Model3DGroup", _children.ToArray());
            _group.WriteTo(writer);
        }
        #endregion

        // TODO: generate brush definitions
    }
}
