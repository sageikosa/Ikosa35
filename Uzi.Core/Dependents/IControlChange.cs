using System.ComponentModel;

namespace Uzi.Core
{
    public interface IControlChange<ChangeType>: INotifyPropertyChanged
    {
        void AddChangeMonitor(IMonitorChange<ChangeType> monitor);
        void RemoveChangeMonitor(IMonitorChange<ChangeType> monitor);
    }
}
