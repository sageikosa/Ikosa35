using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Adjuncts
{
    public interface ISpecialAttack : IPowerDef
    {
        /// <summary>Once application of attack is resolved, this will apply the special attack</summary>
        void ApplySpecialAttack(StepInteraction deliverDamageInteraction);
        IEnumerable<Info> IdentificationInfos { get; }
        bool AllowsSave { get; }
    }
}
