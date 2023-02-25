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
    /// Interaction logic for ModuleOpenDialog.xaml
    /// </summary>
    public partial class PackageOpenDialog : Window
    {
        private IkosaPackageManagerConfig _Config;
        private RelayCommand _OKCmd;

        public PackageOpenDialog(IkosaPackageManagerConfig config)
        {
            InitializeComponent();
            _Config = config;
            _OKCmd = new RelayCommand(() =>
            {
                DialogResult = true;
                Close();
            }, () => tvwPackages.SelectedItem is PackageFileVM);
            DataContext = this;
        }

        public IList<object> PackageSources
            => _Config.Campaigns
            .Select(_c => new PackageSetVM
            {
                PackageSetPathEntry = _c,
                IsNodeExpanded = true,
                CommandHost = this
            })
            .Cast<object>()
            .Union(
                _Config.Packages
                .Select(_p => new PackagePathVM
                {
                    PackagePathEntry = _p,
                    IsNodeExpanded = true,
                    CommandHost = this
                }))
            .ToList();

        public RelayCommand OKCommand => _OKCmd;

        public PackageFileVM SelectedPackageFile => tvwPackages.SelectedItem as PackageFileVM;

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
