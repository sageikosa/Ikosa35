using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.UI
{
    public class NamedKeysPartVM : ModuleManagementVMBase
    {
        private readonly NamedKeysPart _NamedKeys;
        private readonly ObservableCollection<NamedKeyVM> _Contents;
        private NamedKeyVM _Selected;

        public NamedKeysPartVM(NamedKeysPart namedKeys)
        {
            _NamedKeys = namedKeys;
            _Contents = new ObservableCollection<NamedKeyVM>(
                _NamedKeys.Module.NamedKeyGuids
                .OrderBy(_kvp => _kvp.Value)
                .Select(_kvp => new NamedKeyVM(this, _kvp.Key, _kvp.Value)));
            Commands = GetDefaultCommands();
        }

        public NamedKeysPart Part => _NamedKeys;

        public ObservableCollection<NamedKeyVM> NamedKeys => _Contents;

        public NamedKeyVM SelectedNamedKey
        {
            get => _Selected;
            set
            {
                if (_Selected != null)
                {
                    _Selected.Commands = _Selected.GetDefaultCommands();
                }

                _Selected = value;
                DoPropertyChanged(nameof(SelectedNamedKey));
            }
        }

        public override Commandable GetDefaultCommands()
            => new AddRemoveCommands
            {
                AddCommand = new RelayCommand<object>(target =>
                {
                    Commands = new AddNamedKeyCommands(this);
                }),
                RemoveCommand = new RelayCommand<object>(target =>
                {
                    if (target is NamedKeyVM _nk)
                    {
                        _ = Part.Module.NamedKeyGuids.Remove(_nk.KeyID);
                        _ = NamedKeys.Remove(_nk);
                        if (SelectedNamedKey == _nk)
                        {
                            SelectedNamedKey = null;
                        }
                        // TODO: other notifications?
                    }
                })
            };
    }

    public class AddNamedKeyCommands : DoAddCommands
    {
        public AddNamedKeyCommands(NamedKeysPartVM parent)
            : base(parent)
        {
            Owner = parent;
            DoAddCommand = new RelayCommand(() =>
            {
                var _nk = new NamedKeyVM(parent, Guid.NewGuid(), Name);
                parent.Part.Module.NamedKeyGuids[_nk.KeyID] = _nk.Name;
                parent.NamedKeys.Add(_nk);
                parent.Commands = parent.GetDefaultCommands();
                parent.SelectedNamedKey = _nk;
                // TODO: other notifications
            }, () => !string.IsNullOrWhiteSpace(Name));
        }
        public string Name { get; set; }
    }
}
