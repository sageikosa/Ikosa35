using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.UI
{
    public class VariableValueEnablerVM : ValueEnablerVM
    {
        private ModuleVariableValueVM _Current;

        public VariableValueEnablerVM(ValueEnablersFolderVM folder, VariableValueEnabler requirement)
            : base(folder, requirement)
        {
            var (_module, _, _variable, _, _choice) = requirement.RequiredState(IkosaModule);
            _Current = new ModuleVariableValueVM
            {
                Module = _module,
                Variable = _variable,
                VariableValue = _choice
            };
            Commands = GetDefaultCommands();
        }

        public VariableValueEnabler VariableValueEnabler => Enabler as VariableValueEnabler;

        public ModuleVariableValueVM RequiredVariableValue
        {
            get => _Current;
            set
            {
                _Current = value;
                VariableValueEnabler.VariableID = _Current.Variable.ID;
                VariableValueEnabler.ValueID = _Current.VariableValue.ID;
                DoPropertyChanged(nameof(RequiredVariableValue));
            }
        }

        public override Commandable GetDefaultCommands()
            => new EditVariableValueEnablerCommands(this);
    }

    public class ModuleVariableValueVM
    {
        public Module Module { get; set; }
        public Variable Variable { get; set; }
        public VariableValue VariableValue { get; set; }
    }

    public class EditVariableValueEnablerCommands : ValueEnablerEditCommands
    {
        private readonly List<Module> _Modules;
        private Module _SelectedModule;
        private readonly ObservableCollection<Variable> _Variables;
        private Variable _SelectedVariable;
        private readonly ObservableCollection<VariableValue> _Values;
        private VariableValue _SelectedValue;

        public EditVariableValueEnablerCommands(VariableValueEnablerVM variableValueEnabler)
            : base(variableValueEnabler)
        {
            // module
            _Modules = variableValueEnabler.IkosaModule.ToEnumerable().Concat(
                variableValueEnabler.IkosaModule.Resources.Imports.Modules
                .Select(_m => _m.Module)
                .Where(_m => _m != null))
                .ToList();
            _SelectedModule = variableValueEnabler.RequiredVariableValue?.Module ?? _Modules.FirstOrDefault();

            // state
            _Variables = new ObservableCollection<Variable>(_SelectedModule.Variables.Select(_kvp => _kvp.Value)
                .Where(_v => _v.ID != variableValueEnabler.Folder.VariableValue.VariableValue.ID));
            _SelectedVariable = variableValueEnabler.RequiredVariableValue?.Variable ?? _Variables.FirstOrDefault();

            // values
            _Values = new ObservableCollection<VariableValue>(_SelectedVariable.Values);
            _SelectedValue = variableValueEnabler.RequiredVariableValue?.VariableValue ?? _Values.FirstOrDefault();

            // commands
            DoEditCommand = new RelayCommand(() =>
            {
                variableValueEnabler.Name = Name;
                variableValueEnabler.RequiredVariableValue = new ModuleVariableValueVM
                {
                    Module = SelectedModule,
                    Variable = SelectedVariable,
                    VariableValue = SelectedValue
                };
                UpdateDescription();
            }, () => !string.IsNullOrWhiteSpace(Name));   // TODO: && NULL selection validations...
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
                    Variables.Clear();
                    foreach (var _ik in _SelectedModule.Variables.Select(_kvp => _kvp.Value))
                    {
                        Variables.Add(_ik);
                    }
                    SelectedVariable = Variables.FirstOrDefault();

                    DoPropertyChanged(nameof(SelectedModule));
                }
            }
        }

        public ObservableCollection<Variable> Variables => _Variables;

        public Variable SelectedVariable
        {
            get => _SelectedVariable;
            set
            {
                if (_SelectedVariable != value)
                {
                    _SelectedVariable = value;
                    Values.Clear();
                    foreach (var _ssv in _SelectedVariable?.Values ?? new VariableValue[] { })
                    {
                        Values.Add(_ssv);
                    }
                    SelectedValue = Values.FirstOrDefault();
                }
                DoPropertyChanged(nameof(SelectedVariable));
            }
        }

        public ObservableCollection<VariableValue> Values => _Values;

        public VariableValue SelectedValue
        {
            get => _SelectedValue;
            set
            {
                if (value != _SelectedValue)
                {
                    _SelectedValue = value;
                    DoPropertyChanged(nameof(SelectedValue));
                }
            }
        }
    }
}
