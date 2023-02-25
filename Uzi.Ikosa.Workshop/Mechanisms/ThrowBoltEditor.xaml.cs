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
    /// Interaction logic for ThrowboltEditor.xaml
    /// </summary>
    public partial class ThrowBoltEditor : UserControl
    {
        public ThrowBoltEditor(ThrowBolt bolt)
        {
            InitializeComponent();
            DataContext = this;
            _Bolt = bolt;
            tipObject.DataContext = ThrowBolt;
        }

        private ThrowBolt _Bolt;
        public ThrowBolt ThrowBolt { get { return _Bolt; } }

        public double OpenState
        {
            get => _Bolt.OpenState.Value;
            set => _Bolt.OpenState = new Core.OpenStatus(value);
        }
    }
}
