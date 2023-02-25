using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Uzi.Core;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ObjectCreate.xaml
    /// </summary>
    public partial class ObjectCreate : Window
    {
        public ObjectCreate()
        {
            InitializeComponent();
        }

        private string _TagString;

        public CoreObject GetCoreObject()
        {
            var _name = txtName.Text;
            // TODO: defaultModelKey
            if (_TagString.Equals(@"Portal", StringComparison.OrdinalIgnoreCase))
            {
                return createPortal.GetPortal(_name);
            }
            else if (_TagString.Equals(@"Container", StringComparison.OrdinalIgnoreCase))
            {
                return createContainer.GetContainer(_name);
            }
            else if (_TagString.Equals(@"Light", StringComparison.OrdinalIgnoreCase))
            {
                return createLight.GetLight(_name);
            }
            else if (_TagString.Equals(@"Creature", StringComparison.OrdinalIgnoreCase))
            {
                return createCreature.GetCreature(_name);
            }
            return null;
        }

        private void cbOKCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((txtName != null)
                && (txtName.Text.Trim() != string.Empty))
            {
                // TODO: defaultModelKey
                if (tiCreature.IsSelected)
                    e.CanExecute = createCreature.CanCreate;
                else if (tiPortal.IsSelected)
                    e.CanExecute = createPortal.CanCreate;
                else if (tiContainer.IsSelected)
                    e.CanExecute = createContainer.CanCreate;
                else if (tiLight.IsSelected)
                    e.CanExecute = createLight.CanCreate;
            }
            e.Handled = true;
        }

        private void cbOKCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (tiCreature.IsSelected)
                _TagString = @"Creature";
            else if (tiPortal.IsSelected)
                _TagString = @"Portal";
            else if (tiContainer.IsSelected)
                _TagString = @"Container";
            else if (tiLight.IsSelected)
                _TagString = @"Light";

            // all of the above
            DialogResult = true;
            this.Close();
            e.Handled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
            e.Handled = true;
        }
    }
}
