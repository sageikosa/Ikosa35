using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.UI
{
    public class DescriptionVM : ModuleManagementVMBase
    {
        private readonly Description _Model;
        private readonly ObservableCollection<IndexedDescriptionVM> _Descriptions;
        private readonly RelayCommand<object> _Promote;
        private readonly RelayCommand<object> _Demote;
        private IndexedDescriptionVM _Selected;

        public DescriptionVM(Description model)
        {
            _Model = model;
            _Descriptions = new ObservableCollection<IndexedDescriptionVM>(
                model.Descriptions.Select(_d => new IndexedDescriptionVM(this, _d)));

            _Promote = new RelayCommand<object>(
                target =>
                {
                    if (target is IndexedDescriptionVM _vm)
                    {
                        // edit VM
                        var _index = IndexedDescriptions.IndexOf(_vm);
                        _Descriptions.Move(_index, _index - 1);

                        // sync model
                        DoSyncAll();
                    }
                }, target => (target is IndexedDescriptionVM _vm) && _Descriptions.IndexOf(_vm) > 0);
            _Demote = new RelayCommand<object>(
                target =>
                {
                    if (target is IndexedDescriptionVM _vm)
                    {
                        // edit VM
                        var _index = IndexedDescriptions.IndexOf(_vm);
                        _Descriptions.Move(_index, _index + 1);

                        // sync model
                        DoSyncAll();
                    }
                }, target => (target is IndexedDescriptionVM _vm) && _Descriptions.IndexOf(_vm) < (_Descriptions.Count - 1));

            Commands = GetDefaultCommands();
        }

        public Description Description => _Model;
        public ObservableCollection<IndexedDescriptionVM> IndexedDescriptions => _Descriptions;

        public RelayCommand<object> PromoteCommand => _Promote;
        public RelayCommand<object> DemoteCommand => _Demote;

        public IndexedDescriptionVM SelectedDescription
        {
            get => _Selected;
            set
            {
                if (_Selected != value)
                {
                    Commands = GetDefaultCommands();
                }
                _Selected = value;
                DoPropertyChanged(nameof(SelectedDescription));
            }
        }

        public void ReplaceDescriptions(DescriptionVM source)
        {
            _Descriptions.Clear();
            foreach (var _d in source.Description.Descriptions)
            {
                // same strings, different parent
                _Descriptions.Add(new IndexedDescriptionVM(this, _d));
            }
            DoSyncAll();
        }

        public void DoSyncAll()
        {
            // edit model
            try
            {
                // sync array
                Description.Descriptions = _Descriptions.Select(_d => _d.Description).ToArray();
            }
            catch { }
        }

        public void DoSyncDescription(IndexedDescriptionVM infoKeyIndexedDescription)
        {
            var _index = IndexedDescriptions.IndexOf(infoKeyIndexedDescription);
            try
            {
                Description.Descriptions[_index] = infoKeyIndexedDescription.Description;
            }
            catch { }
        }

        public override Commandable GetDefaultCommands()
            => new AddRemoveEditCommands
            {
                Owner = this,
                AddCommand = new RelayCommand<object>((target) =>
                {
                    SelectedDescription = null;
                    Commands = new AddIndexedDescriptionCommands(this);
                }),
                EditCommand = new RelayCommand<object>((target) =>
                {
                    if (target is IndexedDescriptionVM _vm)
                    {
                        Commands = new EditIndexedDescriptionCommands(this, _vm);
                    }
                }, (target) => target is IndexedDescriptionVM),
                RemoveCommand = new RelayCommand<object>((target) =>
                {
                    if (target is IndexedDescriptionVM _vm)
                    {
                        // edit VM
                        var _index = IndexedDescriptions.IndexOf(_vm);
                        _Descriptions.Remove(_vm);

                        // sync model
                        DoSyncAll();
                    }
                }, (target) => target is IndexedDescriptionVM)
            };
    }

    public class AddIndexedDescriptionCommands : DoAddCommands
    {
        public AddIndexedDescriptionCommands(DescriptionVM parent)
            : base(parent)
        {
            DoAddCommand = new RelayCommand(() =>
            {
                // edit vm
                var _ikd = new IndexedDescriptionVM(parent, Description);
                parent.IndexedDescriptions.Add(_ikd);

                // sync model
                parent.DoSyncAll();

                parent.Commands = parent.GetDefaultCommands();
                //                parent.SelectedInfoKey = _vm;
            },
            () => !string.IsNullOrWhiteSpace(Description));
        }
        public string Description { get; set; }
    }

    public class EditIndexedDescriptionCommands : DoEditCommands
    {
        public EditIndexedDescriptionCommands(DescriptionVM parent, IndexedDescriptionVM element)
            : base(parent)
        {
            Description = element.Description;
            DoEditCommand = new RelayCommand(() =>
            {
                element.Description = Description;
                parent.Commands = parent.GetDefaultCommands();
            },
            () => !string.IsNullOrWhiteSpace(Description));
        }

        public string Description { get; set; }
    }
}
