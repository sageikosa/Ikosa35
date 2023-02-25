using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Uzi.Core;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.UI
{
    public abstract class PresentableContext : INotifyPropertyChanged
    {
        private VisualResources _Resources;

        public abstract ICoreObject CoreObject { get; }
        public virtual PresentableCreatureVM Possessor { get; set; }

        public VisualResources VisualResources
        {
            get => _Resources;
            set
            {
                _Resources = value;
                DoPropertyChanged(nameof(VisualResources));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void DoPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
