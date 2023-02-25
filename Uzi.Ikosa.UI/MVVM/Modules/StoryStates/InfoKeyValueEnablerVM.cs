using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.UI
{
    public class InfoKeyValueEnablerVM : ValueEnablerVM
    {
        private ModuleInfoKeyVM _Current;

        public InfoKeyValueEnablerVM(ValueEnablersFolderVM folder, InfoKeyValueEnabler requirement)
            : base(folder, requirement)
        {
            var _current = IkosaModule?.GetInfoKey(InfoKeyEnabler.InfoKeyID) ?? default;
            _Current = new ModuleInfoKeyVM { Module = _current.module, InfoKey = _current.infoKey };
            Commands = GetDefaultCommands();
        }

        public InfoKeyValueEnabler InfoKeyEnabler => Enabler as InfoKeyValueEnabler;

        public ModuleInfoKeyVM RequiredInfoKey
        {
            get => _Current;
            set
            {
                _Current = value;
                InfoKeyEnabler.InfoKeyID = value.InfoKey.ID;
                DoPropertyChanged(nameof(RequiredInfoKey));
            }
        }

        public override Commandable GetDefaultCommands()
            => new EditInfoKeyValueEnablerCommands(this);
    }

    public class ModuleInfoKeyVM
    {
        public Module Module { get; set; }
        public InfoKey InfoKey { get; set; }
    }

    public class EditInfoKeyValueEnablerCommands : ValueEnablerEditCommands
    {
        private readonly List<Module> _Modules;
        private Module _SelectedModule;
        private readonly ObservableCollection<InfoKey> _InfoKeys;
        private InfoKey _SelectedInfoKey;

        public EditInfoKeyValueEnablerCommands(InfoKeyValueEnablerVM infoKeyValueEnabler)
            : base(infoKeyValueEnabler)
        {
            // module
            _Modules = infoKeyValueEnabler.IkosaModule.ToEnumerable().Concat(
                infoKeyValueEnabler.IkosaModule.Resources.Imports.Modules
                .Select(_m => _m.Module)
                .Where(_m => _m != null))
                .ToList();
            _SelectedModule = infoKeyValueEnabler.RequiredInfoKey?.Module ?? _Modules.FirstOrDefault();

            // info key
            _InfoKeys = new ObservableCollection<InfoKey>(_SelectedModule.InfoKeys.Select(_kvp => _kvp.Value));
            _SelectedInfoKey = infoKeyValueEnabler.RequiredInfoKey?.InfoKey ?? _InfoKeys.FirstOrDefault();

            // command
            DoEditCommand = new RelayCommand(() =>
            {
                infoKeyValueEnabler.Name = Name;
                infoKeyValueEnabler.RequiredInfoKey = new ModuleInfoKeyVM { Module = SelectedModule, InfoKey = SelectedInfoKey };
                UpdateDescription();

                infoKeyValueEnabler.Commands = infoKeyValueEnabler.GetDefaultCommands();
            }, () => !string.IsNullOrWhiteSpace(Name) && (_SelectedInfoKey != null));
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
                    InfoKeys.Clear();
                    foreach (var _ik in _SelectedModule.InfoKeys.Select(_kvp => _kvp.Value))
                    {
                        InfoKeys.Add(_ik);
                    }
                    SelectedInfoKey = InfoKeys.FirstOrDefault();
                    DoPropertyChanged(nameof(SelectedModule));
                }
            }
        }

        public ObservableCollection<InfoKey> InfoKeys => _InfoKeys;

        public InfoKey SelectedInfoKey
        {
            get => _SelectedInfoKey;
            set
            {
                if (value != _SelectedInfoKey)
                {
                    _SelectedInfoKey = value;
                    DoPropertyChanged(nameof(SelectedInfoKey));
                }
            }
        }
    }
}
