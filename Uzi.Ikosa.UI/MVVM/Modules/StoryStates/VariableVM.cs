using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.UI
{
    public class VariableVM : ModuleElementVMBase
    {
        private readonly VariableFolderVM _Folder;
        private Visibility _DescriptionEdit;
        private Visibility _ValuesEdit;
        private readonly VariableValueFolderVM _Values;

        public VariableVM(VariableFolderVM folder, Variable variable)
            : base(variable)
        {
            _Folder = folder;
            _DescriptionEdit = Visibility.Visible;
            _ValuesEdit = Visibility.Collapsed;
            _Values = new VariableValueFolderVM(this);
            Commands = GetDefaultCommands();
        }

        public VariableFolderVM Folder => _Folder;
        public Variable Variable => Element as Variable;
        public Visibility DescriptionLayout => _DescriptionEdit == Visibility.Collapsed ? Visibility.Collapsed : Visibility.Hidden;
        public Visibility DescriptionEdit => _DescriptionEdit;
        public Visibility ValuesEdit => _ValuesEdit;

        public VariableValueFolderVM Values => _Values;

        public override Module IkosaModule
            => Folder.IkosaModule;

        public void SetDescriptionEdit()
        {
            _DescriptionEdit = Visibility.Visible;
            _ValuesEdit = Visibility.Collapsed;
            DoPropertyChanged(nameof(DescriptionLayout));
            DoPropertyChanged(nameof(DescriptionEdit));
            DoPropertyChanged(nameof(ValuesEdit));
            Folder.DoSignalVisibility();
        }

        public void SetValuesEdit()
        {
            _DescriptionEdit = Visibility.Collapsed;
            _ValuesEdit = Visibility.Visible;
            DoPropertyChanged(nameof(DescriptionLayout));
            DoPropertyChanged(nameof(DescriptionEdit));
            DoPropertyChanged(nameof(ValuesEdit));
            Folder.DoSignalVisibility();
        }

        public string Name
        {
            get => Variable.Name;
            set
            {
                Variable.Description.Message = value;
                DoPropertyChanged(nameof(Name));
            }
        }

        public override Commandable GetDefaultCommands()
            => new EditVariableCommands(this);
    }


    public class EditVariableCommands : EditCommands
    {
        public EditVariableCommands(VariableVM variable)
        {
            Variable = variable;
            EditCommand = new RelayCommand<object>((target) =>
            {
                variable.Commands = new DoEditVariableCommands(variable);
            });
        }

        public VariableVM Variable { get; set; }
    }

    public class DoEditVariableCommands : DoEditCommands
    {
        public DoEditVariableCommands(VariableVM variable)
            : base(variable)
        {
            Name = variable.Variable.Name;
            DoEditCommand = new RelayCommand(() =>
            {
                variable.Name = Name;
                variable.Commands = variable.GetDefaultCommands();
            },
            () => !string.IsNullOrWhiteSpace(Name) && variable.Folder.IkosaModule.CanUseName(Name, typeof(Variable)));
        }

        public string Name { get; set; }
    }
}
