using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Uzi.Ikosa.UI;
using Uzi.Ikosa.UI.MVVM.Package;

namespace Uzi.Ikosa.Workshop.ModuleManagement
{
    /// <summary>
    /// Interaction logic for SavePackageAsDialog.xaml
    /// </summary>
    public partial class SavePackageAsDialog : Window
    {
        private readonly List<PackageFileEntry> _PackageFiles;
        private readonly RelayCommand _OKCmd;

        public SavePackageAsDialog(IPackageFilesFolder packageFilesFolder)
        {
            InitializeComponent();
            _PackageFiles = packageFilesFolder.PackageFiles.Select(_pf => _pf.PackageFileEntry).ToList();
            _OKCmd = new RelayCommand(() =>
            {
                DialogResult = true;
                Close();
            }, 
            () => !string.IsNullOrWhiteSpace(txtPackageName.Text)
                && !_PackageFiles.Any(_c => string.Equals(_c.Name, $@"{txtPackageName.Text}.ikosa", StringComparison.InvariantCultureIgnoreCase)));
            DataContext = this;
        }
        public RelayCommand OKCommand => _OKCmd;
        public List<PackageFileEntry> PackageFiles => _PackageFiles;
        public string PackageName => txtPackageName?.Text;

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
