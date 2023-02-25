using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class CoveringWrapper : SlottedItemBase
    {
        #region ctor()
        /// <summary>
        /// A CoveringWrapper creates a CoveringSlot and covers the creature.  
        /// It blocks natural unslotting
        /// Only itself can be slotted in the CoveringSlot.
        /// </summary>
        public CoveringWrapper(Creature possessor, ICanCover coreObject)
            : base(coreObject.Name, ItemSlot.Covering)
        {
            _Possessor = possessor;
            _Obj = coreObject;
            Cover();
        }
        #endregion

        #region data
        private ICanCover _Obj;
        #endregion

        /// <summary>The object wrapped by the attachment wrapper</summary>
        public override ICoreObject BaseObject
            => _Obj;

        public ICanCover CoverSource => _Obj;

        public override double Weight
        {
            get => _Obj?.Weight ?? 0d;
            set => base.Weight = 0;
        }

        public override bool IsTransferrable => false;
        protected override string ClassIconKey => string.Empty;

        #region protected override void OnPossessorChanged()
        protected override void OnPossessorChanged()
        {
            if (_Obj is ICoreItem)
            {
                (_Obj as ICoreItem).Possessor = Possessor;
            }
            base.OnPossessorChanged();
        }
        #endregion

        #region private CoveringSlot Cover()
        private CoveringSlot Cover()
        {
            // get slot
            var _slot = CreaturePossessor.Body.ItemSlots.AllSlots
                .OfType<CoveringSlot>()
                .FirstOrDefault(_s => _s.Source == this);
            if (_slot == null)
            {
                // create slot
                _slot = new CoveringSlot(this);
                CreaturePossessor.Body.ItemSlots.Add(_slot);
                SetItemSlot(_slot);
            }

            // assign slot
            if (_slot.SlottedItem == null)
                SetItemSlot(_slot);
            return _slot;
        }
        #endregion

        #region protected override bool FinalSlotCheck(ItemSlot slot)
        protected override bool FinalSlotCheck(ItemSlot slot)
        {
            // can only fill the attachment slot with this attachment wrapper
            if ((slot is CoveringSlot _covSlot) && (_covSlot.Source == this))
                return base.FinalSlotCheck(slot);
            return false;
        }
        #endregion

        #region protected override void OnSetItemSlot()
        protected override void OnSetItemSlot()
        {
            // if covering object is an action provider, it typically provides actions when held (the item will determine whether they are possible)
            if (_Obj is IActionProvider _prov)
            {
                // TODO: ??? covering object may vary actions based on relationship to actor
                if (!CreaturePossessor.Actions.Providers.ContainsKey(this))
                {
                    CreaturePossessor.Actions.Providers.Add(this, _prov);
                }
            }
            CreaturePossessor.ObjectLoad.Add(_Obj);
            _Obj.AddAdjunct(new Covering(this));
            _Obj.AddAdjunct(new Attended(CreaturePossessor));
            base.OnSetItemSlot();
        }
        #endregion

        #region protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        {
            // if we've added an action provider, take it away
            if (CreaturePossessor.Actions.Providers.ContainsKey(this))
            {
                // TODO: actions
                CreaturePossessor.Actions.Providers.Remove(this);
            }

            var _myObj = _Obj;
            _Obj = null;
            CreaturePossessor.ObjectLoad.Remove(_myObj, null);

            // get rid of pathed and aspects
            _myObj.Adjuncts.OfType<Covering>().FirstOrDefault(_a => (_a.CoveringWrapper == this) && _a.IsCovering)?.Eject();
            _myObj.Adjuncts.OfType<Attended>().FirstOrDefault()?.Eject();

            // remove the attachment slot
            var _slot = CreaturePossessor.Body.ItemSlots.AllSlots
                .OfType<CoveringSlot>()
                .FirstOrDefault(_s => _s.Source == this);
            if (_slot != null)
            {
                CreaturePossessor.Body.ItemSlots.Remove(_slot);
            }
            base.OnClearSlots(slotA, slotB);
        }
        #endregion

        public override ActionTime SlottingTime
            => (CoverSource as ICanCoverAsSlot)?.CoverageSlottingTime ?? new ActionTime(Contracts.TimeType.Total);

        public override bool SlottingProvokes
            => (CoverSource as ICanCoverAsSlot)?.CoverageSlottingProvokes ?? true;

        public override ActionTime UnslottingTime
            => (CoverSource as ICanCoverAsSlot)?.CoverageUnSlottingTime ?? new ActionTime(Contracts.TimeType.Total);

        public override bool UnslottingProvokes
            => (CoverSource as ICanCoverAsSlot)?.CoverageUnSlottingProvokes ?? true;
    }
}
