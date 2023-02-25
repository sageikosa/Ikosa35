using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.UI
{
    public class InfoKeyVM : ModuleElementVMBase
    {
        private readonly InfoKeyFolderVM _Folder;

        public InfoKeyVM(InfoKeyFolderVM folder, InfoKey infoKey)
            : base(infoKey)
        {
            _Folder = folder;
            Commands = GetDefaultCommands();
        }

        public InfoKeyFolderVM Folder => _Folder;
        public InfoKey InfoKey => Element as InfoKey;

        public override Module IkosaModule
            => Folder.IkosaModule;

        public string Name
        {
            get => InfoKey.Name;
            set
            {
                InfoKey.Description.Message = value;
                DoPropertyChanged(nameof(Name));
            }
        }

        public override Commandable GetDefaultCommands()
            => new EditInfoKeyCommands(this);
    }

    public class EditInfoKeyCommands : EditCommands
    {
        public EditInfoKeyCommands(InfoKeyVM infoKey)
        {
            InfoKey = infoKey;
            EditCommand = new RelayCommand<object>((target) =>
            {
                infoKey.Commands = new DoEditInfoKeyCommands(infoKey);
            });
        }

        public InfoKeyVM InfoKey { get; set; }
    }

    public class DoEditInfoKeyCommands : DoEditCommands
    {
        public DoEditInfoKeyCommands(InfoKeyVM infoKey)
            : base(infoKey)
        {
            Name = infoKey.InfoKey.Name;
            DoEditCommand = new RelayCommand(() =>
            {
                infoKey.Name = Name;
                infoKey.Commands = infoKey.GetDefaultCommands();
            },
            () => !string.IsNullOrWhiteSpace(Name) && infoKey.Folder.IkosaModule.CanUseName(Name, typeof(InfoKey)));
        }

        public string Name { get; set; }
    }
}
