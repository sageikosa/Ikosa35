using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class SurfaceBoundObject : LocatableObject
    {
        public SurfaceBoundObject(string name, bool isVisible, AnchorFace anchorFace)
            : base(name, isVisible)
        {
            AnchorFace = anchorFace;
        }
        public AnchorFace AnchorFace { get; protected set; }

        public override Sizer Sizer
            => new ObjectSizer(Size.Tiny, this);

        public override IGeometricSize GeometricSize
            => new GeometricSize(1, 1, 1);

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => null;

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        protected override string ClassIconKey
            => string.Empty;

        public override bool IsTargetable => false;
    }
}
