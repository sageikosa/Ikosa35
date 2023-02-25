using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.UI
{
    /// <summary>
    /// Interaction logic for PreConditionRequirementDialog.xaml
    /// </summary>
    public partial class VariableValueEnablerDialog : Window
    {
        public VariableValueEnablerDialog(ValueEnablerEditCommands valueEnabler)
        {
            InitializeComponent();

            // capture original cancel
            var _cancelFinal = valueEnabler.CancelEditCommand;
            var _editFinal = valueEnabler.DoEditCommand;

            // intercept
            valueEnabler.CancelEditCommand = new RelayCommand(() =>
            {
                // call original cancel
                _cancelFinal?.Execute(null);

                // reset it back
                valueEnabler.CancelEditCommand = _cancelFinal;

                // close dialog
                DialogResult = false;
                Close();
            });

            // intercept
            valueEnabler.DoEditCommand = new RelayCommand(() =>
            {
                // call original edit
                _editFinal?.Execute(null);

                // reset it back
                valueEnabler.DoEditCommand = _editFinal;

                // close dialog
                DialogResult = true;
                Close();
            }, () => _editFinal.CanExecute(null));


            DataContext = valueEnabler;
        }
    }
}
