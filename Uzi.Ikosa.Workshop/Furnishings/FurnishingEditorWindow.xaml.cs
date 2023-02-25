using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for FurnishingEditorWindow.xaml
    /// </summary>
    public partial class FurnishingEditorWindow : Window
    {
        public FurnishingEditorWindow()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cbOpenItem_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (e.Parameter is PresentableContainerItemVM)
                || (e.Parameter is PresentableSlottedContainerItemVM)
                || (e.Parameter is KeyRingVM)
                || typeof(PresentableAmmunitionBundle<,,>).IsAssignableFrom(e.Parameter.GetType());
            e.Handled = true;
        }

        private void cbOpenItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _edit = new ObjectEditorWindow(e.Parameter as PresentableContext)
            {
                Title = @"Edit Object",
                Owner = Window.GetWindow(this),
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
            };
            _edit.ShowDialog();
        }
    }
}
