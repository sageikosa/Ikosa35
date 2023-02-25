using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.UI
{
    public class VariableValueFolderVM : ModuleManagementVMBase
    {
        private readonly VariableVM _Variable;
        private readonly ObservableCollection<VariableValueVM> _Values;
        private VariableValueVM _Selected;

        public VariableValueFolderVM(VariableVM variable)
        {
            _Variable = variable;
            _Values = new ObservableCollection<VariableValueVM>(
                variable.Variable.Values.Select(_v => new VariableValueVM(this, _v)));
            Commands = GetDefaultCommands();
        }

        public VariableVM Variable => _Variable;
        public ObservableCollection<VariableValueVM> Values => _Values;

        public VariableValueVM SelectedValue
        {
            get => _Selected;
            set
            {
                if (_Selected != value)
                {
                    _Selected = value;
                    DoPropertyChanged(nameof(SelectedValue));
                }
            }
        }

        public override Commandable GetDefaultCommands()
            => new AddRemoveEditCommands
            {
                AddCommand = new RelayCommand<object>(target =>
                {
                    Commands = new AddVariableValueCommands(this);
                }),
                RemoveCommand = new RelayCommand<object>(target =>
                {
                    if (target is VariableValueVM _vm)
                    {
                        Values.Remove(_vm);
                        Variable.Variable.Remove(_vm.VariableValue.ID);
                        if (SelectedValue == _vm)
                        {
                            SelectedValue = null;
                        }
                    }
                }, target => target is VariableValueVM),
                EditCommand = new RelayCommand<object>(target =>
                {
                    Variable.SetValuesEdit();
                    Commands = new CancelCommands
                    {
                        CancelCommand = new RelayCommand(() =>
                        {
                            Variable.SetDescriptionEdit();
                            Commands = GetDefaultCommands();
                        }),
                        Owner = this
                    };
                }, target => target is VariableValueVM),
                Owner = this
            };
    }

    public class AddVariableValueCommands : DoAddCommands
    {
        public AddVariableValueCommands(VariableValueFolderVM parent)
            : base(parent)
        {
            DoAddCommand = new RelayCommand(() =>
            {
                var _vm = new VariableValueVM(parent, parent.Variable.Variable.Add(new Description(Name)));
                parent.Values.Add(_vm);
                parent.Commands = parent.GetDefaultCommands();
                parent.SelectedValue = _vm;
            },
            () => !string.IsNullOrWhiteSpace(Name) && !parent.Variable.Variable.Values.Any(_v => _v.Description.Message == Name));
        }

        public string Name { get; set; }
    }
}
