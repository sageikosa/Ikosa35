using System;
using Uzi.Core;

namespace Uzi.Ikosa.Items.Weapons.Natural
{
    [Serializable]
    public class NaturalSecondaryAdjunct : Adjunct
    {
        public NaturalSecondaryAdjunct(object source)
            : base(source)
        {
        }

        public override bool IsProtected { get { return true; } }

        public override object Clone()
        {
            return new NaturalSecondaryAdjunct(Source);
        }
    }
}
