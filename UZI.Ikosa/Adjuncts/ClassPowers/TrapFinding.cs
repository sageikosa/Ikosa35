using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Indicates ability to find difficult traps</summary>
    [Serializable]
    public class TrapFinding : Adjunct, ITrapFinding
    {
        /// <summary>Indicates ability to find difficult traps</summary>
        public TrapFinding(IPowerClass powerClass)
            : base(powerClass)
        {
        }

        public IPowerClass PowerClass => Source as IPowerClass;

        public override object Clone()
            => new TrapFinding(PowerClass);

        // ITrapFinding
        /// <summary>Note: still need to match or beat difficulty</summary>
        public bool CanFindTrap(ICoreObject coreObj)
            => true;

        /// <summary>Note: still need to match or beat difficulty</summary>
        public bool CanDisableTrap(ICoreObject coreObj)
            => true;
    }
}
