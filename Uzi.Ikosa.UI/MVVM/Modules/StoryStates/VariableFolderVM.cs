using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Guildsmanship;
using Uzi.Packaging;

namespace Uzi.Ikosa.UI
{
    public class VariableFolderVM : ModuleManagementVMBase
    {
        private readonly Module _Module;
        private readonly PartsFolder _Folder;
        private readonly Window _Window;
        private readonly ObservableCollection<VariableVM> _Contents;
        private VariableVM _Selected;

        public VariableFolderVM(Module module, PartsFolder folder, Window window)
        {
            _Module = module;
            _Folder = folder;
            _Window = window;
            _Contents = new ObservableCollection<VariableVM>(
                _Folder.FolderContents.OfType<Variable>().Select(_ss => new VariableVM(this, _ss)));
            Commands = GetDefaultCommands();
        }

        public Module IkosaModule => _Module;
        public PartsFolder PartsFolder => _Folder;
        public Window Window => _Window;

        public ObservableCollection<VariableVM> Variables => _Contents;

        public VariableVM SelectedVariable
        {
            get => _Selected;
            set
            {
                if (_Selected != null)
                {
                    _Selected.Commands = _Selected.GetDefaultCommands();
                }
                _Selected = value;
                DoPropertyChanged(nameof(SelectedVariable));
            }
        }

        public void DoSignalVisibility()
        {
            DoPropertyChanged(nameof(DescriptionEdit));
            DoPropertyChanged(nameof(ValuesEdit));
        }

        public Visibility DescriptionEdit => SelectedVariable?.DescriptionEdit ?? Visibility.Visible;
        public Visibility ValuesEdit => SelectedVariable?.ValuesEdit ?? Visibility.Collapsed;

        public override Commandable GetDefaultCommands()
            => new AddRemoveCommands
            {
                AddCommand = new RelayCommand<object>((target) =>
                {
                    Commands = new AddVariableCommands(this);
                }),
                RemoveCommand = new RelayCommand<object>((target) =>
                {
                    if (target is VariableVM _ss)
                    {
                        IkosaModule.Variables.Remove(_ss.Variable.ID);
                        _Contents.Remove(_ss);
                        if (SelectedVariable == _ss)
                        {
                            SelectedVariable = null;
                        }
                        PartsFolder.ContentsChanged();
                    }
                }, target => target is VariableVM)
            };
    }

    public class AddVariableCommands : DoAddCommands
    {
        public AddVariableCommands(VariableFolderVM parent)
            : base(parent)
        {
            DoAddCommand = new RelayCommand(() =>
            {
                var _ss = new Variable(new Description(Name));
                parent.IkosaModule.Variables.Add(_ss.ID, _ss);
                var _vm = new VariableVM(parent, _ss);
                parent.Variables.Add(_vm);
                parent.Commands = parent.GetDefaultCommands();
                parent.SelectedVariable = _vm;
                parent.PartsFolder.ContentsChanged();
            },
            () => !string.IsNullOrWhiteSpace(Name) && parent.IkosaModule.CanUseName(Name, typeof(Variable)));
        }
        public string Name { get; set; }
    }
}
