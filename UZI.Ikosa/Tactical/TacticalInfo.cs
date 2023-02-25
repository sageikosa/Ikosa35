using Uzi.Core;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using System;
using System.Collections.Generic;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Not-Serializable
    /// </summary>
    public readonly struct TacticalInfo
    {
        public TacticalInfo(in SegmentRef segmentRef, in Point3D sourcePoint, in Point3D targetPoint,
            IGeometricRegion sourceRegion, IGeometricRegion targetRegion)
        {
            _TacticalCellRef = segmentRef;
            _SourcePoint = sourcePoint;
            _TargetPoint = targetPoint;
            _SourceRegion = sourceRegion;
            _TargetRegion = targetRegion;
        }

        #region data
        private readonly SegmentRef _TacticalCellRef;
        private readonly Point3D _SourcePoint;
        private readonly Point3D _TargetPoint;
        private readonly IGeometricRegion _SourceRegion;
        private readonly IGeometricRegion _TargetRegion;
        #endregion

        public SegmentRef TacticalCellRef => _TacticalCellRef;
        public Point3D SourcePoint => _SourcePoint;
        public Point3D TargetPoint => _TargetPoint;
        public IGeometricRegion SourceRegion => _SourceRegion;
        public IGeometricRegion TargetRegion => _TargetRegion;

        #region public bool CrossesFace(AnchorFace face)
        /// <summary>True if entry or exit point is within 1/100 foot from appropriate face</summary>
        public bool CrossesFace(AnchorFace face)
        {
            var _ord = 0d;
            bool _either(double a, double b)
                => a.CloseEnough(_ord, 0.01d) || b.CloseEnough(_ord, 0.01d);
            bool _between(double a, double b)
                => _ord.Between(a, b);

            switch (face)
            {
                case AnchorFace.XLow:
                    _ord = TacticalCellRef.X * 5d;
                    return _either(TacticalCellRef.EntryPoint.X, TacticalCellRef.ExitPoint.X)
                        && _between(SourcePoint.X, TargetPoint.X);

                case AnchorFace.YLow:
                    _ord = TacticalCellRef.Y * 5d;
                    return _either(TacticalCellRef.EntryPoint.Y, TacticalCellRef.ExitPoint.Y)
                        && _between(SourcePoint.Y, TargetPoint.Y);

                case AnchorFace.ZLow:
                    _ord = TacticalCellRef.Z * 5d;
                    return _either(TacticalCellRef.EntryPoint.Z, TacticalCellRef.ExitPoint.Z)
                        && _between(SourcePoint.Z, TargetPoint.Z);

                case AnchorFace.XHigh:
                    _ord = (TacticalCellRef.X + 1) * 5d;
                    return _either(TacticalCellRef.EntryPoint.X, TacticalCellRef.ExitPoint.X)
                        && _between(SourcePoint.X, TargetPoint.X);

                case AnchorFace.YHigh:
                    _ord = (TacticalCellRef.Y + 1) * 5d;
                    return _either(TacticalCellRef.EntryPoint.Y, TacticalCellRef.ExitPoint.Y)
                        && _between(SourcePoint.Y, TargetPoint.Y);

                case AnchorFace.ZHigh:
                    _ord = (TacticalCellRef.Z + 1) * 5d;
                    return _either(TacticalCellRef.EntryPoint.Z, TacticalCellRef.ExitPoint.Z)
                        && _between(SourcePoint.Z, TargetPoint.Z);

                default:
                    return false;
            }
        }
        #endregion
    }
}
