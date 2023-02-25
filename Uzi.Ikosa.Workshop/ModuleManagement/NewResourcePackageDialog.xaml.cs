using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop.ModuleManagement
{
    /// <summary>
    /// Interaction logic for NewResourcePackageDialog.xaml
    /// </summary>
    public partial class NewResourcePackageDialog : Window
    {
        private IkosaPackageManagerConfig _Config;
        private List<PackagePathEntry> _PackagePaths;
        private RelayCommand _OKCmd;

        public NewResourcePackageDialog(IkosaPackageManagerConfig config)
        {
            InitializeComponent();
            _Config = config;
            _PackagePaths = _Config.Packages.Union(_Config.Campaigns.SelectMany(_c => _c.GetPackagePaths())).ToList();
            _OKCmd = new RelayCommand(() =>
            {
                DialogResult = true;
                Close();
            }, () => lstPackageSets.SelectedIndex >= 0);
            DataContext = this;
        }

        public RelayCommand OKCommand => _OKCmd;
        public List<PackagePathEntry> PackagePaths => _PackagePaths;
        public PackagePathEntry SelectedPackageSet => lstPackageSets?.SelectedItem as PackagePathEntry;

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
