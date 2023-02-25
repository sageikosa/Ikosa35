using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Interactions
{
    public interface IStructureDamage : ICore
    {
        int GetHardness();
        int StructurePoints { get; set; }

        /// <summary>Reduction to falling damage dice due to object characteristics</summary>
        double FallReduce { get; }

        /// <summary>Maximum speed for falling</summary>
        int MaxFallSpeed { get; }
    }
}
