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

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for ActivityInfoBuildControl.xaml
    /// </summary>
    public partial class ActivityInfoBuildControl : UserControl
    {
        public ActivityInfoBuildControl()
        {
            try { InitializeComponent(); } catch { }
            var _uri = new Uri(@"/Uzi.Ikosa.Client.UI;component/Items/ItemListTemplates.xaml", UriKind.Relative);
            Resources.MergedDictionaries.Add(Application.LoadComponent(_uri) as ResourceDictionary);
            Resources.Add(@"slctItemListTemplate", ItemListTemplateSelector.GetDefault(Resources));
            Resources.Add(@"iconItemListTemplate", ItemListTemplateSelector.GetMenuDefault(Resources));
        }
    }
}
