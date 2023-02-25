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
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for CoreSelectPrerequisiteItem.xaml
    /// </summary>
    public partial class CoreSelectPrerequisiteItem : UserControl
    {
        public CoreSelectPrerequisiteItem()
        {
            try { InitializeComponent(); } catch { }

            var _uri = new Uri(@"/Uzi.Ikosa.Client.UI;component/Items/ItemListTemplates.xaml", UriKind.Relative);
            Resources.MergedDictionaries.Add(Application.LoadComponent(_uri) as ResourceDictionary);
            Resources.Add(@"slctItemListTemplate", ItemListTemplateSelector.GetDefault(Resources));

            DataContextChanged += CoreSelectPrerequisiteItem_DataContextChanged;
        }

        public CoreSelectPrerequisiteModel CoreSelectPrerequisiteModel
            => DataContext as CoreSelectPrerequisiteModel;

        public CoreSelectPrerequisiteInfo CoreSelectPrerequisiteInfo
            => CoreSelectPrerequisiteModel?.Prerequisite as CoreSelectPrerequisiteInfo;

        private void CoreSelectPrerequisiteItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _selected = CoreSelectPrerequisiteInfo?.Selected ?? Guid.Empty;
            cboSelect.SelectedItem = CoreSelectPrerequisiteModel?.Infos
                .FirstOrDefault(_c => _c.ID == _selected);
        }

        private void cboSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var _select = CoreSelectPrerequisiteInfo;
            if (_select != null)
            {
                _select.Selected = (cboSelect.SelectedItem as AwarenessInfo)?.ID ?? Guid.Empty;
                _select.IsReady = (_select.Selected != null);
            }
        }
    }
}
