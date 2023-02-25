using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Actions
{
    public interface ISuccessCheckPrerequisite : IStepPrerequisite
    {
        bool Success { get; }
    }
}
