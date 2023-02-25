using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.UI
{
    public class ValueEnablersFolderVM : ModuleManagementVMBase
    {
        private readonly VariableValueVM _VariableValue;
        private readonly ObservableCollection<ValueEnablerVM> _ValueEnablers;
        private ValueEnablerVM _Selected;

        public ValueEnablersFolderVM(VariableValueVM variableValue)
        {
            _VariableValue = variableValue;
            _ValueEnablers = new ObservableCollection<ValueEnablerVM>(
                variableValue.VariableValue.ValueEnablers
                .Select(_pc => ValueEnablerVM.FromPreCondition(this, _pc))
                .Where(_pcr => _pcr != null)
                );
            Commands = GetDefaultCommands();
        }

        public VariableValueVM VariableValue => _VariableValue;
        public ObservableCollection<ValueEnablerVM> ValueEnablers => _ValueEnablers;

        public ValueEnablerVM SelectedPreCondition
        {
            get => _Selected;
            set
            {
                if (_Selected != value)
                {
                    Commands = GetDefaultCommands();
                }
                _Selected = value;
                DoPropertyChanged(nameof(SelectedPreCondition));
            }
        }

        public override Commandable GetDefaultCommands()
            => new AddRemoveEditCommands
            {
                Owner = this,
                AddCommand = new RelayCommand<object>(target =>
                {
                    Commands = new AddPreConditionRequirementCommands(this);
                }),
                RemoveCommand = new RelayCommand<object>(target =>
                {
                    if (target is ValueEnablerVM _vm)
                    {
                        ValueEnablers.Remove(_vm);
                        VariableValue.VariableValue.ValueEnablers.Remove(_vm.Enabler);
                        if (SelectedPreCondition == _vm)
                        {
                            SelectedPreCondition = null;
                        }
                    }
                }, target => target is ValueEnablerVM),
                EditCommand = new RelayCommand<object>(target =>
                {
                    if (target is ValueEnablerVM _vm)
                    {
                        var _dlg = new VariableValueEnablerDialog(_vm.Commands as ValueEnablerEditCommands)
                        {
                            Owner = VariableValue.Folder.Variable.Folder.Window,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };
                        _dlg.ShowDialog();
                    }
                }, target => target is ValueEnablerVM)
            };
    }

    public enum PreConditionType
    {
        InfoKey,
        Variable,
        Item
    }

    public class AddPreConditionRequirementCommands : DoAddCommands
    {
        public AddPreConditionRequirementCommands(ValueEnablersFolderVM parent)
            : base(parent)
        {
            PreConditionRequirement = PreConditionType.InfoKey;
            DoAddCommand = new RelayCommand(() =>
            {
                switch (PreConditionRequirement)
                {
                    case PreConditionType.InfoKey:
                        {
                            var _ikr = new InfoKeyValueEnabler(new Description(Name), Guid.Empty);
                            parent.VariableValue.VariableValue.ValueEnablers.Add(_ikr);
                            var _vm = ValueEnablerVM.FromPreCondition(parent, _ikr);
                            parent.ValueEnablers.Add(_vm);
                            parent.SelectedPreCondition = _vm;
                        }
                        break;

                    case PreConditionType.Variable:
                        {
                            var _ssr = new VariableValueEnabler(new Description(Name));
                            parent.VariableValue.VariableValue.ValueEnablers.Add(_ssr);
                            var _vm = ValueEnablerVM.FromPreCondition(parent, _ssr);
                            parent.ValueEnablers.Add(_vm);
                            parent.SelectedPreCondition = _vm;
                        }
                        break;

                    case PreConditionType.Item:
                        {
                            var _itmr = new ItemValueEnabler(new Description(Name));
                            parent.VariableValue.VariableValue.ValueEnablers.Add(_itmr);
                            var _vm = ValueEnablerVM.FromPreCondition(parent, _itmr);
                            parent.ValueEnablers.Add(_vm);
                            parent.SelectedPreCondition = _vm;
                        }
                        break;

                    default:
                        break;
                }
                parent.Commands = parent.GetDefaultCommands();
            },
            () => !string.IsNullOrWhiteSpace(Name) && !parent.VariableValue.VariableValue.ValueEnablers.Any(_p => _p.Description.Message == Name));
        }

        public string Name { get; set; }
        public PreConditionType PreConditionRequirement { get; set; }
    }

    public class EditPreConditionRequirementCommands : DoEditCommands
    {
        public EditPreConditionRequirementCommands(ValueEnablersFolderVM parent, ValueEnablerVM preCondition)
            : base(parent)
        {
            Name = preCondition.Name;
            DoEditCommand = new RelayCommand(() =>
            {
                preCondition.Name = Name;
                parent.Commands = parent.GetDefaultCommands();
            },
            () => !string.IsNullOrWhiteSpace(Name) && !parent.ValueEnablers.Any(_v => _v.Name == Name));
        }

        public string Name { get; set; }
    }
}
