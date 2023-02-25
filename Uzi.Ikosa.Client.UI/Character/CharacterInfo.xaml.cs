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

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for CharacterInfo.xaml
    /// </summary>
    public partial class CharacterInfo : UserControl
    {
        public CharacterInfo()
        {
            try { InitializeComponent(); } catch { }
        }

        public Visibility Take10Visibility
        {
            get { return (Visibility)GetValue(Take10VisibilityProperty); }
            set { SetValue(Take10VisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Take10Visibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Take10VisibilityProperty =
            DependencyProperty.Register(nameof(Take10Visibility), typeof(Visibility), 
                typeof(CharacterInfo), new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(Take10VisibilityChanged)));

        private static void Take10VisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _cInfo = d as CharacterInfo;
            if (_cInfo != null)
            {
                var _vis = (Visibility)e.NewValue;
                _cInfo.cboTakeSTR.Visibility = _vis;
                _cInfo.cboTakeDEX.Visibility = _vis;
                _cInfo.cboTakeCON.Visibility = _vis;
                _cInfo.cboTakeINT.Visibility = _vis;
                _cInfo.cboTakeWIS.Visibility = _vis;
                _cInfo.cboTakeCHA.Visibility = _vis;
            }
        }

    }
}
