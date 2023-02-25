using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for RangedTriggerableAttackEditor.xaml
    /// </summary>
    public partial class RangedTriggerableAttackEditor : TabItem, IHostedTabItem
    {
        public RangedTriggerableAttackEditor(IHostTabControl host)
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

        public RangedTriggerable RangedTriggerable
        {
            get { return (RangedTriggerable)GetValue(RangedTriggerableProperty); }
            set { SetValue(RangedTriggerableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MeleeTriggerable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RangedTriggerableProperty =
            DependencyProperty.Register(
                nameof(RangedTriggerable),
                typeof(RangedTriggerable),
                typeof(RangedTriggerableAttackEditor),
                new PropertyMetadata(null, new PropertyChangedCallback(OnOpenCloseTriggerableChanged)));

        public int CriticalLow
        {
            get => RangedTriggerable.CriticalLow;
            set
            {
                RangedTriggerable.CriticalRange = 21 - value;
            }
        }

        public int DiceCount
        {
            get => RangedTriggerable.DamageDice?.Number ?? 0;
            set
            {
                RangedTriggerable.DamageDice = new DiceRoller(value, (byte)DiceSides);
            }
        }

        public StandardDieType DiceSides
        {
            get => (StandardDieType)RangedTriggerable.DamageDice?.Sides;
            set
            {
                RangedTriggerable.DamageDice = new DiceRoller(DiceCount, (byte)value);
            }
        }

        private static void OnOpenCloseTriggerableChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is RangedTriggerableAttackEditor _editor)
            {
                // ???
            }
        }
    }
}
