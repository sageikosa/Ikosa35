using System.Linq;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public static class PlaneListShellBuilder
    {
        private static Point3D[] _Points = new Point3D[]
        {
            (new Point3D(0, 0, 0)),  // 0
            (new Point3D(0, 0, 5)),  // 1
            (new Point3D(0, 5, 0)),  // 2
            (new Point3D(0, 5, 5)),  // 3
            (new Point3D(5, 0, 0)),  // 4
            (new Point3D(5, 0, 5)),  // 5
            (new Point3D(5, 5, 0)),  // 6
            (new Point3D(5, 5, 5))   // 7
        };

        private static PlanarPoints BuildPlanarPoints(Vector3D offset, params int[] ptIndex)
        {
            return new PlanarPoints(ptIndex.Select(_pi => _Points[_pi] + offset).ToArray());
        }

        #region public static PlaneListShell GetPyramid(Vector3D offset, AnchorFace baseFace, params AnchorFace[] sides)
        public static PlaneListShell GetPyramid(Vector3D offset, AnchorFace baseFace, params AnchorFace[] sides)
        {
            var _shell = new PlaneListShell();

            // builders
            void _tri(int p1, int p2, int p3)
            {
                _shell.Add(BuildPlanarPoints(offset, p1, p2, p3));
            };
            void _sides(int top, int bottom, int nose, int wing1, int wing2)
            {
                _tri(top, bottom, wing1);
                _tri(top, wing2, bottom);
                _tri(top, wing1, nose);
                _tri(top, nose, wing2);
            };

            switch (baseFace)
            {
                case AnchorFace.ZLow:
                    #region ZLow
                    _shell.Add(BuildPlanarPoints(offset, 0, 2, 6, 4));
                    if (sides.Contains(AnchorFace.XLow))
                    {
                        if (sides.Contains(AnchorFace.YLow))
                        {
                            _sides(1, 0, 6, 4, 2);
                        }
                        else // YHigh
                        {
                            _sides(3, 2, 4, 0, 6);
                        }
                    }
                    else // XHigh
                    {
                        if (sides.Contains(AnchorFace.YLow))
                        {
                            _sides(5, 4, 2, 6, 0);
                        }
                        else // YHigh
                        {
                            _sides(7, 6, 0, 2, 4);
                        }
                    }
                    break;
                #endregion

                case AnchorFace.ZHigh:
                    #region ZHigh
                    _shell.Add(BuildPlanarPoints(offset, 1, 5, 7, 3));
                    if (sides.Contains(AnchorFace.XLow))
                    {
                        if (sides.Contains(AnchorFace.YLow))
                        {
                            _sides(0, 1, 7, 3, 5);
                        }
                        else // YHigh
                        {
                            _sides(2, 3, 5, 7, 1);
                        }
                    }
                    else // ZHigh, XHigh
                    {
                        if (sides.Contains(AnchorFace.YLow))
                        {
                            _sides(4, 5, 3, 1, 7);
                        }
                        else // YHigh
                        {
                            _sides(6, 7, 1, 5, 3);
                        }
                    }
                    break;
                #endregion

                case AnchorFace.YLow:
                    #region YLow
                    _shell.Add(BuildPlanarPoints(offset, 0, 4, 5, 1));
                    if (sides.Contains(AnchorFace.XLow))
                    {
                        if (sides.Contains(AnchorFace.ZLow))
                        {
                            _sides(2, 0, 5, 1, 4);
                        }
                        else // ZHigh
                        {
                            _sides(3, 1, 4, 5, 0);
                        }
                    }
                    else // YLow, XHigh
                    {
                        if (sides.Contains(AnchorFace.ZLow))
                        {
                            _sides(6, 4, 1, 0, 5);
                        }
                        else // ZHigh
                        {
                            _sides(7, 5, 0, 4, 1);
                        }
                    }
                    break;
                #endregion

                case AnchorFace.YHigh:
                    #region YHigh
                    _shell.Add(BuildPlanarPoints(offset, 3, 7, 6, 2));
                    if (sides.Contains(AnchorFace.XLow))
                    {
                        if (sides.Contains(AnchorFace.ZLow))
                        {
                            _sides(0, 2, 7, 6, 3);
                        }
                        else // ZHigh
                        {
                            _sides(1, 3, 6, 2, 7);
                        }
                    }
                    else // YHigh, XHigh
                    {
                        if (sides.Contains(AnchorFace.ZLow))
                        {
                            _sides(4, 6, 3, 7, 2);
                        }
                        else // ZHigh
                        {
                            _sides(5, 7, 2, 3, 6);
                        }
                    }
                    break;
                #endregion

                case AnchorFace.XLow:
                    #region XLow
                    _shell.Add(BuildPlanarPoints(offset, 0, 1, 3, 2));
                    if (sides.Contains(AnchorFace.ZLow))
                    {
                        if (sides.Contains(AnchorFace.YLow))
                        {
                            _sides(4, 0, 3, 2, 1);
                        }
                        else // YHigh
                        {
                            _sides(6, 2, 1, 3, 0);
                        }
                    }
                    else // XLow, ZHigh
                    {
                        if (sides.Contains(AnchorFace.YLow))
                        {
                            _sides(5, 1, 2, 0, 3);
                        }
                        else // YHigh
                        {
                            _sides(7, 3, 0, 1, 2);
                        }
                    }
                    break;
                #endregion

                case AnchorFace.XHigh:
                default:
                    #region XHigh
                    _shell.Add(BuildPlanarPoints(offset, 4, 6, 7, 5));
                    if (sides.Contains(AnchorFace.ZLow))
                    {
                        if (sides.Contains(AnchorFace.YLow))
                        {
                            _sides(0, 4, 7, 5, 6);
                        }
                        else // YHigh
                        {
                            _sides(2, 6, 5, 4, 7);
                        }
                    }
                    else // XHigh, ZHigh
                    {
                        if (sides.Contains(AnchorFace.YLow))
                        {
                            _sides(1, 5, 6, 7, 4);
                        }
                        else // YHigh
                        {
                            _sides(3, 7, 4, 6, 5);
                        }
                    }
                    break;
                    #endregion
            }

            return _shell;
        }
        #endregion

        #region public static PlaneListShell GetTriangularPrism(Vector3D offset, params AnchorFace[] faces)
        /// <summary>Gets PlaneShellList for a triangular prism with 2 faces</summary>
        public static PlaneListShell GetTriangularPrism(Vector3D offset, params AnchorFace[] faces)
        {
            var _shell = new PlaneListShell();
            if (faces.Contains(AnchorFace.XLow))
            {
                _shell.Add(BuildPlanarPoints(offset, 0, 1, 3, 2));
                if (faces.Contains(AnchorFace.YLow))
                {
                    _shell.Add(BuildPlanarPoints(offset, 3, 1, 5));
                    _shell.Add(BuildPlanarPoints(offset, 4, 0, 2));
                    _shell.Add(BuildPlanarPoints(offset, 5, 4, 2, 3));
                }
                else if (faces.Contains(AnchorFace.YHigh))
                {
                    _shell.Add(BuildPlanarPoints(offset, 7, 3, 1));
                    _shell.Add(BuildPlanarPoints(offset, 0, 2, 6));
                    _shell.Add(BuildPlanarPoints(offset, 1, 0, 6, 7));
                }
                else if (faces.Contains(AnchorFace.ZLow))
                {
                    _shell.Add(BuildPlanarPoints(offset, 1, 0, 4));
                    _shell.Add(BuildPlanarPoints(offset, 6, 2, 3));
                    _shell.Add(BuildPlanarPoints(offset, 1, 4, 6, 3));
                }
                else if (faces.Contains(AnchorFace.ZHigh))
                {
                    _shell.Add(BuildPlanarPoints(offset, 5, 1, 0));
                    _shell.Add(BuildPlanarPoints(offset, 2, 3, 7));
                    _shell.Add(BuildPlanarPoints(offset, 5, 0, 2, 7));
                }
            }
            else if (faces.Contains(AnchorFace.XHigh))
            {
                _shell.Add(BuildPlanarPoints(offset, 4, 6, 7, 5));
                if (faces.Contains(AnchorFace.YLow))
                {
                    _shell.Add(BuildPlanarPoints(offset, 1, 5, 7));
                    _shell.Add(BuildPlanarPoints(offset, 6, 4, 0));
                    _shell.Add(BuildPlanarPoints(offset, 0, 1, 7, 6));
                }
                else if (faces.Contains(AnchorFace.YHigh))
                {
                    _shell.Add(BuildPlanarPoints(offset, 5, 7, 3));
                    _shell.Add(BuildPlanarPoints(offset, 2, 6, 4));
                    _shell.Add(BuildPlanarPoints(offset, 5, 3, 2, 4));
                }
                else if (faces.Contains(AnchorFace.ZLow))
                {
                    _shell.Add(BuildPlanarPoints(offset, 0, 4, 5));
                    _shell.Add(BuildPlanarPoints(offset, 7, 6, 2));
                    _shell.Add(BuildPlanarPoints(offset, 0, 5, 7, 2));
                }
                else if (faces.Contains(AnchorFace.ZHigh))
                {
                    _shell.Add(BuildPlanarPoints(offset, 4, 5, 1));
                    _shell.Add(BuildPlanarPoints(offset, 3, 7, 6));
                    _shell.Add(BuildPlanarPoints(offset, 4, 1, 3, 6));
                }
            }

            if (faces.Contains(AnchorFace.YLow))
            {
                _shell.Add(BuildPlanarPoints(offset, 0, 4, 5, 1));
                if (faces.Contains(AnchorFace.ZLow))
                {
                    _shell.Add(BuildPlanarPoints(offset, 2, 0, 1));
                    _shell.Add(BuildPlanarPoints(offset, 5, 4, 6));
                    _shell.Add(BuildPlanarPoints(offset, 1, 5, 6, 2));
                }
                else if (faces.Contains(AnchorFace.ZHigh))
                {
                    _shell.Add(BuildPlanarPoints(offset, 0, 1, 3));
                    _shell.Add(BuildPlanarPoints(offset, 7, 5, 4));
                    _shell.Add(BuildPlanarPoints(offset, 3, 7, 4, 0));
                }
            }
            else if (faces.Contains(AnchorFace.YHigh))
            {
                _shell.Add(BuildPlanarPoints(offset, 2, 3, 7, 6));
                if (faces.Contains(AnchorFace.ZLow))
                {
                    _shell.Add(BuildPlanarPoints(offset, 3, 2, 0));
                    _shell.Add(BuildPlanarPoints(offset, 4, 6, 7));
                    _shell.Add(BuildPlanarPoints(offset, 3, 0, 4, 7));
                }
                else if (faces.Contains(AnchorFace.ZHigh))
                {
                    _shell.Add(BuildPlanarPoints(offset, 1, 3, 2));
                    _shell.Add(BuildPlanarPoints(offset, 6, 7, 5));
                    _shell.Add(BuildPlanarPoints(offset, 1, 2, 6, 5));
                }
            }

            if (faces.Contains(AnchorFace.ZLow))
            {
                _shell.Add(BuildPlanarPoints(offset, 0, 2, 6, 4));
            }
            else if (faces.Contains(AnchorFace.ZHigh))
            {
                _shell.Add(BuildPlanarPoints(offset, 1, 5, 7, 3));
            }
            return _shell;
        }
        #endregion
    }
}
