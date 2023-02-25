using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// An AttachmentWrapper creates an AttachmentSlot and attaches itself to the creature.  
    /// If it is unslotted, it removes the slot.
    /// Only itself can be slotted in the AttachmentSlot.
    /// </summary>
    [Serializable]
    public class AttachmentWrapper : SlottedItemBase
    {
        #region construction
        /// <summary>
        /// An AttachmentWrapper creates an AttachmentSlot and attaches itself to the creature.  
        /// If it is unslotted, it removes the slot.
        /// Only itself can be slotted in the AttachmentSlot.
        /// </summary>
        public AttachmentWrapper(Creature possessor, ICoreObject coreObject, bool allowUnSlotAction, TimeType detachTime = TimeType.Total)
            : this(possessor, coreObject, new ActionTime(detachTime), allowUnSlotAction)
        {
        }

        /// <summary>
        /// An AttachmentWrapper creates an AttachmentSlot and attaches itself to the creature.  
        /// If it is unslotted, it removes the slot.
        /// Only itself can be slotted in the AttachmentSlot.
        /// </summary>
        public AttachmentWrapper(Creature possessor, ICoreObject coreObject, ActionTime detachTime, bool allowUnSlotAction)
            : base(coreObject.Name, ItemSlot.Attachment)
        {
            _UnSlotAct = allowUnSlotAction;
            _Possessor = possessor;
            _Obj = coreObject;
            _DetachTime = detachTime;
            Attach();
        }
        #endregion

        #region private data
        private ICoreObject _Obj;
        private ActionTime _DetachTime;
        private bool _UnSlotAct;
        #endregion

        /// <summary>The object wrapped by the attachment wrapper</summary>
        public override ICoreObject BaseObject
            => _Obj;

        /// <summary>ActionTime needed top detach the wrapper</summary>
        public ActionTime DetachTime => _DetachTime;

        public override double Weight { get { return _Obj == null ? 0d : _Obj.Weight; } set { base.Weight = 0; } }
        public override bool IsTransferrable => true; // TODO: perhaps this should be false?
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

        #region private AttachmentSlot Attach()
        private AttachmentSlot Attach()
        {
            // get slot
            var _slot = CreaturePossessor.Body.ItemSlots.AllSlots
                .OfType<AttachmentSlot>()
                .FirstOrDefault(_s => _s.Source == this);
            if (_slot == null)
            {
                // create slot
                _slot = new AttachmentSlot(this, _UnSlotAct);
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
            if ((slot is AttachmentSlot _attSlot) && (_attSlot.Source == this))
                return base.FinalSlotCheck(slot);
            return false;
        }
        #endregion

        #region protected override void OnSetItemSlot()
        protected override void OnSetItemSlot()
        {
            // if the held object is an action provider, it typically provides actions when held (the item will determine whether they are possible)
            if (_Obj is IActionProvider _prov)
            {
                if (!CreaturePossessor.Actions.Providers.ContainsKey(this))
                {
                    CreaturePossessor.Actions.Providers.Add(this, _prov);
                }
            }
            CreaturePossessor.ObjectLoad.Add(_Obj);
            _Obj.AddAdjunct(new Attached(this));
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
                CreaturePossessor.Actions.Providers.Remove(this);
            }

            var _myObj = _Obj;
            _Obj = null;
            CreaturePossessor.ObjectLoad.Remove(_myObj, null);

            // get rid of pathed and aspects
            var _held = _myObj.Adjuncts.OfType<Attached>().FirstOrDefault(_a => _a.AttachmentWrapper == this);
            if (_held != null)
                _held.Eject();
            var _attended = _myObj.Adjuncts.OfType<Attended>().FirstOrDefault();
            if (_attended != null)
                _attended.Eject();

            // remove the attachment slot
            var _slot = CreaturePossessor.Body.ItemSlots.AllSlots
                .OfType<AttachmentSlot>()
                .FirstOrDefault(_s => _s.Source == this);
            if (_slot != null)
            {
                CreaturePossessor.Body.ItemSlots.Remove(_slot);
            }
            base.OnClearSlots(slotA, slotB);
        }
        #endregion

        public override ActionTime SlottingTime => new ActionTime(TimeType.Free);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(TimeType.Free);
        public override bool UnslottingProvokes => false;
    }
}
