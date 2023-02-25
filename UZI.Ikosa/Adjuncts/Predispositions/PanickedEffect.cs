using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Drops and must flee</summary>
    [Serializable]
    public class PanickedEffect : FrightenedEffect
    {
        public PanickedEffect(object source)
            : base(source)
        {
        }

        protected override string ConditionString => Condition.Panicked;

        protected override void OnActivate(object source)
        {
            Creature _critter = Anchor as Creature;
            if (_critter != null)
            {
                var _located = _critter.GetLocated();
                if (_located != null)
                {
                    var _procMan = _located.Locator.MapContext.ContextSet.ProcessManager;
                    foreach (var _slot in _critter.Body.ItemSlots.AllSlots.OfType<HoldingSlot>().Where(_is => _is.SlottedItem != null))
                    {
                        // go right to the holding slots...
                        var _act = new CoreActivity(_critter, new DropHeldObject(_slot, string.Empty), null);
                        _procMan.StartProcess(_act);
                    }
                }
            }
            base.OnActivate(source);
        }

        public override string Description => @"Panicked (must flee)";
    }
}
