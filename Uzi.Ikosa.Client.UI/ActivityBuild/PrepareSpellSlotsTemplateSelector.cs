using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client.UI
{
    public class PrepareSpellSlotsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SpellSlotSetTemplate { get; set; }
        public DataTemplate SpellSlotLevelTemplate { get; set; }
        public DataTemplate AvailableSlotTemplate { get; set; }
        public DataTemplate LockedSlotTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch (item)
            {
                case SpellSlotSetTargeting _slotSet:
                    return SpellSlotLevelTemplate;

                case SpellSlotLevelTargeting _slotLevel:
                    return SpellSlotLevelTemplate;

                case PreparedSpellSlotTargeting _prepSpell:
                    var _info = _prepSpell.OriginalPreparedSpellSlotInfo;
                    if (_prepSpell.SpellSlotLevel.SpellSlotSet.PrepareSpellSlots.PrepareSpellSlotsAimInfo.AbandonRecharge)
                    {
                        if (_info.IsCharged || _info.CanRecharge)
                        {
                            return AvailableSlotTemplate;
                        }
                        return LockedSlotTemplate;
                    }
                    else if (!_info.IsCharged)
                    {
                        return LockedSlotTemplate;
                    }
                    else if (!(_info.SpellSource?.SpellDef?.Key == null))
                    {
                        return LockedSlotTemplate;
                    }
                    return AvailableSlotTemplate;

                default:
                    break;
            }
            return null;
        }
    }
}
