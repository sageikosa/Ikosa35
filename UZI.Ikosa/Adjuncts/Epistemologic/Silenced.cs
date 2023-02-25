using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Silenced : Adjunct
    {
        public Silenced(object source)
            : base(source)
        {
        }

        public override object Clone()
        {
            return new Silenced(Source);
        }
    }
}
