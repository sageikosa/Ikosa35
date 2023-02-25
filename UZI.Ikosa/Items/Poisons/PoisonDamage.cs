using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// Abstract base for poison damage
    /// </summary>
    [Serializable]
    public abstract class PoisonDamage
    {
        public PoisonDamage()
        {
        }

        public abstract string Name { get; }

        /// <summary>Let the system determine what the damage is based (and return a description)</summary>
        public abstract IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter);

        /// <summary>Supply roll values</summary>
        public abstract IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter, int[] rollValue);

        public abstract IEnumerable<PoisonRoll> GetRollers();
    }

    public class PoisonRoll
    {
        public string Key { get; set; }
        public Roller Roller { get; set; }
        public string Name { get; set; }
    }
}
