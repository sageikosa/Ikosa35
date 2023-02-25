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
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for PreRequisiteDialog.xaml
    /// </summary>
    public partial class PreRequisiteDialog : Window
    {
        public static RoutedCommand OKCommand = new RoutedCommand();

        public Guid StepID { get { return preList.StepID; } }

        public PreRequisiteDialog(IPrerequisiteProxy actorModel)
        {
            // TODO: must set dataContext before using!
            InitializeComponent();
            if (!preList.Infos.Any())
            {
                this.Close();
            }
        }
    }
}
