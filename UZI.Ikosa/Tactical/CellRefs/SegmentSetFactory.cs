using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    public class SegmentSetFactory
    {
        public SegmentSetFactory(LocalMap map, IGeometricRegion sourceRegion, IGeometricRegion targetRegion,
            ITacticalInquiry[] exclusions, SegmentSetProcess setType)
        {
            Map = map;
            Exclusions = exclusions;
            SegmentSetProcess = setType;
            SourceRegion = sourceRegion;
            TargetRegion = targetRegion;
        }

        public SegmentSetProcess SegmentSetProcess { get; private set; }
        public LocalMap Map { get; private set; }
        public ITacticalInquiry[] Exclusions { get; private set; }
        public IGeometricRegion SourceRegion { get; private set; }
        public IGeometricRegion TargetRegion { get; private set; }

        public SegmentSet CreateSegmentSet(Point3D startPoint, Point3D endPoint, PlanarPresence planar)
        {
            return new SegmentSet(Map, startPoint, endPoint, SourceRegion, TargetRegion, Exclusions, 
                SegmentSetProcess, planar);
        }

        public bool CanAddMore(SegmentSet segmentSet)
        {
            switch (SegmentSetProcess)
            {
                case SegmentSetProcess.Geometry:
                    return true;

                case SegmentSetProcess.Detect:
                    return segmentSet.IsLineOfDetect;

                case SegmentSetProcess.Observation:
                case SegmentSetProcess.Effect:
                default:
                    return segmentSet.IsLineOfEffect;
            }
        }
    }

    /// <summary>
    /// Geometry | Effect | Observation | Detect
    /// </summary>
    public enum SegmentSetProcess
    {
        Geometry,
        Effect,
        Observation,
        Detect
    }
}
