using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Visualize;

namespace Uzi.Visualize
{
    [Serializable]
    public class GeometricSize : IGeometricSize
    {
        // TODO: improve performance by saving (or caching) max and conversions

        #region Construction
        public GeometricSize(IGeometricSize geomSize)
        {
            _ZExtent = geomSize.ZExtent;
            _YExtent = geomSize.YExtent;
            _XExtent = geomSize.XExtent;
        }

        public GeometricSize(IGeometricRegion region)
        {
            _ZExtent = region.UpperZ - region.LowerZ + 1;
            _YExtent = region.UpperY - region.LowerY + 1;
            _XExtent = region.UpperX - region.LowerX + 1;
        }

        public GeometricSize(double zHeight, double yLength, double xLength)
        {
            _ZExtent = zHeight;
            _YExtent = yLength;
            _XExtent = xLength;
        }
        #endregion

        #region data
        private double _ZExtent;
        private double _YExtent;
        private double _XExtent;
        #endregion

        public long ZHeight => Math.Max(Convert.ToInt64(_ZExtent), 1);
        public long YLength => Math.Max(Convert.ToInt64(_YExtent), 1);
        public long XLength => Math.Max(Convert.ToInt64(_XExtent), 1);

        public double ZExtent => _ZExtent;
        public double YExtent => _YExtent;
        public double XExtent => _XExtent;

        public long GetAxialLength(Axis axis) =>
            axis == Axis.Z ? ZHeight :
            axis == Axis.Y ? YLength :
            XLength;

        /// <summary>Center cell compatible with geometry location suppliers centered on locators</summary>
        public ICellLocation CenterCell(bool expectEven)
            => new CellPosition(
                Convert.ToInt32(Math.Floor((ZExtent - (expectEven ? 0 : 1)) / 2)),
                Convert.ToInt32(Math.Floor((YExtent - (expectEven ? 0 : 1)) / 2)),
                Convert.ToInt32(Math.Floor((XExtent - (expectEven ? 0 : 1)) / 2)));

        public static GeometricSize UnitSize()
            => new GeometricSize(1, 1, 1);

        public IEnumerable<long> Lengths
        {
            get
            {
                yield return ZHeight;
                yield return YLength;
                yield return XLength;
                yield break;
            }
        }
    }
}
