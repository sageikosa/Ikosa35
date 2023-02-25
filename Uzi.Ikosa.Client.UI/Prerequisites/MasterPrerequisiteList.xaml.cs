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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for MasterPrerequisiteList.xaml
    /// </summary>
    public partial class MasterPrerequisiteList : UserControl
    {
        public MasterPrerequisiteList()
        {
            try { InitializeComponent(); } catch { }
            DataContextChanged += new DependencyPropertyChangedEventHandler(PrerequisiteList_DataContextChanged);
        }

        void PrerequisiteList_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // TODO: notify UI
        }

        public IEnumerable<PrerequisiteModel> Infos
            => DataContext as IEnumerable<PrerequisiteModel>;

        public Guid StepID
            => Infos.FirstOrDefault()?.Prerequisite.StepID ?? Guid.Empty;
    }
}
