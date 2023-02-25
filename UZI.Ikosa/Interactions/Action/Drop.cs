using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using System.Collections.Generic;
using Uzi.Ikosa.Tactical;
using System.Linq;

namespace Uzi.Ikosa.Interactions.Action
{
    public class Drop : InteractData
    {
        /// <summary>Drop at a location</summary>
        public Drop(CoreActor actor, LocalMap map, ICellLocation location, bool gently)
            : base(actor)
        {
            Location = location;
            Surface = null;
            DropGently = gently;
            Map = map;
        }

        /// <summary>Drop onto a surface</summary>
        public Drop(CoreActor actor, LocalMap map, ICellLocation location, bool gently, IMoveAlterer surface)
            : base(actor)
        {
            Location = location;
            Surface = surface;
            DropGently = gently;
            Map = map;
        }

        public bool DropGently { get; set; }
        public ICellLocation Location { get; set; }
        public IMoveAlterer Surface { get; set; }
        public LocalMap Map { get; set; }

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return new DropHandler();
            yield break;
        }

        public static void DoDrop(Creature critter, ICoreObject coreObj, object source, bool gently)
        {
            var _loc = critter?.GetLocated();
            if (_loc != null)
            {
                var _location = new CellLocation(_loc.Locator.Location);
                var _drop = new Drop(critter, _loc.Locator.Map, new Cubic(_location, GeometricSize.UnitSize()), gently);
                var _iAct = new Interaction(critter, source, coreObj, _drop);
                coreObj.HandleInteraction(_iAct);
            }
        }

        /// <summary>Source is used as the interaction source and for locating the drop location and setting</summary>
        public static void DoDropEject(ICoreObject source, ICoreObject target)
        {
            var _loc = source?.GetLocated();
            if (_loc != null)
            {
                var _location = new CellLocation(_loc.Locator.Location);
                var _drop = new Drop(null, _loc.Locator.Map, new Cubic(_location, GeometricSize.UnitSize()), false);
                var _iAct = new Interaction(null, source, target, _drop);
                target.HandleInteraction(_iAct);
            }
        }

        public static void DoDropFromEthereal(ICellLocation location, ICoreObject source, ICoreObject target)
        {
            var _loc = source?.GetLocated();
            if (_loc != null)
            {
                var _location = new CellLocation(location);
                var _drop = new Drop(null, _loc.Locator.Map, new Cubic(_location, GeometricSize.UnitSize()), false);
                var _iAct = new Interaction(null, source, target, _drop);
                target.HandleInteraction(_iAct);
            }
        }
    }
}
