using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.UI.SpellPrep
{
    public class PreparedSpellSlotModel : SpellSlotModel<PreparedSpellSlot>
    {
        public PreparedSpellSlotModel(PreparedSpellSlotLevelModel level, IPreparedCasterClass caster,
            List<ClassSpell> spells, PreparedSpellSlot slot, double time)
            : base(level, slot, time)
        {
            _Spells = spells;
            _Caster = caster;
            _PrepareSpell = new RelayCommand<ClassSpell>((spell) =>
            {
                SpellSlot.RechargeSlot(time);
                SpellSlot.PreparedSpell = new SpellSource(_Caster, spell.Level, SpellSlot.SlotLevel, false, spell.SpellDef);
                DoPropertyChanged(nameof(IsCharged));
                DoPropertyChanged(nameof(SpellSlot));
                _Level.RefreshSlots();
            });
            AddMenuItems(_Menus);
        }

        #region data
        private readonly List<ClassSpell> _Spells;
        private IPreparedCasterClass _Caster;
        private readonly RelayCommand<ClassSpell> _PrepareSpell;
        #endregion

        protected override void AddMenuItems(List<object> menus)
        {
            // discharge menu
            var _dischargeMenu = new MenuViewModel
            {
                Header = @"Discharge",
            };
            base.AddMenuItems(_dischargeMenu.SubItems);
            menus.Add(_dischargeMenu);
            // menus.Add(new SeparatorViewModel());

            // prepare menu
            var _prepareMenu = new MenuViewModel
            {
                Header = @"Prepare"
            };
            menus.Add(_prepareMenu);
            var _levelItems = from _s in _Spells
                              let _sm = new PrepareSpellModel { ClassSpell = _s, SlotModel = this }
                              group _sm by _sm.ClassSpell.Level
                                  into _lvl
                              select (object)(new MenuViewModel
                              {
                                  Header = $@"Level {_lvl.Key}",
                                  SubItems = (from _l in _lvl select (object)_l).ToList()
                              });

            _prepareMenu.SubItems.AddRange(_levelItems);
        }

        public RelayCommand<ClassSpell> PrepareSpell => _PrepareSpell;
    }
}
