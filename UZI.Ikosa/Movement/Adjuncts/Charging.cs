using System;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class Charging : Adjunct
    {
        public Charging(object source)
            : base(source)
        {
        }

        public override object Clone()
        {
            return new Charging(Source);
        }
    }
}
