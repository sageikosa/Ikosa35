using System;
using System.ComponentModel;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        protected void DoPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
