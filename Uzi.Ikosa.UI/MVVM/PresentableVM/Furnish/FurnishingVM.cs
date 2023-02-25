using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Objects;
using Uzi.Visualize;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.UI
{
    public class FurnishingVM : PresentableThingVM<Furnishing>, INotifyPropertyChanged
    {
        public FurnishingVM(Furnishing furnishing, VisualResources resources)
        {
            Thing = furnishing;
            VisualResources = resources;
            _RemoveCmd = new RelayCommand<ICoreObject>(
                (target) =>
                {
                    target.UnbindFromObject(Thing);
                    DoPropertyChanged(nameof(EditorTabs));
                },
                (target) => target != null);
            _AddCmd = new RelayCommand<string>((target) => AddCompartment(target));
        }

        #region data
        private readonly RelayCommand<ICoreObject> _RemoveCmd;
        private readonly RelayCommand<string> _AddCmd;
        #endregion

        public string Name => Furnishing.Name;
        public Furnishing Furnishing => Thing;
        public RelayCommand<ICoreObject> RemoveCommand => _RemoveCmd;
        public RelayCommand<string> AddCommand => _AddCmd;

        #region public bool IsImmobile { get; set; }
        public bool IsImmobile
        {
            get => Thing.Adjuncts.OfType<Immobile>().Any(_i => _i.Immobilizer == Furnishing);
            set
            {
                if (value)
                {
                    if (!IsImmobile)
                    {
                        Furnishing.AddAdjunct(new Immobile(Furnishing));
                    }
                }
                else
                {
                    Furnishing
                        ?.Adjuncts
                        .OfType<Immobile>()
                        .FirstOrDefault(_i => _i.Immobilizer == Furnishing)
                        ?.Eject();
                }
            }
        }
        #endregion

        public IEnumerable<TabItemVM> EditorTabs
            => (new TabItemVM
            {
                Title = Furnishing?.Name,
                Content = this,
                IsSelected = true,
                ImageSource = TabItemVM.ImageSourceFromPath(@"Uzi.Ikosa.UI", @"/images/box.png")
            }).ToEnumerable()
            .Union(from _m in Furnishing?.Anchored
                   select new TabItemVM
                   {
                       Title = _m.Name,
                       Content = _m.GetPresentableObjectVM(VisualResources, null),
                       ImageSource = TabItemVM.ImageSourceFromPath(@"Uzi.Ikosa.UI",
                            _m is ContainerObject ? @"/images/box.png" : @"/images/box-knob.png")
                   }).ToList();

        #region private void AddCompartment(string parameter)
        private void AddCompartment(string parameter)
        {
            ICoreObject _getItem()
            {
                var _container = new ContainerObject(@"compartment", Furnishing.ObjectMaterial, true, true)
                {
                    MaxStructurePoints = 10,
                    MaximumLoadWeight = 100,
                    TareWeight = 2
                };
                switch (parameter)
                {
                    case @"Shelf":
                        _container.Name = @"Shelf";
                        _container.BindToObject(Furnishing);
                        // TODO: sides depend on "openness" of shelf
                        return _container;

                    case @"Drawer":
                        var _closeable = new Drawer(@"Drawer", Furnishing.ObjectMaterial, _container, true, 1)
                        {
                            MaxStructurePoints = 10
                        };
                        _container.TareWeight = 0;
                        _closeable.TareWeight = 2;
                        _closeable.OpenWeight = 2 / 5;

                        var _bind = new CloseableContainerBinder(Furnishing);
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
