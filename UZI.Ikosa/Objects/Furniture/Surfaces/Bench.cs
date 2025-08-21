using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Bench : SupportedSurface
    {
        public Bench(Items.Materials.Material material)
            : base(nameof(Bench), material)
        {
        }

        protected override SupportedSurface GetClone()
            => new Bench(ObjectMaterial);

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
            => nameof(Bench);
    }
}
