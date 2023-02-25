using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Items
{
    /// <summary>Allows a non-slotted item (or something designed for a different slot) to be held</summary>
    [Serializable]
    public class HoldingWrapper : SlottedItemBase
    {
        /// <summary>Allows a non-slotted item to be held</summary>
        public HoldingWrapper(Creature possessor, ICoreObject coreObject)
            : base(coreObject.Name, ItemSlot.HoldingSlot)
        {
            _Possessor = possessor;
            _Obj = coreObject;
        }

        private ICoreObject _Obj;

        /// <summary>The object wrapped by the holding wrapper</summary>
        public override ICoreObject BaseObject => _Obj;

        public override double Weight { get { return _Obj == null ? 0d : _Obj.Weight; } set { base.Weight = 0; } }

        public override bool IsTransferrable
            // TODO: perhaps this should be false?
            => true;

        protected override void OnPossessorChanged()
        {
            if (_Obj is ICoreItem)
            {
                (_Obj as ICoreItem).Possessor = Possessor;
            }
            base.OnPossessorChanged();
        }

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
            _Obj.AddAdjunct(new Held(this));
            _Obj.AddAdjunct(new Attended(CreaturePossessor));
            base.OnSetItemSlot();
        }

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

            _myObj.Adjuncts.OfType<Held>().FirstOrDefault(_h => _h.HoldingWrapper == this)?.Eject();
            _myObj.Adjuncts.OfType<Attended>().FirstOrDefault()?.Eject();
            base.OnClearSlots(slotA, slotB);
        }

        protected override string ClassIconKey
            => string.Empty;

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Free);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Free);
        public override bool UnslottingProvokes => false;

        public override IEnumerable<string> IconKeys
            => (BaseObject as ItemBase)?.IconKeys ?? new string[] { };
    }
}
