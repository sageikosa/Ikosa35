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

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ContainerEditor.xaml
    /// </summary>
    public partial class ContainerEditor : UserControl
    {
        public ContainerEditor()
        {
            InitializeComponent();
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

            if (_out < 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Negatives not allowed for Weight";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion
    }
}
