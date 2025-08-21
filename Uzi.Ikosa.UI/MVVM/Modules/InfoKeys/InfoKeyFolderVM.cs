using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Guildsmanship;
using Uzi.Packaging;

namespace Uzi.Ikosa.UI
{
    public class InfoKeyFolderVM : ModuleManagementVMBase
    {
        private readonly Module _Module;
        private readonly PartsFolder _Folder;
        private readonly ObservableCollection<InfoKeyVM> _Contents;
        private InfoKeyVM _Selected;

        public InfoKeyFolderVM(Module module, PartsFolder folder)
        {
            _Module = module;
            _Folder = folder;
            _Contents = new ObservableCollection<InfoKeyVM>(
                _Folder.FolderContents.OfType<InfoKey>().Select(_ik => new InfoKeyVM(this, _ik)));
            Commands = GetDefaultCommands();
        }
        public Module IkosaModule => _Module;
        public PartsFolder PartsFolder => _Folder;

        public ObservableCollection<InfoKeyVM> InfoKeys => _Contents;

        public InfoKeyVM SelectedInfoKey
        {
            get => _Selected;
            set
            {
                if (_Selected != null)
                {
                    _Selected.Commands = _Selected.GetDefaultCommands();
                }

                _Selected = value;
                DoPropertyChanged(nameof(SelectedInfoKey));
            }
        }

        public override Commandable GetDefaultCommands()
            => new AddRemoveCommands
            {
                AddCommand = new RelayCommand<object>((target) =>
                {
                    Commands = new AddInfoKeyCommands(this);
                }),
                RemoveCommand = new RelayCommand<object>((target) =>
                {
                    if (target is InfoKeyVM _ik)
                    {
                        IkosaModule.InfoKeys.Remove(_ik.InfoKey.ID);
                        _Contents.Remove(_ik);
                        if (SelectedInfoKey == _ik)
                        {
                            SelectedInfoKey = null;
                        }
                        PartsFolder.ContentsChanged();
                    }
                })
            };

    }

    public class AddInfoKeyCommands : DoAddCommands
    {
        public AddInfoKeyCommands(InfoKeyFolderVM parent)
            : base(parent)
        {
            DoAddCommand = new RelayCommand(() =>
            {
                var _ik = new InfoKey(new Description(Description));
                parent.IkosaModule.InfoKeys.Add(_ik.ID, _ik);
                var _vm = new InfoKeyVM(parent, _ik);
                parent.InfoKeys.Add(_vm);
                parent.Commands = parent.GetDefaultCommands();
                parent.SelectedInfoKey = _vm;
                parent.PartsFolder.ContentsChanged();
            },
            () => !string.IsNullOrWhiteSpace(Description) && parent.IkosaModule.CanUseName(Description, typeof(InfoKey)));
        }
        public string Description { get; set; }
    }
}
