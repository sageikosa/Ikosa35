using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class PowerCapacity : Adjunct, IPowerCapacity, IProtectable
    {
        public PowerCapacity(object source)
            : base(source)
        {
        }

        public virtual string CapacityDescription
            => @"at will";

        public bool CanUseCharges(int number)
            => true;

        public override object Clone()
            => new PowerCapacity(Source);

        public void UseCharges(int number) { }

        public bool IsExposedTo(Creature critter)
            => this.HasExposureTo(critter);
    }
}
