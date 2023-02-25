using System;
using Uzi.Core;

namespace Uzi.Ikosa
{
    /// <summary>
    /// Flag strings for the conditions collection.  
    /// Conditions should be added by effects, spells or abilities, as these can implement most of the effects.
    /// </summary>
    [Serializable]
    public class Condition : ISourcedObject
    {
        #region Common Condition Flags
        public readonly static string AbilityDamaged = @"Ability Damaged";
        public readonly static string AbilityDrained = @"Ability Drained";
        public readonly static string Blinded = @"Blinded";
        public readonly static string BlownAway = @"Blown Away";
        public readonly static string Checked = @"Checked";
        public readonly static string Confused = @"Confused";
        public readonly static string Cowering = @"Cowering";
        public readonly static string Dazed = @"Dazed";
        public readonly static string Dazzled = @"Dazzled";
        public readonly static string Dead = @"Dead";
        public readonly static string Deafened = @"Deafened";
        public readonly static string Disabled = @"Disabled";
        public readonly static string Dying = @"Dying";
        public readonly static string DyingHard = @"Dying Hard";
        public readonly static string EnergyDrained = @"Energy Drained";
        public readonly static string Entangled = @"Entangled";
        public readonly static string Exhausted = @"Exhausted";
        public readonly static string Fascinated = @"Fascinated";
        public readonly static string Fatigued = @"Fatigued";
        public readonly static string Frightened = @"Frightened";
        public readonly static string Grappling = @"Grappling";
        public readonly static string Helpless = @"Helpless";
        public readonly static string Incorporeal = @"Incorporeal";
        public readonly static string Invisible = @"Invisible";
        public readonly static string KnockedDown = @"KnockedDown";
        public readonly static string Nauseated = @"Nauseated";
        public readonly static string NotHealing = @"Not Healing";
        public readonly static string Panicked = @"Panicked";
        public readonly static string Paralyzed = @"Paralyzed";
        public readonly static string Petrified = @"Petrified";
        public readonly static string Pinned = @"Pinned";
        public readonly static string Prone = @"Prone";
        public readonly static string Repulsed = @"Repulsed";
        public readonly static string Shaken = @"Shaken";
        public readonly static string Sickened = @"Sickened";
        public readonly static string SingleAction = @"Single Actions Only";
        public readonly static string Stable = @"Stable";
        public readonly static string Staggered = @"Staggered";
        public readonly static string Stunned = @"Stunned";
        public readonly static string Unconscious = @"Unconscious";
        /// <summary>Cannot react to opportunistic activities</summary>
        public readonly static string UnpreparedForOpportunities = @"Unprepared(Opportunities)";
        /// <summary>Cannot dodge, does not get Max Dexterity to Armor Rating</summary>
        public readonly static string UnpreparedToDodge = @"Unprepared(Dodge)";

        // breathing flags
        public readonly static string RecoveringBreath = @"Recovering Breath";
        public readonly static string HoldingBreath = @"Holding Breath";
        public readonly static string Drowning = @"Drowning";
        #endregion

        #region Construction
        public Condition(string name, object source)
        {
            Name = name;
            Source = source;
            Extra = string.Empty;
        }

        public Condition(string name, object source, string extra)
        {
            Name = name;
            Source = source;
            Extra = extra;
        }
        #endregion

        public string Name { get; private set; }
        public object Source { get; private set; }
        public string Extra { get; private set; }
        public string Display
            => (Extra.Length > 0)
            ? $@"{Name} ({Extra})"
            : Name;
    }
}