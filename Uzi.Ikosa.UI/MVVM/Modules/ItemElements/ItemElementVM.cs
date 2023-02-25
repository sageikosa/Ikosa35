using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.UI
{
    public class ItemElementVM : ModuleElementVMBase
    {
        private readonly Module _Module;

        public ItemElementVM(Module module, ItemElement itemElement)
            : base(itemElement)
        {
            _Module = module;
            Commands = GetDefaultCommands();
        }

        public Module Module => _Module;
        public ItemElement ItemElement => Element as ItemElement;

        public override Module IkosaModule
            => _Module;

        public string Name
        {
            get => ItemElement.Name;
            set
            {
                ItemElement.Description.Message = value;
                DoPropertyChanged(nameof(Name));
            }
        }

        public override Commandable GetDefaultCommands()
            => new EditItemElementCommands(this);

        public PresentableContext ObjectPresentation
            => ItemElement.ItemBase.GetPresentableObjectVM(IkosaModule.Visuals, null);
    }

    public class EditItemElementCommands : EditCommands
    {
        public EditItemElementCommands(ItemElementVM itemElement)
        {
            ItemElement = itemElement;
            EditCommand = new RelayCommand<object>(target =>
            {
                itemElement.Commands = new DoEditItemElementCommands(itemElement);
            });
        }

        public ItemElementVM ItemElement { get; set; }
    }

    public class DoEditItemElementCommands : DoEditCommands
    {
        public DoEditItemElementCommands(ItemElementVM itemElement)
            : base(itemElement)
        {
            Name = itemElement.ItemElement.Name;
            DoEditCommand = new RelayCommand(() =>
            {
                itemElement.Name = Name;
                itemElement.Commands = itemElement.GetDefaultCommands();
            },
            () => !string.IsNullOrWhiteSpace(Name) && itemElement.IkosaModule.CanUseName(Name, typeof(ItemElement)));
        }
        public string Name { get; set; }
    }
}
