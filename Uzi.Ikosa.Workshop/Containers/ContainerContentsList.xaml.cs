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
using Uzi.Ikosa.Objects;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>Interaction logic for ContainerContentsList.xaml</summary>
    public partial class ContainerContentsList : Window
    {
        public ContainerContentsList()
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
            if (DataContext is ContainerItemBase _cItem)
            {
                _cItem.Container.Remove(lstItems.SelectedItem as ICoreObject);
            }
            else
            {
                if (DataContext is SlottedContainerItemBase _scItem)
                {
                    _scItem.Container.Remove(lstItems.SelectedItem as ICoreObject);
                }
            }
        }
    }
}
