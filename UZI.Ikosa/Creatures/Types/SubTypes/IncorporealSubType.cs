using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Creatures.SubTypes
{
    [Serializable]
    public class IncorporealSubType : CreatureSubType
    {
        public IncorporealSubType(object source) : base(source) { }

        public override string Name => @"Incorporeal";

        public override CreatureSubType Clone(object source)
            => new IncorporealSubType(source);
    }
}
