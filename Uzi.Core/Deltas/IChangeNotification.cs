using System;

namespace Uzi.Core
{
    public interface IChangeNotification
    {
        event EventHandler ValueChanged;
    }

    public interface ITerminating
    {
        event EventHandler Terminating;
    }
}
