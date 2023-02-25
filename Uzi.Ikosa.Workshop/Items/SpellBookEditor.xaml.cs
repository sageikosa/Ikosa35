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
    /// Interaction logic for SpellBookEditor.xaml
    /// </summary>
    public partial class SpellBookEditor : UserControl
    {
        public SpellBookEditor()
        {
            InitializeComponent();
        }

        private SpellBook _SpellBook => (DataContext as SpellBookVM)?.Thing;

    }
}
