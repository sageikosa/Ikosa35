using System.Collections.Generic;
using System.Windows;

namespace Uzi.Ikosa.Tactical
{
    public class MovementSlice
    {
        private List<MovementStrip> _Strips = new List<MovementStrip>();

        /// <summary>Strip-Axial Offset of slice from base</summary>
        public int SliceOffset { get; set; }

        /// <summary>Index of first usable strip</summary>
        public int StartIndex { get; set; }

        /// <summary>Index of last usable strip</summary>
        public int EndIndex { get; set; }

        /// <summary>Start coordinate</summary>
        public double Start { get; set; }

        /// <summary>Start coordinate</summary>
        public double End { get; set; }

        /// <summary>Available usable extent</summary>
        public double Extent { get { return End - Start; } }

        /// <summary></summary>
        public double Coordinate { get; set; }

        public List<MovementStrip> Strips { get { return _Strips; } }

        public double DistanceFromStartToSliceEnd(MovementSlice endSlice)
        {
            Point _start = new Point(Start, Coordinate);
            Point _end = new Point(endSlice.End, endSlice.Coordinate);
            return (_end - _start).Length;
        }

        public double DistanceFromEndToSliceStart(MovementSlice startSlice)
        {
            Point _start = new Point(startSlice.Start, startSlice.Coordinate);
            Point _end = new Point(End, Coordinate);
            return (_end - _start).Length;
        }
    }
}
