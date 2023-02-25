using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Bars: PortalledObjectBase
    {
        public Bars(string name, Material objectMaterial, double thickness)
            : base(name, objectMaterial, thickness)
        {
            // TODO: block/hinder by size
            ExtraSoundDifficulty.BaseValue = 1;
            Opacity = 0.2;                      // bars do not generally stop light
            BlocksMove = true;                  // bars are meant to bar movement
            DoesHindersMove = true;                 // they can hinder transit also as needed
            DoesSupplyCover = CoverLevel.Hard;    // bars generally provide cover
            DoesSupplyConcealment = true;         // can make a hide check with bars
            // TODO: bend-bars to alter blockage
        }

        protected override string ClassIconKey
            => nameof(Bars);

        public override object Clone()
            => new Bars(Name, ObjectMaterial, Thickness);
    }
}
