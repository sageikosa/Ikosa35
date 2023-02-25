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
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.UI;
using Uzi.Ikosa.UI.SpellPrep;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for SpellSlots.xaml
    /// </summary>
    public partial class SpellSlots : UserControl
    {
        public SpellSlots()
        {
            InitializeComponent();
            DataContextChanged += SpellSlots_DataContextChanged;
        }

        void SpellSlots_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            icSlotSets.ItemsSource = null;
            if (DataContext is PresentableCreatureVM _creature)
            {
                var _time = (_creature.Thing.Setting as LocalMap)?.CurrentTime ?? 0d;
                var _list = (from _slottedCaster in _creature.Thing.Classes.OfType<IPreparedCasterClass>()
                             from _set in _slottedCaster.AllSpellSlotSets
                             select (new PreparedSpellSlotSetModel(_slottedCaster, _set.SetIndex, _set.SetName, _set.SlotSet, _time)) as object)
                             .Union(from _spontaneousCaster in _creature.Thing.Classes.OfType<ISlottedCasterClass<SpellSlot>>()
                                    from _set in _spontaneousCaster.AllSpellSlotSets
                                    select new SpontaneousSpellSlotSetModel(_set.SetIndex, _set.SetName, _set.SlotSet, _time))
                                    .ToList();
                icSlotSets.ItemsSource = _list;
            }
        }
    }
}
