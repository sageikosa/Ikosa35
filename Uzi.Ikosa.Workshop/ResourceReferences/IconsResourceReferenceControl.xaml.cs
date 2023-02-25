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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Packaging;
using Uzi.Visualize.Packaging;
using Uzi.Visualize;
using Uzi.Ikosa.Workshop.ModuleManagement;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for IconsResourceReferenceControl.xaml
    /// </summary>
    public partial class IconsResourceReferenceControl : UserControl
    {
        public static RoutedCommand SelectCmd = new RoutedCommand();

        public IconsResourceReferenceControl()
        {
            InitializeComponent();
        }

        private IconsResourceReferenceTab GetTab()
            => DataContext as IconsResourceReferenceTab;

        private IconsResourceReference GetReference()
            => GetTab()?.Reference;

        #region private void btnBrowse_Click(object sender, RoutedEventArgs e)
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (GetReference() != null)
            {
                var _dlg = new PackageOpenDialog(GetTab().Config)
                {
                };
                if (_dlg.ShowDialog() ?? false)
                {
                    var _ref = GetReference();
                    var _pck = _dlg.SelectedPackageFile;
                    _ref.PackageSet = _pck.InPackageSet ? _pck.Folder.PackagePathEntry.Name : string.Empty;
                    _ref.PackageID = _pck.PackageFileEntry.Name;

                    var _dc = DataContext;
                    DataContext = null;
                    DataContext = _dc;
                }
            }
        }
        #endregion

        private void cbSelectCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (GetReference() != null) && (GetReference().ResolvePackage() != null);
            e.Handled = true;
        }

        private void cbSelectCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _package = GetReference().ResolvePackage();
            var _selector = new SelectPart(_package, SelectPart.UnboundVisualResources)
            {
                Owner = Window.GetWindow(this)
            };
            if (_selector.ShowDialog() ?? false)
            {
                var _ref = GetReference();
                GetReference().InternalPath = _selector.InternalPath;

                var _dc = DataContext;
                DataContext = null;
                DataContext = _dc;
            }
            e.Handled = true;
        }
    }
}
