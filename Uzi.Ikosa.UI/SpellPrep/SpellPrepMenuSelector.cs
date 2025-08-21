using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Uzi.Ikosa.UI.SpellPrep
{
    public class SpellPrepMenuSelector : StyleSelector
    {
        public Style ChargedSpontaneousSlotStyle { get; set; }
        public Style UnchargedSlotStyle { get; set; }
        public Style EmptyPreparedSlotStyle { get; set; }
        public Style FilledPreparedSlotStyle { get; set; }
        public Style MenuStyle { get; set; }
        public Style PrepareStyle { get; set; }

        public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
        {
            if (item is SpontaneousSpellSlotModel)
            {
                var _slot = item as SpontaneousSpellSlotModel;
                if (_slot.IsCharged)
                {
                    return ChargedSpontaneousSlotStyle;
                }

                return UnchargedSlotStyle;
            }
            else if (item is PreparedSpellSlotModel)
            {
                var _slot = item as PreparedSpellSlotModel;
                if (_slot.SpellSlot.PreparedSpell != null)
                {
                    return FilledPreparedSlotStyle;
                }

                if (_slot.IsCharged)
                {
                    return EmptyPreparedSlotStyle;
                }

                return UnchargedSlotStyle;
            }
            else if (item is PrepareSpellModel)
            {
                return PrepareStyle;
            }
            else if (item is MenuBaseViewModel)
            {
                return MenuStyle;
            }
            return base.SelectStyle(item, container);
        }
    }
}
