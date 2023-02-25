using Uzi.Core.Contracts;
using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Visualize;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class DropStep : CoreStep
    {
        #region ctor()
        public DropStep(CoreActivity activity, HoldingSlot slot, ICellLocation location, bool gently)
            : base(activity)
        {
            _Slot = slot;
            _Loc = location;
            _Critter = null;
            _Obj = null;
            _Gently = gently;
        }

        public DropStep(CoreStep predecessor, HoldingSlot slot, ICellLocation location)
            : base(predecessor)
        {
            _Slot = slot;
            _Loc = location;
            _Critter = null;
            _Obj = null;
        }

        public DropStep(CoreStep predecessor, Creature creature, ICoreObject target, ICellLocation location, bool gently)
            : base(predecessor)
        {
            _Slot = null;
            _Loc = location;
            _Critter = creature;
            _Obj = target;
            _Gently = gently;
        }
        #endregion

        #region data
        private HoldingSlot _Slot;
        private ICellLocation _Loc;
        private bool _Gently;
        private Creature _Critter;
        private ICoreObject _Obj;
        #endregion

        public HoldingSlot HoldingSlot =>
            _Slot
            ?? (_Critter?.Body.ItemSlots.SlotForItem(_Obj) as HoldingSlot);

        public ICellLocation CellLocation => _Loc;
        public CoreActivity Activity => Process as CoreActivity;
        public bool DropGently => _Gently;

        protected override bool OnDoStep()
        {
            var _slot = HoldingSlot;
            var _item = _slot?.SlottedItem;
            if (_item != null)
            {
                var _baseObj = _item.BaseObject;
                var _info = GetInfoData.GetInfoFeedback(_baseObj, _slot.Creature);
                _item.ClearSlots();
                if (_item.MainSlot == null)
                {
                    // drop interaction
                    var _dropData = new Drop(_slot.Creature, _slot.Creature.Setting as LocalMap, CellLocation, DropGently);
                    var _interact = new Interaction(_slot.Creature, _slot, _baseObj, _dropData);
                    _baseObj.HandleInteraction(_interact);

                    #region feedback
                    // feedback
                    if (_info != null)
                    {
                        PreEmptStatus(Activity.Actor.ID,
                            //new RefreshNotify { Message = @"Dropped", Items = true, Creature = true, Awarenesses = true }, 
                            //_info);
                            new RefreshNotify(true, false, true, true, false));
                    }
                    else
                    {
                        PreEmptStatus(Activity.Actor.ID,
                            //new RefreshNotify { Message = @"Dropped", Items = true, Creature = true, Awarenesses = true });
                            new RefreshNotify(true, false, true, true, false));
                    }
                    #endregion
                }
                else
                {
                    #region feedback
                    // feedback
                    if (_info != null)
                    {
                        Activity?.Terminate(new Description(@"Dropped", @"Could not drop"), _info);
                    }
                    else
                    {
                        Activity?.Terminate(@"Could not drop");
                    }
                    #endregion
                }
            }
            else
            {
                Activity?.Terminate($@"Nothing to Drop from {HoldingSlot.SubType}");
            }
            return true;
        }

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        public override bool IsDispensingPrerequisites
            => false;
    }
}
