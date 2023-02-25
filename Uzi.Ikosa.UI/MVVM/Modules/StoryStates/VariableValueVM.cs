using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.UI
{
    public class VariableValueVM : ModuleElementVMBase
    {
        private readonly VariableValueFolderVM _Folder;
        private readonly ValueEnablersFolderVM _Enablers;

        public VariableValueVM(VariableValueFolderVM folder, VariableValue value)
            : base(value)
        {
            _Folder = folder;
            _Enablers = new ValueEnablersFolderVM(this);
            Commands = GetDefaultCommands();
        }

        public VariableValueFolderVM Folder => _Folder;
        public VariableValue VariableValue => Element as VariableValue;
        public ValueEnablersFolderVM ValueEnablers => _Enablers;

        public override Module IkosaModule
            => Folder.Variable.Folder.IkosaModule;

        public string Name
        {
            get => VariableValue.Description.Message;
            set
            {
                VariableValue.Description.Message = value;
                DoPropertyChanged(nameof(Name));
            }
        }

        public bool RequiresAllEnablers
        {
            get => VariableValue.Validation == VariableValueEnablerValidation.All;
            set
            {
                VariableValue.Validation = value ? VariableValueEnablerValidation.All : VariableValueEnablerValidation.Any;
                DoPropertyChanged(nameof(RequiresAllEnablers));
            }
        }

        public override Commandable GetDefaultCommands()
            => new EditVariableValueCommands(this);
    }

    public class EditVariableValueCommands : EditCommands
    {
        public EditVariableValueCommands(VariableValueVM variableValue)
        {
            VariableValue = variableValue;
            EditCommand = new RelayCommand<object>(target =>
            {
                variableValue.Commands = new DoEditVariableValueCommands(variableValue);
            });
        }

        public VariableValueVM VariableValue { get; set; }
    }

    public class DoEditVariableValueCommands : DoEditCommands
    {
        public DoEditVariableValueCommands(VariableValueVM variableValue)
            : base(variableValue)
        {
            Name = variableValue.Name;
            RequiresAllEnablers = variableValue.RequiresAllEnablers;
            DoEditCommand = new RelayCommand(() =>
            {
                variableValue.Name = Name;
                variableValue.RequiresAllEnablers = RequiresAllEnablers;
                variableValue.Commands = variableValue.GetDefaultCommands();
            },
            () => (!string.IsNullOrWhiteSpace(Name) && !variableValue.Folder.Values.Any(_v => _v.Name == Name))
            || (variableValue.RequiresAllEnablers != RequiresAllEnablers));
        }

        public string Name { get; set; }
        public bool RequiresAllEnablers { get; set; }
    }
}
