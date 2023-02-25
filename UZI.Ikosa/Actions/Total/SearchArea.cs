using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class SearchArea : ActionBase
    {
        public SearchArea(IActionSource powerLevel, ActionTime cost, string orderKey)
            : base(powerLevel, cost, true, false, orderKey)
        {
        }

        // TODO: find secret door (Difficulty=20+; total)...auto-search same cell (elf)/wider for dwarves and stone
        // TODO: find simple traps (Difficulty=20; total)...dwarves auto search stonework traps
        // TODO: 'rogue' find traps (Difficulty=21+; total)...dwarves auto search difficult stonework traps
        // TODO: 'rogue' find traps (Difficulty=25+; total)...dwarves auto search magical stonework traps
        // TODO: find item in horde or chest (Difficulty = 10; total)

        public override string Key => @"Search.Area";
        public override string DisplayName(CoreActor actor) => @"Search Area";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new LocationAim(@"Location", @"Location to search", Visualize.LocationAimMode.Cell,
                FixedRange.One, FixedRange.One, new MeleeRange());
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Search", activity.Actor, observer);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _locTarget = activity.Targets.OfType<LocationTarget>().FirstOrDefault();
            if ((activity.Actor is Creature _critter)
                && (_locTarget != null))
            {
                // searchable objects in locators in region
                activity.EnqueueRegisterPreEmptively(Budget);

                // widen search area to find nearby secret things
                var _cell = new CellLocation(_locTarget.Location);
                IEnumerable<ICellLocation> _outermostCells()
                {
                    yield return _cell;
                    yield return _cell.Add(1, 0, 0);
                    yield return _cell.Add(-1, 0, 0);
                    yield return _cell.Add(0, 1, 0);
                    yield return _cell.Add(0, -1, 0);
                    yield return _cell.Add(0, 0, 1);
                    yield return _cell.Add(0, 0, -1);
                }

                var _planar = _critter.Senses.PlanarPresence;
                var _cells = new CellList(_outermostCells(), 0, 0, 0);
                foreach (var _searchable in from _tLoc in _locTarget.MapContext.LocatorsInRegion(_cells, _planar)
                                            from _obj in _tLoc.AllConnectedOf<ICoreObject>()
                                            where _obj.HasActiveAdjunct<Searchable>()
                                            select _obj)
                {
                    // roll a d20
                    var _roll = new Deltable(DieRoller.RollDie(_critter.ID, 20, $@"{_critter.Name} Search", @"Search check"));

                    // add in search skill
                    _roll.Deltas.Add(new SoftQualifiedDelta(_critter.Skills.Skill<SearchSkill>()));

                    // perform search
                    _searchable.HandleInteraction(
                        new Interaction(activity.Actor, _critter, _searchable, new SearchData(_critter, _roll, false)));
                }
                return null;
            }
            return activity.GetActivityResultNotifyStep(@"Invalid targets");
        }
    }
}
