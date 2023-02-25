using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.UI
{
    public class ItemValueEnablerVM : ValueEnablerVM
    {
        private readonly ObservableCollection<ItemElementVM> _Items;

        public ItemValueEnablerVM(ValueEnablersFolderVM folder, ItemValueEnabler requirement)
            : base(folder, requirement)
        {
            _Items = new ObservableCollection<ItemElementVM>(ItemRequirement.ItemIDs
                .Select(_ie =>
                {
                    var (_module, _item) = IkosaModule?.GetItemElement(_ie) ?? default;
                    return new ItemElementVM(_module, _item);
                })
                .Where(_i => _i.ItemElement != null));
            Commands = GetDefaultCommands();
        }

        public ItemValueEnabler ItemRequirement => Enabler as ItemValueEnabler;

        public ObservableCollection<ItemElementVM> RequiredItems => _Items;

        public override Commandable GetDefaultCommands()
            => new EditItemValueEnablerCommands(this);
    }

    public class EditItemValueEnablerCommands : ValueEnablerEditCommands
    {
        private readonly List<Module> _Modules;
        private Module _SelectedModule;
        private readonly ObservableCollection<ItemElementVM> _Resolvable;
        private readonly ObservableCollection<ItemElementVM> _Items;

        public EditItemValueEnablerCommands(ItemValueEnablerVM itemValueEnabler)
            : base(itemValueEnabler)
        {
            // module
            _Modules = itemValueEnabler.IkosaModule.ToEnumerable().Concat(
                itemValueEnabler.IkosaModule.Resources.Imports.Modules
                .Select(_m => _m.Module)
                .Where(_m => _m != null))
                .ToList();
            _SelectedModule = _Modules.FirstOrDefault();

            // items
            _Resolvable = new ObservableCollection<ItemElementVM>(_SelectedModule.ItemElements
                .Select(_ie => new ItemElementVM(itemValueEnabler.IkosaModule, _ie.Value)));
            _Items = new ObservableCollection<ItemElementVM>(itemValueEnabler.RequiredItems);

            // commands
            DoEditCommand = new RelayCommand(() =>
            {
                // sync name
                itemValueEnabler.Name = Name;

                // sync items
                itemValueEnabler.RequiredItems.Clear();
                foreach (var _item in RequiredItems)
                {
                    itemValueEnabler.RequiredItems.Add(_item);
                }

                // sync description
                UpdateDescription();

                // done
                itemValueEnabler.Commands = itemValueEnabler.GetDefaultCommands();
            }, () => !string.IsNullOrWhiteSpace(Name) /* TODO: ITEM ... // && (_SelectedInfoKey != null)*/);

            AddItemCommand = new RelayCommand<ItemElementVM>(target =>
            {
                if (!RequiredItems.Any(_i => _i.ItemElement.ID == target.ItemElement.ID))
                {
                    RequiredItems.Add(target);
                }
            }, target => !RequiredItems.Any(_i => _i.ItemElement.ID == target?.ItemElement.ID));
            RemoveItemCommand = new RelayCommand<ItemElementVM>(target =>
            {
                _ = RequiredItems.Remove(target);
            });
        }

        public List<Module> Modules => _Modules;

        public Module SelectedModule
        {
            get => _SelectedModule;
            set
            {
                if (value != _SelectedModule)
                {
                    _SelectedModule = value;
                    ItemElements.Clear();
                    foreach (var _ik in _SelectedModule.ItemElements.Select(_kvp => _kvp.Value))
                    {
                        ItemElements.Add(new ItemElementVM(_SelectedModule, _ik));
                    }
                    DoPropertyChanged(nameof(SelectedModule));
                }
            }
        }

        public ObservableCollection<ItemElementVM> ItemElements => _Resolvable;
        public ObservableCollection<ItemElementVM> RequiredItems => _Items;

        public RelayCommand<ItemElementVM> AddItemCommand { get; private set; }
        public RelayCommand<ItemElementVM> RemoveItemCommand { get; private set; }
    }


}
