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
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Core;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for Creature.xaml
    /// </summary>

    public partial class CreatureEditor : TabItem, IHostedTabItem
    {
        public CreatureEditor(PresentableCreatureVM critterVM, IHostTabControl host)
            : base()
        {
            InitializeComponent();
            DataContext = critterVM;
            _Host = host;
        }

        private readonly IHostTabControl _Host;

        public PresentableCreatureVM PresentableCreature => DataContext as PresentableCreatureVM;
        public Creature Creature => PresentableCreature.Thing;

        public void Refresh()
        {
            var _vm = PresentableCreature;
            DataContext = null;
            DataContext = _vm;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
            e.Handled = true;
        }

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion
    }
}
