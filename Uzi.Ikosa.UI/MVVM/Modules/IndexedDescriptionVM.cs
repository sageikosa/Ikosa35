using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Uzi.Ikosa.UI
{
    public class IndexedDescriptionVM : INotifyPropertyChanged
    {
        private readonly DescriptionVM _Parent;
        private string _Description;

        public IndexedDescriptionVM(DescriptionVM parent, string description)
        {
            _Parent = parent;
            _Description = description;
        }

        public string Description
        {
            get => _Description;
            set
            {
                // edit VM
                _Description = value;

                // edit model
                _Parent.DoSyncDescription(this);
                DoPropertyChanged(nameof(Description));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void DoPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
