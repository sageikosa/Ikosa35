using System.ComponentModel;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class MenuBaseViewModel : INotifyPropertyChanged
    {
        public string Order { get; set; }
        public string Key { get; set; }

        protected void DoNotify(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
