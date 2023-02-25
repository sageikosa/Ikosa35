using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Fidelity
{
    /// <summary>Used to group all commanded creatures and the commander together.</summary>
    [Serializable]
    public class CommandGroup : AdjunctGroup
    {
        /// <summary>Used to group all commanded creatures and the commander together.</summary>
        public CommandGroup(OverwhelmCreatures overwhelm)
            : base(overwhelm)
        {
        }

        /// <summary>Adjunct for the commander of the group</summary>
        public CommandMaster Commander { get { return Members.OfType<CommandMaster>().FirstOrDefault(); } }
        public OverwhelmCreatures OverwhelmCreatures { get { return Source as OverwhelmCreatures; } }

        public decimal CurrentPowerDice
        {
            get
            {
                return (from _cmdCritter in Members.OfType<CommandedCreature>()
                        where (_cmdCritter.Creature != null) // NOTE: *should* always be true
                        select _cmdCritter.Creature.AdvancementLog.PowerDiceCount).Sum();
            }
        }

        public IEnumerable<Creature> CommandedCreatures
        {
            get
            {
                foreach (var _cmdCritter in Members.OfType<CommandedCreature>())
                    yield return _cmdCritter.Creature;
                yield break;
            }
        }

        public CommandedCreature this[Creature creature]
        {
            get { return Members.OfType<CommandedCreature>().FirstOrDefault(_cc => _cc.Creature == creature); }
        }

        // TODO: maybe not?
        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();
    }
}