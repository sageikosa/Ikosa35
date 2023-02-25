using System;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class TransparentDoor : PortalledObjectBase
    {
        public TransparentDoor(string name, Material objectMaterial, double thickness)
            : base(name, objectMaterial, thickness)
        {
            ExtraSoundDifficulty.BaseValue = 4;
        }

        protected override string ClassIconKey
            => nameof(Bench);

        public override object Clone()
            => new TransparentDoor(Name, ObjectMaterial, Thickness);
    }
}
