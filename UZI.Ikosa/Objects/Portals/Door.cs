using System;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Door : PortalledObjectBase
    {
        public Door(string name, Material objectMaterial, double thickness)
            : base(name, objectMaterial, thickness)
        {
            ExtraSoundDifficulty.BaseValue = 5;
            DoesBlocksLineOfEffect = true;          // line of effect generally stopped by doors
            DoesBlocksSpread = true;                // spreads generally stopped by doors
            BlocksMove = true;                  // transit generally stopped by doors
            DoesHindersMove = true;                 // transit generally hindered by doors
            DoesSupplyCover = CoverLevel.Hard;    // door can generally provide hard cover
        }

        protected override string ClassIconKey
            => nameof(Door);

        public override object Clone()
            => new Door(Name, ObjectMaterial, Thickness);
    }
}
