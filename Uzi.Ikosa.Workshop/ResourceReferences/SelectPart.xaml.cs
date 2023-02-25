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
using Uzi.Ikosa.Guildsmanship;
using Uzi.Packaging;
using Uzi.Visualize;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.Workshop
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

        public SelectPart(CorePackage package, Func<IBasePart, bool> canReference)
        {
            InitializeComponent();
            DataContext = package;
            _CanReference = canReference;
        }

        public static bool UnboundVisualResources(IBasePart part)
            => (part is VisualResources)
            && ((part.NameManager is CorePackage) || (part.NameManager is CorePackagePartsFolder));

        public static bool AnyModule(IBasePart part)
            => part is Module;

        private readonly Func<IBasePart, bool> _CanReference;

        public string InternalPath { get; set; }

        private void cbOKCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (tvwParts?.SelectedItem is IBasePart _ibp)
                && _CanReference(_ibp);
            e.Handled = true;
        }

        private void cbOKCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            var _resolver = tvwParts.SelectedItem as IBasePart;
            InternalPath = _resolver.GetInternalPath();
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
            e.Handled = true;
        }
    }
}
