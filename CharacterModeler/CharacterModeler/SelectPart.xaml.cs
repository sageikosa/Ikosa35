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
using Uzi.Packaging;

namespace CharacterModeler
{
    /// <summary>
    /// Interaction logic for SelectPart.xaml
    /// </summary>
    public partial class SelectPart : Window
    {
        public static RoutedCommand OKCommand = new RoutedCommand();

        public SelectPart()
        {
            InitializeComponent();
        }

        public SelectPart(CorePackage package, Func<object, bool> partFilter)
        {
            InitializeComponent();
            _Filter = partFilter;
            this.DataContext = package;
        }

        private Func<object, bool> _Filter;

        public IBasePart BasePart { get; set; }

        private void cbOKCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (tvwParts != null) && (tvwParts.SelectedItem is IBasePart)
                && ((_Filter == null) || _Filter(tvwParts.SelectedItem));
            e.Handled = true;
        }

        private void cbOKCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            BasePart = tvwParts.SelectedItem as IBasePart;
            DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            e.Handled = true;
        }
    }
}
