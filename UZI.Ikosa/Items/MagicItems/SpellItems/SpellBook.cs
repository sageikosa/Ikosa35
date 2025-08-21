using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class SpellBook : ItemBase, IActionProvider
    {
        #region construction
        public SpellBook(string name)
            : this(name, 100, 100)
        {
        }

        protected SpellBook(string name, int totalPages, decimal pageCost)
            : base(@"Book", Size.Miniature)
        {
            Name = name;
            _PagesLeft = totalPages;
            _PageCost = pageCost;
            _TotalPages = totalPages;

            _Spells = [];
            ItemMaterial = LeatherMaterial.Static; // book binding
            MaxStructurePoints.BaseValue = 3;
            // TODO: ... break DC of 8.
            BaseWeight = 3d;
        }
        #endregion

        #region private data
        private int _PagesLeft;
        private int _TotalPages;
        private decimal _PageCost;
        private Collection<BookSpell> _Spells;
        #endregion

        /// <summary>Cost to be paid to fill a page</summary>
        public decimal PageCost => _PageCost;

        /// <summary>Pages left in the book</summary>
        public int PagesLeft => _PagesLeft;

        /// <summary>Total pages in the book</summary>
        public int TotalPages => _TotalPages;

        public bool CanHoldSpell(int level)
            => (level == 0) ? PagesLeft > 0 : PagesLeft >= level;

        #region public void Add(BookSpell spell)
        public void Add(BookSpell spell)
        {
            var _needs = (spell.Level > 0 ? spell.Level : 1);
            if (_needs <= _PagesLeft)
            {
                _Spells.Add(spell);
                _PagesLeft -= _needs;
                SetAugmentationPrice();
                DoPropertyChanged(@"PagesLeft");
                DoPropertyChanged(@"Spells");
            }
        }
        #endregion

        #region public void Remove(BookSpell spell)
        public void Remove(BookSpell spell)
        {
            if (_Spells.Contains(spell))
            {
                var _needs = (spell.Level > 0 ? spell.Level : 1);
                _Spells.Remove(spell);
                _PagesLeft += _needs;
                SetAugmentationPrice();
                DoPropertyChanged(@"PagesLeft");
                DoPropertyChanged(@"Spells");
            }
        }
        #endregion

        public IEnumerable<BookSpell> Spells
            => _Spells.Select(_s => _s);

        public IEnumerable<BookSpell> DecipheredSpells(Guid casterGuid)
            => _Spells.Where(_s => _s.HasDecipheredOrScribed(casterGuid));

        public IEnumerable<BookSpell> UnDecipheredSpells(Guid casterGuid)
            => _Spells.Where(_s => !_s.HasDecipheredOrScribed(casterGuid));

        protected virtual void SetAugmentationPrice()
        {
            Price.BaseItemExtraPrice = 15 + (TotalPages - PagesLeft) * PageCost;
        }

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            // TODO: budget

            // TODO: prepare into spell slots (owner versus borrower?)
            // TODO: enscribe new spell
            // TODO: copy spell book
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            return GetInfoData.GetInfoFeedback(this, budget.Actor);
        }
        #endregion

        protected override string ClassIconKey { get { return @"book"; } }
    }
}
