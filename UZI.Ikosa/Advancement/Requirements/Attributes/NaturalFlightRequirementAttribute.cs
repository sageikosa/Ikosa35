using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class NaturalFlightRequirementAttribute : RequirementAttribute
    {
        public NaturalFlightRequirementAttribute()
        {
        }

        public override string Name => @"Can Fly";
        public override string Description => @"Has a natural flight speed";

        public override bool MeetsRequirement(Creature creature)
            => creature?.Movements.AllMovements.OfType<FlightSuMovement>().Any(_f => _f.Source is Species)
            ?? false;

        public override RequirementMonitor CreateMonitor(IRefreshable target, Creature owner)
        {
            return null;
        }
    }
}
