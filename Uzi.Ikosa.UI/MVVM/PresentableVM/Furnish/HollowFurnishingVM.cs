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
    public class HollowFurnishingVM : PresentableThingVM<HollowFurnishing>, INotifyPropertyChanged
    {
        public HollowFurnishingVM(HollowFurnishing hollowFurnishing, VisualResources resources)
        {
            Thing = hollowFurnishing;
            VisualResources = resources;
            _RemoveCmd = new RelayCommand<ICoreObject>(
                (target) =>
                {
                    target.UnbindFromObject(Thing);
                    DoPropertyChanged(nameof(EditorTabs));
                },
                (target) => (target != null) && !(target is HollowFurnishingLid));
            _AddCmd = new RelayCommand<string>((target) => AddCompartment(target));
        }

        #region data
        private RelayCommand<ICoreObject> _RemoveCmd;
        private RelayCommand<string> _AddCmd;
        #endregion

        public string Name => HollowFurnishing.Name;
        public HollowFurnishing HollowFurnishing => Thing;
        public RelayCommand<ICoreObject> RemoveCommand => _RemoveCmd;
        public RelayCommand<string> AddCommand => _AddCmd;

        public IEnumerable<Model3DPartListItem> ResolvableModels
            => VisualResources.ResolvableModels.ToList();

        #region public bool IsLidded { get; set; }
        public bool IsLidded
        {
            get => Thing.IsLidded;
            set
            {
                if (value)
                {
                    if (!Thing.IsLidded)
                    {
                        Thing.CreateLid(Thing.ObjectMaterial).BindToObject(Thing);
                    }
                }
                else if (Thing.IsLidded)
                {
                    Thing.Anchored.OfType<HollowFurnishingLid>().FirstOrDefault()?
                        .UnbindFromObject(Thing);
                }
                DoPropertyChanged(nameof(EditorTabs));
            }
        }
        #endregion

        #region public bool IsImmobile { get; set; }
        public bool IsImmobile
        {
            get => HollowFurnishing.Adjuncts.OfType<Immobile>().Any(_i => _i.Immobilizer == HollowFurnishing);
            set
            {
                if (value)
                {
                    if (!IsImmobile)
                    {
                        HollowFurnishing.AddAdjunct(new Immobile(HollowFurnishing));
                    }
                }
                else
                {
                    HollowFurnishing
                        ?.Adjuncts
                        .OfType<Immobile>()
                        .FirstOrDefault(_i => _i.Immobilizer == HollowFurnishing)
                        ?.Eject();
                }
            }
        }
        #endregion

        public IEnumerable<TabItemVM> EditorTabs
            => (new TabItemVM
            {
                Title = HollowFurnishing?.Name,
                Content = this,
                IsSelected = true,
                ImageSource = TabItemVM.ImageSourceFromPath(@"Uzi.Ikosa.UI", @"/images/box.png")
            }).ToEnumerable()
            .Union(from _lid in HollowFurnishing?.Anchored.OfType<HollowFurnishingLid>()
                   select new TabItemVM
                   {
                       Title = _lid.Name,
                       Content = new HollowFurnishingLidVM(_lid, VisualResources),
                       ImageSource = TabItemVM.ImageSourceFromPath(@"Uzi.Ikosa.UI", @"/images/box-knob.png")
                   })
            .Union(from _m in HollowFurnishing?.Anchored
                   where !(_m is HollowFurnishingLid)
                   select new TabItemVM
                   {
                       Title = _m.Name,
                       Content = _m,
                       ImageSource = TabItemVM.ImageSourceFromPath(@"Uzi.Ikosa.UI",
                            _m is ContainerObject ? @"/images/box.png" : @"/images/box-knob.png")
                   }).ToList();

        #region private void AddCompartment(string parameter)
        private void AddCompartment(string parameter)
        {
            ICoreObject _getItem()
            {
                var _container = new ContainerObject(@"compartment", HollowFurnishing.ObjectMaterial, true, true)
                {
                    MaxStructurePoints = 1,
                    MaximumLoadWeight = 100,
                    TareWeight = 0
                };
                switch (parameter)
                {
                    case @"Interior":
                        _container.Name = @"Interior";
                        _container.BindToObject(HollowFurnishing);
                        _container.AddAdjunct(new ConnectedSides(SideIndex.Top));
                        return _container;

                    case @"Drawer":
                        var _closeable = new Drawer(@"Drawer", HollowFurnishing.ObjectMaterial, _container, true, 1)
                        {
                            MaxStructurePoints = 10
                        };
                        _closeable.TareWeight = 2;
                        _closeable.OpenWeight = 2 / 5;

                        var _bind = new CloseableContainerBinder(HollowFurnishing);
                        _closeable.AddAdjunct(_bind);
                        _closeable.AddAdjunct(new ConnectedSides(SideIndex.Front));
                        return _closeable;
                    default:
                        return null;
                }
            }
            var _item = _getItem();
            if (_item != null)
            {
                DoPropertyChanged(nameof(EditorTabs));
            }
        }
        #endregion
    }
}
