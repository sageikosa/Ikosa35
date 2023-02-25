using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class CampSiteFocus : LocatableObject
    {
        public CampSiteFocus(IGeometricSize size)
            : base(@"Camp Site", false)
        {
            _Geom = size;
        }

        #region data
        private IGeometricSize _Geom;
        #endregion

        public override bool IsTargetable => false;
        protected override string ClassIconKey => @"camp_site";

        public override Sizer Sizer
            => new ObjectSizer(Size.Fine, this);

        public override IGeometricSize GeometricSize
            => _Geom;

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => null;

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        /// <summary>UnPath() and UnGroup(); effectively removing this from gameplay.</summary>
        public void Abandon()
        {
            this.UnPath();
            this.UnGroup();
        }
    }
}
