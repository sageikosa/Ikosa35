using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    public interface IInteractHandlerExtendable
    {
        void AddIInteractHandler(IInteractHandler handler);
        void RemoveIInteractHandler(IInteractHandler handler) ;
    }
}
