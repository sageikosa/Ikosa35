using System;
using System.Collections.Generic;
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
using Uzi.Core;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for LabelModifiable.xaml
    /// </summary>

    public partial class LabelModifiable : System.Windows.Controls.UserControl
    {
        public LabelModifiable()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            //if (_Modifiable != null)
            //{
            //    _Modifiable.ValueChanged -= new EventHandler(_Modifiable_ValueChanged);
            //}
            _Modifiable = null;
        }

        public void InitDeltable(Deltable modifiable)
        {
            _Modifiable = modifiable;
            //_Modifiable.ValueChanged += new EventHandler(_Modifiable_ValueChanged);
            RebuildControl();
        }

        void _Modifiable_ValueChanged(object sender, EventArgs e)
        {
            RebuildControl();
        }

        private void RebuildControl()
        {
            // show label value
            lblValue.Content = _Modifiable.EffectiveValue.ToString();

            // rebuild tooltip
            ToolTip _tip = new ToolTip();
            this.ToolTip = _tip;
            StackPanel _stack = new StackPanel();
            _tip.Content = _stack;

            // base value
            Label _lblBase = new Label();
            _lblBase.FontWeight = FontWeights.Bold;
            _lblBase.Content = string.Format("Base = {0}", _Modifiable.BaseValue);
            _lblBase.Margin = new Thickness(1,0,1,0);
            _stack.Children.Add(_lblBase);

            // modifiers
            foreach (IDelta _mod in _Modifiable)
            {
                Label _lblMod = new Label();
                _lblMod.Margin = new Thickness(1,0,1,0);
                _lblMod.Content = Delta.FormatModifier(_mod);
                _stack.Children.Add(_lblMod);
            }
        }

        private Deltable _Modifiable;
    }
}