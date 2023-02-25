using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class Impassible : Adjunct
    {
        public Impassible(object source)
            : base(source)
        {
        }

        public CoreObject CoreObject
            => Anchor as CoreObject;

        public override bool IsProtected => true;

        public override object Clone()
            => new Impassible(Source);
    }

    public static class ImpassibleHelper
    {
        public static bool IsImpassible(this CoreObject self)
            => self.HasActiveAdjunct<Impassible>();
    }
}
