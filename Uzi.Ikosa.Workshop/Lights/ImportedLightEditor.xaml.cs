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
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ImportedLightEditor.xaml
    /// </summary>
    public partial class ImportedLightEditor : TabItem, IHostedTabItem
    {
        public ImportedLightEditor(ImportedLight light, IHostTabControl host)
        {
            InitializeComponent();
            this.DataContext = light;
            _Host = host;
        }

        private IHostTabControl _Host;

        public ImportedLight ImportedLight { get { return DataContext as ImportedLight; } }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region double text field validation
        private void txtDbl_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            double _out = 0;
            if (!double.TryParse(_txt.Text, out _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            if (_txt.Name.EndsWith(@"Off"))
            {
                if (_out < 0)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Try to keep the light point within the locator";
                    return;
                }
            }
            else if (_out <= 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Only positives allowed for light ranges";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion
    }
}
