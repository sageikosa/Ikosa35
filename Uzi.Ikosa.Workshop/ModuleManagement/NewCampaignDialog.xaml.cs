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
using Uzi.Ikosa.UI;
using Uzi.Ikosa.UI.MVVM.Package;

namespace Uzi.Ikosa.Workshop.ModuleManagement
{
    /// <summary>
    /// Interaction logic for NewCampaignDialog.xaml
    /// </summary>
    public partial class NewCampaignDialog : Window
    {
        private readonly List<PackagePathEntry> _Campaigns;
        private readonly RelayCommand _OKCmd;

        public NewCampaignDialog(PackageSetVM packageSetVM)
        {
            InitializeComponent();
            _Campaigns = packageSetVM.Campaigns.Select(_p => _p.PackagePathEntry).ToList();
            _OKCmd = new RelayCommand(() =>
            {
                DialogResult = true;
                Close();
            }, 
            () => !string.IsNullOrWhiteSpace(txtCampaignName.Text)
                && !_Campaigns.Any(_c => string.Equals(_c.Name, txtCampaignName.Text, StringComparison.InvariantCultureIgnoreCase)));
            DataContext = this;
        }

        public RelayCommand OKCommand => _OKCmd;
        public List<PackagePathEntry> Campaigns => _Campaigns;
        public string CampaignName => txtCampaignName?.Text;

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
