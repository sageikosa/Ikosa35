using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Adjuncts
{
    public interface IPowerCapacity
    {
        string CapacityDescription { get; }
        bool CanUseCharges(int number);
        void UseCharges(int number);
    }
}
