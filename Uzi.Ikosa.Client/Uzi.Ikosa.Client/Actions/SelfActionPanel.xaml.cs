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
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Client
{
    /// <summary>
    /// Interaction logic for SelfActionPanel.xaml
    /// </summary>
    public partial class SelfActionPanel : UserControl
    {
        public SelfActionPanel()
        {
            try { InitializeComponent(); } catch { }
        }

        public void SetActions(IEnumerable<ActionInfo> actions)
        {
            var _actions = actions.ToList();
            mnuSelfActions.Items.Clear();

            // group 2 (options)
            var _optActs = (from _a in actions
                            where (_a.Provider.ProviderInfo is FeatInfo) || (_a.Provider.ProviderInfo is SkillInfo)
                            group _a by _a.Provider.ID).ToList();
            // NOTE: referenced code, removed definition
            // ActionMenus.AddMenuItems(mnuSelfActions, _optActs, actions, null);

            // group 3 (species and templates)...
            // TODO: species --> creature class?
            // TODO: templates ... not currently defined
        }
    }
}
