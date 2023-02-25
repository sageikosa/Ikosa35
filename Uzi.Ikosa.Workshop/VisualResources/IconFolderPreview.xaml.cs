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

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for IconFolderPreview.xaml
    /// </summary>
    public partial class IconFolderPreview : UserControl
    {
        public IconFolderPreview()
        {
            InitializeComponent();
        }

        public IHostTabControl HostTabControl
        {
            get { return (IHostTabControl)GetValue(HostedTabControlProperty); }
            set { SetValue(HostedTabControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HostedTabControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HostedTabControlProperty =
            DependencyProperty.Register(nameof(HostTabControl), typeof(IHostTabControl), typeof(IconFolderPreview),
                new PropertyMetadata(null));

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstIcon.SelectedItem is IconPart _icon)
            {
                HostTabControl?.FindOrOpen<IconViewerTab>(
                    _iv => _iv.IconPart == _icon,
                    () => new IconViewerTab(_icon, HostTabControl));
            }

        }

        private void miOpen_Click(object sender, RoutedEventArgs e)
        {
            if (lstIcon.SelectedItem is IconPart _icon)
            {
                HostTabControl?.FindOrOpen<IconViewerTab>(
                    _iv => _iv.IconPart == _icon,
                    () => new IconViewerTab(_icon, HostTabControl));
            }
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lstIcon.SelectedItem is IconPart _icon)
            {
                if (_icon.NameManager != null)
                {
                    if (_icon.NameManager is VisualResources)
                    {
                        (_icon.NameManager as VisualResources).RemovePart(_icon);
                    }
                    else if (_icon.NameManager is CorePackage)
                    {
                        (_icon.NameManager as CorePackage).Remove(_icon);
                    }
                    else if (_icon.NameManager is CorePackagePartsFolder)
                    {
                        (_icon.NameManager as CorePackagePartsFolder).Remove(_icon);
                    }
                }
            }
        }
    }
}
