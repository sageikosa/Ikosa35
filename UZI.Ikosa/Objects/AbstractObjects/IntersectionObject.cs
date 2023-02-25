using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class IntersectionObject : LocatableObject
    {
        public IntersectionObject(string name, Intersection intersection)
            : base(name, false)
        {
            _Intersection = intersection;
            _Geom = Visualize.GeometricSize.UnitSize();
        }

        #region data
        private Intersection _Intersection;
        private GeometricSize _Geom;
        #endregion

        public Intersection Intersection => _Intersection;

        public override Sizer Sizer
            => new ObjectSizer(Size.Fine, this);

        public override IGeometricSize GeometricSize
            => _Geom;

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => null;

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        protected override string ClassIconKey
            => string.Empty;

        public override bool IsTargetable => false;
    }
}
