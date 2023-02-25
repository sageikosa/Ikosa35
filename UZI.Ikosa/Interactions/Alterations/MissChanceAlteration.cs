using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Flag alteration so miss chance isn't rolled more than once</summary>
    [Serializable]
    public class MissChanceAlteration : InteractionAlteration
    {
        /// <summary>Flag alteration so miss chance isn't rolled more than once</summary>
        public MissChanceAlteration(InteractData interact, object source, int percentRolled)
            : base(interact, source)
        {
            PercentRolled = percentRolled;
            SecondRoll = false;
        }

        /// <summary>Maximum miss chance tested against</summary>
        public int PercentRolled { get; private set; }
        /// <summary>Indicates whether a second roll was already made</summary>
        public bool SecondRoll { get; private set; }

        public void SecondChance(int newScore)
        {
            PercentRolled = newScore;
            SecondRoll = true;
        }

        public override IEnumerable<Info> Information
        {
            get
            {
                yield return new Info { Message = $@"Miss Chance: {PercentRolled}" };
                yield break;
            }
        }

        public static MissChanceAlteration GetMissChance(Interaction workSet, object newSource)
        {
            var _miss = workSet.InteractData.Alterations.OfType<MissChanceAlteration>().FirstOrDefault();
            if (_miss != null)
            {
                return _miss;
            }
            else
            {
                // roll miss chance
                var _score = DieRoller.RollDie(workSet.Actor?.ID ?? Guid.Empty, 100, @"Miss chance", @"Miss chance");
                _miss = new MissChanceAlteration(workSet.InteractData, newSource, _score);
                workSet.InteractData.Alterations.Add(workSet.Target, _miss);
                return _miss;
            }
        }
    }
}
