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
using Uzi.Ikosa.Items;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for WandEditor.xaml
    /// </summary>
    public partial class WandEditor : UserControl
    {
        public WandEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(WandEditor_DataContextChanged);
        }

        private Wand _Wand => (DataContext as WandVM)?.Thing; 

        void WandEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            udCharge.Value = _Wand.SpellTrigger.PowerBattery.AvailableCharges;
            udMaxCharge.Value = _Wand.SpellTrigger.PowerBattery.MaximumCharges.BaseValue;
        }

        private void udMaxCharge_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var _slider = Convert.ToInt32(udMaxCharge.Value);
            _Wand.SpellTrigger.PowerBattery.MaximumCharges.BaseValue = _slider;
            udCharge.Value = _Wand.SpellTrigger.PowerBattery.AvailableCharges;
        }

        private void udCharge_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var _slider = Convert.ToInt32(udCharge.Value);
            var _charges = _Wand.SpellTrigger.PowerBattery.AvailableCharges;
            if (_slider > _charges)
            {
                _Wand.SpellTrigger.PowerBattery.AddCharges(_slider - _charges);
            }
            else if (_slider < _charges)
            {
                _Wand.SpellTrigger.PowerBattery.UseCharges(_charges - _slider);
            }
            else
            {
                _Wand.SetPrice();
            }
        }
    }
}
