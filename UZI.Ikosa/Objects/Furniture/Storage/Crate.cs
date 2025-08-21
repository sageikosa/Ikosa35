using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Crate : HollowFurnishing
    {
        public Crate(Material material)
            : base(nameof(Crate), material)
        {
        }

        public override HollowFurnishingLid CreateLid(Material material)
            => new CrateLid(material)
            {
                Width = Width,
                Length = Length,
                Height = 0.0825,
                TareWeight = TareWeight / 2,
                MaxStructurePoints = MaxStructurePoints / 2
            };

        public override bool IsHardSnap(AnchorFace face)
            => true;

        protected override HollowFurnishing GetClone()
            => new Crate(ObjectMaterial);

        public override IEnumerable<string> IconKeys
        {
            get
            {
                // provide any overrides
                foreach (var _iKey in IconKeyAdjunct.GetIconKeys(this))
                {
                    yield return _iKey;
                }

                // material class combination
                yield return $@"{ObjectMaterial?.Name}_{ClassIconKey}";

                // ... and then the class key
                yield return ClassIconKey;
                yield break;
            }
        }

        protected override string ClassIconKey
            => nameof(Crate);
    }

    [Serializable]
    public class CrateLid : HollowFurnishingLid
    {
        public CrateLid(Material material)
            : base(@"Crate Lid", material)
        {
        }

        public override object Clone()
            => new CrateLid(ObjectMaterial);

        public override bool IsValidLid(HollowFurnishing hollowFurnishing)
            => hollowFurnishing is Crate;

        protected override string ClassIconKey => nameof(CrateLid);
    }
}
