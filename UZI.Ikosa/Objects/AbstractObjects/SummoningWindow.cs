using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    /// <summary>
    /// Summoned creatures (etc) have a linked mobile placeholder.  
    /// If creature moves, so does the placeholder.
    /// If summoning is disabled by anti-magic or the like, the placeholder holds the location until the anti-magic effect ends.
    /// Creature is "located" from this, and "delocated" back to this on activation/deactivation.
    /// </summary>
    [Serializable]
    public class SummoningWindow : LocatableObject
    {
        #region data
        private ICoreObject _Summoned;
        #endregion

        public SummoningWindow(string name, ICoreObject summonedTarget)
            : base(name, false)
        {
            _Summoned = summonedTarget;
        }

        public ICoreObject SummonedTarget => _Summoned;

        // passthrough summoned object's size and geometry
        public override IGeometricSize GeometricSize => (_Summoned as ISizable).GeometricSize;
        public override Sizer Sizer => (_Summoned as ISizable).Sizer;

        public override bool IsTargetable => false;
        protected override string ClassIconKey => string.Empty;

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => null;

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        /// <summary>UnPath() and UnGroup().  Effectively removing this from gameplay.</summary>
        public void Abandon()
        {
            this.UnPath();
            this.UnGroup();
        }
    }
}
