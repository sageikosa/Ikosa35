using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Uzi.Ikosa.Objects;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.UI
{
    public class HollowFurnishingLidVM : PresentableThingVM<HollowFurnishingLid>, INotifyPropertyChanged
    {
        public HollowFurnishingLidVM(HollowFurnishingLid lid, VisualResources resources)
        {
            Thing = lid;
            VisualResources = resources;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => HollowFurnishingLid.Name;
        public HollowFurnishingLid HollowFurnishingLid => Thing;
    }
}
