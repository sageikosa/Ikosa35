using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Indicates that the object can be used as a fastener for a hasp
    /// </summary>
    [Serializable]
    public class Fastener : Adjunct
    {
        public Fastener(object source)
            : base(source)
        {
        }

        public override object Clone()
        {
            return new Fastener(Source);
        }
    }
}
