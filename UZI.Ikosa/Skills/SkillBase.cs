using System;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Adjuncts;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Uzi.Core.Contracts;
using System.Linq;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Skills
{
    // TODO: consider creating a base class that forks into SkillBase and AbilityCheckBase (which cannot have ranks added)
    [Serializable]
    public abstract class SkillBase : Deltable, ICreatureBound, ISupplyQualifyDelta, IDeserializationCallback
    {
        #region ctor()
        // assumes the skillUser's abilities are already bound
        protected SkillBase(Creature skillUser)
            : base(0)
        {
            // track user, add modifiers
            _Creature = skillUser;
            if (KeyAbilityMnemonic != string.Empty)
            {
                Deltas.Add(skillUser.Abilities[KeyAbilityMnemonic]);
                Deltas.Add((IQualifyDelta)skillUser.ExtraSkillCheck);
            }
            if (CheckFactor > 0d)
            {
                // encumberance floats as weight changes (armor/shield is tied into this)
                Deltas.Add(skillUser.EncumberanceCheck);
            }
        }
        #endregion

        #region data
        [NonSerialized, JsonIgnore]
        private SkillInfoAttribute _Info = null;

        private Guid _ID = Guid.NewGuid();
        protected Creature _Creature;
        #endregion

        #region public virtual bool IsClassSkillAtPowerLevel(int powerLevel)
        /// <summary>Determines whether the skill is a class skill at the power level</summary>
        public virtual bool IsClassSkillAtPowerLevel(int powerLevel)
        {
            if (Creature.AdvancementLog.NumberPowerDice >= powerLevel)
            {
                return Creature.AdvancementLog[powerLevel].AdvancementClass.IsClassSkill(this);
            }

            // nope
            return false;
        }
        #endregion

        /// <summary>Any class has this as a class skill</summary>
        public virtual bool IsClassSkill
            => Creature.Classes.IsClassSkill(this);

        #region public static SkillInfoAttribute SkillInfo(Type skillType)
        public static SkillInfoAttribute SkillInfo(Type skillType)
        {
            try
            {
                var _attr = skillType.GetCustomAttributes(typeof(SkillInfoAttribute), false);
                if (_attr.Length != 0)
                    return (SkillInfoAttribute)_attr[0];
            }
            catch
            {
                return null;
            }
            return new SkillInfoAttribute(skillType.Name, string.Empty);
        }
        #endregion

        public Guid ID => _ID;
        public Creature Creature => _Creature;
        public Guid PresenterID => _ID;

        private SkillInfoAttribute GetInfoAttrib()
            => _Info ??= SkillBase.SkillInfo(GetType());

        public virtual string SkillName
            => GetInfoAttrib().Name;

        public virtual string KeyAbilityMnemonic
            => GetInfoAttrib().Mnemonic;

        public virtual bool UseUntrained
            => GetInfoAttrib().UseUntrained;

        public virtual double CheckFactor
            => GetInfoAttrib().CheckFactor;

        public bool IsTrained
            => BaseValue > 0;

        public double SkillRanksAtPowerLevel(int powerLevel)
            => Creature.AdvancementLog.SkillRanks(this, powerLevel);

        /// <summary>Returns true if a skill check matches or beats the difficulty</summary>
        public bool SkillCheck(int difficulty, ICore opposed, int rollValue) 
        {
            // inform master of difficulty
            Deltable.GetDeltaCalcNotify(null, $@"{SkillName} Check Difficulty").DeltaCalc.SetBaseResult(difficulty);

            return
                CheckValue(new Qualifier(Creature, opposed, Creature), rollValue, Deltable.GetDeltaCalcNotify(Creature.ID, $@"{SkillName} Check").DeltaCalc)
                >= difficulty;
        }

        /// <summary>Returns true if an automatic skill check matches or beats the difficulty</summary>
        public bool AutoCheck(int difficulty, ICore opposed)
            => SkillCheck(difficulty, opposed, DiceRoller.RollDie(Creature.ID, 20, SkillName, $@"{SkillName} Check"));

        /// <summary>Effective check value, given a pre-determined roll.</summary>
        public int? CheckValue(Qualifier workSet, int rollValue, DeltaCalcInfo info)
        {
            // assume deltaCalcInfo is "clean"
            info.SetBaseResult(rollValue);

            var _deltas = QualifiedDeltas(workSet).Where(_d => _d.Value != 0).ToList();
            foreach (var _del in _deltas)
            {
                info.AddDelta(_del.Name, _del.Value);
            }
            info.Result = rollValue + _deltas.Sum(_c => _c.Value);
            return info.Result;
        }

        // TODO: non-proficient with armor check penalty (all str/dex skills affected)?

        public static int MaxClassSkillRanksAtPowerLevel(int powerLevel)
            => powerLevel + 3;

        public static double MaxCrossClassSkillRanksAtPowerLevel(int powerLevel)
            => (MaxClassSkillRanksAtPowerLevel(powerLevel) / 2.0d);

        // ISupplyQualifyDelta Members
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => QualifiedDeltas(qualify, GetType(), SkillName);

        public SkillInfo ToSkillInfo(Creature critter)
        {
            var _info = ToInfo<SkillInfo>(null);
            _info.Message = SkillName;
            _info.IsClassSkill = IsClassSkill;
            _info.KeyAbilityMnemonic = KeyAbilityMnemonic;
            _info.UseUntrained = UseUntrained;
            _info.IsTrained = IsTrained;
            _info.CheckFactor = CheckFactor;
            var _expiry = critter.GetTake10Remaining(GetType());
            _info.Take10 = _expiry != null
                ? new Take10Info { RemainingRounds = _expiry.Value }
                : null;
            return _info;
        }

        public void OnDeserialization(object sender)
        {
            // can add support for IActionProviders after skills have already been saved
            if ((this is IActionProvider _provider)
                && !Creature.Actions.Providers.ContainsKey(this))
                Creature.Actions.Providers.Add(this, _provider);
        }
    }
}
