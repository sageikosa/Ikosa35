using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.UI
{
    public class FlexibleFlatPanelVM : PresentableThingVM<FlexibleFlatPanel>, INotifyPropertyChanged
    {
        public FlexibleFlatPanelVM(FlexibleFlatPanel flexibleFlatPanel, VisualResources resource)
        {
            Thing = flexibleFlatPanel;
            VisualResources = resource;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => FlexibleFlatPanel.Name;
        public FlexibleFlatPanel FlexibleFlatPanel => Thing;

        public IEnumerable<Model3DPartListItem> ResolvableModels
            => VisualResources.ResolvableModels.ToList();

        public IEnumerable<TabItemVM> EditorTabs
            => (new TabItemVM
            {
                Title = FlexibleFlatPanel?.Name,
                Content = this,
                IsSelected = true,
                ImageSource = TabItemVM.ImageSourceFromPath(@"Uzi.Ikosa.UI", @"/images/box.png")
            }).ToEnumerable()
            .Union(from _m in FlexibleFlatPanel?.Anchored
                   select new TabItemVM
                   {
                       Title = _m.Name,
                       Content = _m,
                       ImageSource = TabItemVM.ImageSourceFromPath(@"Uzi.Ikosa.UI",
                            _m is ContainerObject ? @"/images/box.png" : @"/images/box-knob.png")
                   }).ToList();
    }
}
