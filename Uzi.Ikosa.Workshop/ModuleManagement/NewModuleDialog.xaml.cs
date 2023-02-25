using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop.ModuleManagement
{
    /// <summary>
    /// Interaction logic for NewModuleDialog.xaml
    /// </summary>
    public partial class NewModuleDialog : Window
    {
        private IkosaPackageManagerConfig _Config;
        private List<PackagePathEntry> _Campaigns;
        private RelayCommand _OKCmd;

        public NewModuleDialog(IkosaPackageManagerConfig config)
        {
            InitializeComponent();
            _Config = config;
            _Campaigns = _Config.Campaigns.SelectMany(_c => _c.GetPackagePaths()).ToList();
            _OKCmd = new RelayCommand(() =>
            {
                DialogResult = true;
                Close();
            }, () => lstCampaign.SelectedIndex >= 0);
            DataContext = this;
        }

        public RelayCommand OKCommand => _OKCmd;
        public List<PackagePathEntry> Campaigns => _Campaigns;
        public PackagePathEntry SelectedCampaign => lstCampaign?.SelectedItem as PackagePathEntry;

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}
