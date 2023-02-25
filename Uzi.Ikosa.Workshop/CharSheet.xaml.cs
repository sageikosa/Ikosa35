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
using System.Windows.Shapes;
using System.Runtime.Serialization.Formatters.Binary;
using Uzi.Ikosa;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Advancement.NPCClasses;
using Uzi.Core.Dice;
using Uzi.Core;
using System.IO;
using System.Xml.Serialization;
using Uzi.Ikosa.Fidelity;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>Interaction logic for CharSheet.xaml</summary>
    public partial class CharSheet : System.Windows.Window
    {
        public CharSheet()
        {
            SizeToContent = SizeToContent.WidthAndHeight;
            InitializeComponent();
            btnRead.Click += new RoutedEventHandler(btnRead_Click);
            //RegionLoad _regLoad = new RegionLoad(@"C:\Documents and Settings\James\My Documents\Ouseysoft\UZI.Ikosa.Environ\Overland\Region.xml");
            //_regLoad.Show();
        }

        void btnRead_Click(object sender, RoutedEventArgs e)
        {
            var _load = Creature.ReadFile(@"D:\UZI\temp\Gargy.Ikosa");
            var _card = new SummaryCard(_load);
            _card.Show();
        }
    }
}