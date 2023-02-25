using System;

namespace Uzi.Core
{
    public interface IInteractProvider
    {
        IInteract GetIInteract(Guid id);
    }
}
