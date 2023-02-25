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
using Uzi.Core.Dice;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for MeleeTriggerableAttackEditor.xaml
    /// </summary>
    public partial class MeleeTriggerableAttackEditor : TabItem, IHostedTabItem
    {
        public MeleeTriggerableAttackEditor(IHostTabControl host)
        {
            InitializeComponent();
            DataContext = this;

            _Host = host;
        }

        private IHostTabControl _Host;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion

        public MeleeTriggerable MeleeTriggerable
        {
            get { return (MeleeTriggerable)GetValue(MeleeTriggerableProperty); }
            set { SetValue(MeleeTriggerableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MeleeTriggerable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MeleeTriggerableProperty =
            DependencyProperty.Register(
                nameof(MeleeTriggerable),
                typeof(MeleeTriggerable),
                typeof(MeleeTriggerableAttackEditor),
                new PropertyMetadata(null, new PropertyChangedCallback(OnOpenCloseTriggerableChanged)));

        public int CriticalLow
        {
            get => MeleeTriggerable.CriticalLow;
            set
            {
                MeleeTriggerable.CriticalRange = 21 - value;
            }
        }

        public int DiceCount
        {
            get => MeleeTriggerable.DamageDice?.Number ?? 0;
            set
            {
                MeleeTriggerable.DamageDice = new DiceRoller(value, (byte)DiceSides);
            }
        }

        public StandardDieType DiceSides
        {
            get => (StandardDieType)MeleeTriggerable.DamageDice?.Sides;
            set
            {
                MeleeTriggerable.DamageDice = new DiceRoller(DiceCount, (byte)value);
            }
        }

        private static void OnOpenCloseTriggerableChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is MeleeTriggerableAttackEditor _editor)
            {
                // ???
            }
        }
    }
}
