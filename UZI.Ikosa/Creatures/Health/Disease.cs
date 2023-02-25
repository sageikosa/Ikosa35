using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa
{
    // NOTE: supernatural diseases will implement IMagicAura
    [Serializable]
    public class Disease
    {
        [Flags]
        public enum InfectionVector { Ingested = 1, Inhaled = 2, Injury = 4, Contact = 8 };

        public Disease(string name, PoisonDamage damage, Roller incubation, TimeUnit incubationUnitFactor,
            int? savesToRecover, IVolatileValue difficulty, InfectionVector vector)
        {
            _Name = name;
            _Damage = damage;
            _Roller = incubation;
            _UnitFactor = incubationUnitFactor;
            _Saves = savesToRecover;
            _Difficulty = difficulty;
            _Vector = vector;
        }

        #region data
        private string _Name;
        private PoisonDamage _Damage;
        private InfectionVector _Vector;
        private Roller _Roller;
        private TimeUnit _UnitFactor;
        private int? _Saves;
        private IVolatileValue _Difficulty;
        #endregion

        public string Name => _Name;
        public PoisonDamage Damage => _Damage;
        public IVolatileValue Difficulty => _Difficulty;
        public InfectionVector Infection => _Vector;

        /// <summary>How long until first damage affects</summary>
        public Roller IncubationRoller => _Roller;

        /// <summary>Time unit factor for incubation</summary>
        public TimeUnit IncubationUnitFactor => _UnitFactor;

        /// <summary>Number of successful saves in a row until healed (default 2, null if never)</summary>
        public int? SaveSuccessToHeal => _Saves;

        public IEnumerable<PoisonRoll> GetDamageRollers()
            => Damage.GetRollers();

        public IEnumerable<DamageInfo> ApplyDamage(CoreStep step, Creature critter)
            => Damage.ApplyDamage(this, step, critter);

        public IEnumerable<DamageInfo> ApplyDamage(CoreStep step, Creature critter, int[] rollValue)
            => Damage.ApplyDamage(this, step, critter, rollValue);
    }

    public interface IDiseaseProvider
    {
        Disease GetDisease();
    }
}
