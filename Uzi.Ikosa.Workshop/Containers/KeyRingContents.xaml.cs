using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for KeyRingContents.xaml
    /// </summary>
    public partial class KeyRingContents : Window
    {
        public KeyRingContents()
        {
            InitializeComponent();
        }

        private void cmdbndRemove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lstItems != null) && (lstItems.SelectedItem != null);
            e.Handled = true;
        }

        private void cmdbndRemove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is KeyRing _cItem)
            {
                _cItem.Remove(lstItems.SelectedItem as IKeyRingMountable);
            }
        }
    }
}
