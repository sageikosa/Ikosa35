using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    public interface IFinalizeProcess
    {
        void FinalizeProcess(CoreProcess process, bool deactivated);
    }
}
