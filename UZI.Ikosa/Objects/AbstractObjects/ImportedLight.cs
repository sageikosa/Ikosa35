using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    /// <summary>
    /// Used to define light from external environment sources within the confines of a local map
    /// </summary>
    [Serializable]
    public class ImportedLight : LocatableObject
    {
        public ImportedLight(Illumination light)
            : base(@"Imported Light", false)
        {
            // adjunct
            AddAdjunct(light);
            light.IsActive = true;
        }

        public override IGeometricSize GeometricSize
            => new GeometricSize(1,1,1);

        public Illumination Illumination
            => Adjuncts.OfType<Illumination>().FirstOrDefault(_i => _i.Source.Equals(typeof(ImportedLight))); 

        public override Sizer Sizer
            => new ObjectSizer(Size.Medium, this);

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => null;

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        protected override string ClassIconKey
            => string.Empty;

        public override bool IsTargetable => false;
    }
}
