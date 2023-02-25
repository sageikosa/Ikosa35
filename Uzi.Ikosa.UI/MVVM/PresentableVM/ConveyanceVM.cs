using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.UI
{
    public class ConveyanceVM : PresentableThingVM<Conveyance>, INotifyPropertyChanged
    {
        public ConveyanceVM(Conveyance convey)
        {
            Thing = convey;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => Conveyance.Name;
        public Conveyance Conveyance => Thing;

        public IEnumerable<TabItemVM> EditorTabs
            => (new TabItemVM
            {
                Title = Conveyance?.Name,
                Content = this,
                IsSelected = true,
                ImageSource = TabItemVM.ImageSourceFromPath(@"Uzi.Ikosa.UI", @"/images/box.png")
            }).ToEnumerable()
            .Union(from _m in Conveyance?.Anchored
                   select new TabItemVM
                   {
                       Title = _m.Name,
                       Content = _m,
                       ImageSource = TabItemVM.ImageSourceFromPath(@"Uzi.Ikosa.UI",
                            _m is ContainerObject ? @"/images/box.png" : @"/images/box-knob.png")
                   }).ToList();
    }
}
