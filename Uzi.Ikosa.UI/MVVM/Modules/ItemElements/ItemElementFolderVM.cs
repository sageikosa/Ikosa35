using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Guildsmanship;
using Uzi.Ikosa.Items;
using Uzi.Packaging;

namespace Uzi.Ikosa.UI
{
    public class ItemElementFolderVM : ModuleManagementVMBase
    {
        private readonly Module _Module;
        private readonly PartsFolder _Folder;
        private readonly ObservableCollection<ItemElementVM> _Contents;
        private ItemElementVM _Selected;

        public ItemElementFolderVM(Module module, PartsFolder folder)
        {
            _Module = module;
            _Folder = folder;
            _Contents = new ObservableCollection<ItemElementVM>(
                _Folder.FolderContents.OfType<ItemElement>().Select(_ie => new ItemElementVM(_Module, _ie)));
            Commands = GetDefaultCommands();
        }

        public Module IkosaModule => _Module;
        public PartsFolder PartsFolder => _Folder;

        public void AddItem(IItemBase item)
        {
            var _ie = new ItemElement(item, new Description(item.Name));
            IkosaModule.ItemElements.Add(_ie.ID, _ie);
            var _vm = new ItemElementVM(IkosaModule, _ie);
            ItemElements.Add(_vm);
            SelectedItemElement = _vm;
            PartsFolder.ContentsChanged();
        }

        public ObservableCollection<ItemElementVM> ItemElements => _Contents;

        public ItemElementVM SelectedItemElement
        {
            get => _Selected;
            set
            {
                if (_Selected != null)
                {
                    _Selected.Commands = _Selected.GetDefaultCommands();
                }

                _Selected = value;
                DoPropertyChanged(nameof(SelectedItemElement));
            }
        }

        public override Commandable GetDefaultCommands()
            => new AddItemElementCommands(this);
    }

    public class AddItemElementCommands : DoAddCommands
    {
        public AddItemElementCommands(ItemElementFolderVM parent)
            : base(parent)
        {
            Owner = parent;
            DoAddCommand = new RelayCommand(() =>
            {
                //var _ik = new InfoKey(new Description(Description));
                //parent.IkosaModule.InfoKeys.Add(_ik.ID, _ik);
                //var _vm = new InfoKeyVM(parent, _ik);
                //parent.InfoKeys.Add(_vm);
                parent.Commands = parent.GetDefaultCommands();
                //parent.SelectedInfoKey = _vm;
                parent.PartsFolder.ContentsChanged();
            },
            () => !string.IsNullOrWhiteSpace(Description) && parent.IkosaModule.CanUseName(Description, typeof(ItemElement)));
            RemoveCommand = new RelayCommand<object>((target) =>
            {
                if (target is ItemElementVM _ie)
                {
                    _ = parent.IkosaModule.ItemElements.Remove(_ie.ItemElement.ID);
                    _ = parent.ItemElements.Remove(_ie);
                    if (parent.SelectedItemElement == _ie)
                    {
                        parent.SelectedItemElement = null;
                    }
                    parent.PartsFolder.ContentsChanged();
                }
            });
        }

        public string Description { get; set; }
        public ItemElementFolderVM Parent => Owner as ItemElementFolderVM;
        public RelayCommand<object> RemoveCommand { get; set; }
    }
}
