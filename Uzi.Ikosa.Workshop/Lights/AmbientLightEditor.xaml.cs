using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for AmbientLightEditor.xaml
    /// </summary>
    public partial class AmbientLightEditor : TabItem, IHostedTabItem
    {
        public AmbientLightEditor(Uzi.Ikosa.Tactical.AmbientLight light, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = light;
            _Host = host;
        }

        private IHostTabControl _Host;

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

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        public Uzi.Ikosa.Tactical.AmbientLight Light { get { return DataContext as Uzi.Ikosa.Tactical.AmbientLight; } }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region light range text field validation
        private void txtLight_TextChanged(object sender, TextChangedEventArgs e)
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
                _txt.ToolTip = @"Only positives allowed for light ranges";
                return;
            }

            var _light = Light;
            if (_light.VeryBright <= 0)
            {
                if (_light.Bright <= 0)
                {
                    if (_light.NearShadow <= 0)
                    {
                        if (_light.FarShadow <= 0)
                        {
                            _light.AmbientLevel = Tactical.LightRange.OutOfRange;
                        }
                        else
                        {
                            _light.AmbientLevel = Tactical.LightRange.FarShadow;
                        }
                    }
                    else
                    {
                        _light.AmbientLevel = Tactical.LightRange.NearShadow;
                    }
                }
                else
                {
                    _light.AmbientLevel = Tactical.LightRange.Bright;
                }
            }
            else
            {
                _light.AmbientLevel =
                    (chkSolar.IsChecked ?? false) ? Tactical.LightRange.Solar : Tactical.LightRange.VeryBright;
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
