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
using System.Windows.Shapes;
using CharacterModeler.Generator;
using System.Xml;

namespace CharacterModeler
{
    /// <summary>
    /// Interaction logic for CharacterModelerFrame.xaml
    /// </summary>
    public partial class CharacterModelerFrame : Window
    {
        public CharacterModelerFrame()
        {
            InitializeComponent();
        }

        private void GenerateWeapons(object sender, RoutedEventArgs e)
        {
            //var _path = @"C:\Users\jousey\Source\Workspaces\XAML\Frags\";
            const string _PATH = @"C:\Ikosa.Resources\resources\MetaModels\Fragments\Items\Weapons\";
            Action<FragmentGenerator, string> _makeFile =
                (generator, name) =>
                {
                    using (var _writer = XmlWriter.Create($@"{_PATH}{name}.frag"))
                        generator.WriteXml(_writer);
                };

            _makeFile(new BastardSword(), @"BastardSword");
            _makeFile(new BattleAxe(), @"BattleAxe");
            _makeFile(new Dagger(), @"Dagger");
            _makeFile(new Dart(), @"Dart");
            _makeFile(new DoubleAxe(), @"DoubleAxe");
            _makeFile(new GreatAxe(), @"GreatAxe");
            _makeFile(new GreatSword(), @"GreatSword");
            // halberd
            _makeFile(new HandCrossbow(), @"HandCrossbow");
            _makeFile(new HeavyCrossbow(), @"HeavyCrossbow");
            _makeFile(new HeavyMace(), @"HeavyMace");
            // heavy pick
            // hooked hammer
            _makeFile(new Javelin(), @"Javelin");
            _makeFile(new LightCrossbow(), @"LightCrossbow");
            // light hammer
            _makeFile(new LightMace(), @"LightMace");
            // light pick
            _makeFile(new LongBow(), @"LongBow");
            // long spear
            _makeFile(new LongSword(), @"LongSword");
            _makeFile(new MorningStar(), @"MorningStar");
            _makeFile(new Shield(), @"Shield");
            _makeFile(new ShortBow(), @"ShortBow");
            _makeFile(new ShortSword(), @"ShortSword");
            // spear
            // throwing axe
            // two bladed sword
            // war axe
            _makeFile(new WarHammer(), @"Warhammer");
        }
    }
}
