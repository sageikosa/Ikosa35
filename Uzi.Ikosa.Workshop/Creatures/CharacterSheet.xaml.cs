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
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Core;
using Uzi.Visualize.Packaging;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for CharacterSheet.xaml
    /// </summary>
    public partial class CharacterSheet : UserControl
    {
        public CharacterSheet()
        {
            InitializeComponent();
        }

        private PresentableCreatureVM PresentableCreature => DataContext as PresentableCreatureVM;

        private void tbDescription_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var _editor = new DescriptionEditor()
            {
                DataContext = PresentableCreature.Thing,
                Owner = Window.GetWindow(this)
            };
            _editor.ShowDialog();

            // hard refresh
            var _dc = DataContext;
            DataContext = null;
            DataContext = _dc;
        }

        private void mnuPortrait(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem _mnu)
            {
                if (_mnu.Header is BitmapImagePartListItem _partListItem)
                {
                    var _imgAdj = PresentableCreature.Thing.Adjuncts.OfType<ImageKeyAdjunct>().FirstOrDefault();
                    if ((_imgAdj != null) && !_imgAdj.Key.Equals(_partListItem.BitmapImagePart.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        _imgAdj.Eject();
                    }
                    PresentableCreature.Thing.AddAdjunct(new ImageKeyAdjunct(_partListItem.BitmapImagePart.Name, 0));

                    // force a rebind (too lazy to "fix" this in a more elegant way right now!)
                    var _vm = PresentableCreature;
                    DataContext = null;
                    DataContext = _vm;
                }
            }
            e.Handled = true;
        }

        private void imgPortrait_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                var _imgAdj = PresentableCreature.Thing.Adjuncts.OfType<ImageKeyAdjunct>().FirstOrDefault();
                if (_imgAdj != null)
                {
                    _imgAdj.Eject();
                }

                // force a rebind (too lazy to "fix" this in a more elegant way right now!)
                var _vm = PresentableCreature;
                DataContext = null;
                DataContext = _vm;
                e.Handled = true;
            }
        }
    }
}
