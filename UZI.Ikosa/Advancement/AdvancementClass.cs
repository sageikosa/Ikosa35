using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Advancement
{
    /// <summary>Defines the public interfaces for improving creatures (adding classes and increasing level)</summary>
    [Serializable]
    public abstract class AdvancementClass : CreatureBindBase, IAdvancementClass
    {
        #region Constructors
        protected AdvancementClass(byte powerDieSize)
            : this(powerDieSize, false)
        {
        }

        protected AdvancementClass(byte powerDieSize, bool noHitDice)
        {
            CurrentLevel = 0;
            _Locked = 0;

            // track level by power dice number
            _LevelDelta = new PowerDieTrackDelta(this, ClassName);
            _EffLvl = new ConstDeltable(0);
            _EffLvl.Deltas.Add(_LevelDelta);

            _PowerDieSize = powerDieSize;
            _BaseAttackModifier = new Delta(BaseAttackBonus, this, ClassName);
            _FortitudeModifier = new Delta(FortitudeBonus, this, ClassName);
            _ReflexModifier = new Delta(ReflexBonus, this, ClassName);
            _WillModifier = new Delta(WillBonus, this, ClassName);
            UsesNoPowerDice = noHitDice;
        }
        #endregion

        #region data
        protected byte _PowerDieSize;
        private Delta _BaseAttackModifier;
        private Delta _FortitudeModifier;
        private Delta _ReflexModifier;
        private Delta _WillModifier;
        private Guid _ID = Guid.NewGuid();

        /// <summary>The underlying delta value that represents the locked class level</summary>
        protected int _Locked;

        /// <summary>Underlying count of hit dice directly attached to the class</summary>
        protected PowerDieTrackDelta _LevelDelta;

        /// <summary>Expressed level for qualified use</summary>
        protected ConstDeltable _EffLvl;
        #endregion

        /// <summary>
        /// Indicates that the advancement class is for proficiencies and traits only.  
        /// It should add a power die group, but not include any power dice.
        /// </summary>
        public bool UsesNoPowerDice { get; private set; }

        /// <summary>true if only a single fractional power die can be associated</summary>
        public virtual bool UsesOnlyFractionalPowerDie { get { return false; } }

        /// <summary>For PowerDie 1 in fractional mode, this represents the fractional value (default=1)</summary>
        public virtual decimal OptionalFraction => 1m;

        public virtual decimal SmallestFraction => 1m;

        public abstract int MaxLevel { get; }

        #region Class Skills (Enumerate and Membership checking)
        /// <summary>Best (usually) implemented as an enumerator over a static collection.</summary>
        public virtual IEnumerable<Type> ClassSkills()
        {
            yield break;
        }

        /// <summary>Scans over the ClassSkills() enumerator, and adds any provided by an adjunct</summary>
        public bool IsClassSkill(SkillBase skill)
        {
            // default listed class skills for class
            foreach (var _type in ClassSkills())
            {
                if (_type.Equals(skill.GetType()))
                {
                    return true;
                }
            }
            if (Creature != null)
            {
                // and anything added to the class by an adjunct
                foreach (var _skillProvider in Creature.Adjuncts.OfType<IClassSkills>())
                {
                    if (_skillProvider.IsClassSkill(this, skill))
                        return true;
                }

            }
            return false;
        }
        #endregion

        public abstract string ClassName { get; }

        public abstract int SkillPointsPerLevel { get; }

        #region public virtual byte PowerDieSize { get; set; }
        /// <summary>Power Die size is updateable since some templates (undead) can override PD-size retroactively and proactively.</summary>
        public virtual byte PowerDieSize
        {
            get
            {
                return _PowerDieSize;
            }
            set
            {
                // Undead templates (etc) can override PowerDieSize
                _PowerDieSize = value;
            }
        }
        #endregion

        #region PD Characteristics: BAB, Fort, Reflex, Will
        public abstract double BABProgression { get; }
        public abstract bool HasGoodFortitude { get; }
        public abstract bool HasGoodReflex { get; }
        public abstract bool HasGoodWill { get; }
        #endregion

        #region Base Values for this PD at Current Level
        /// <summary>Base Attack Bonus at locked level</summary>
        public int BaseAttackBonus
            => Convert.ToInt32(Math.Floor(LockedLevel * BABProgression));

        public int BaseAttackAt(int level)
            => Convert.ToInt32(Math.Floor(level * BABProgression));

        protected int EffectiveBaseSave(bool goodSave)
            => goodSave
            ? (LockedLevel + 4) / 2
            : LockedLevel / 3;

        /// <summary>Fortitude bonus at current level</summary>
        public int FortitudeBonus
            => EffectiveBaseSave(HasGoodFortitude);

        /// <summary>Reflex bonus at current level</summary>
        public int ReflexBonus
            => EffectiveBaseSave(HasGoodReflex);

        /// <summary>Will bonus at current level</summary>
        public int WillBonus
            => EffectiveBaseSave(HasGoodWill);
        #endregion

        /// <summary>Key for class in AdvancementClass set</summary>
        public string Key
            => GetType().FullName;

        /// <summary>ICore needs this for IActionProvider</summary>
        public Guid ID => _ID;
        public Guid PresenterID => _ID;

        #region Binding to Creature Behavior
        /// <summary>Default implementation checks if creature allows the add through the AdvancementClasses instance.</summary>
        public override bool CanAdd(Creature testCreature)
            => testCreature.Classes.CanAdd(this);

        /// <summary>
        /// Adds class to list.
        /// Adds modifiers for BAB, Fort, Reflex and Will.
        /// Adds proficiencies
        /// </summary>
        protected override void OnAdd()
        {
            // CreatureBindBase.OnAdd()
            base.OnAdd();
            Creature.Classes.Add(this);

            // proficiencies
            if (this is IArmorProficiency)
                Creature.Proficiencies.Add((IArmorProficiency)this);
            if (this is IWeaponProficiency)
                Creature.Proficiencies.Add((IWeaponProficiency)this);
            if (this is IShieldProficiency)
                Creature.Proficiencies.Add((IShieldProficiency)this);

            if (!UsesNoPowerDice)
            {
                // base-attack modifier
                Creature.BaseAttack.Deltas.Add(_BaseAttackModifier);

                // save modifiers
                Creature.FortitudeSave.Deltas.Add(_FortitudeModifier);
                Creature.ReflexSave.Deltas.Add(_ReflexModifier);
                Creature.WillSave.Deltas.Add(_WillModifier);
            }
        }
        #endregion

        #region Unbinding from Creature
        /// <summary>
        /// Default implementation checks if current and locked level are both zero
        /// </summary>
        public override bool CanRemove()
        {
            // can only be removed if the currentLevel and LockedLevel are both 0
            return (CurrentLevel == 0) && (LockedLevel == 0);
        }

        protected override void OnRemove()
        {
            base.OnRemove();
            _BaseAttackModifier.DoTerminate();
            _FortitudeModifier.DoTerminate();
            _ReflexModifier.DoTerminate();
            _WillModifier.DoTerminate();

            // proficiencies
            if (this is IArmorProficiency)
                Creature.Proficiencies.Remove((IArmorProficiency)this);
            if (this is IWeaponProficiency)
                Creature.Proficiencies.Remove((IWeaponProficiency)this);
            if (this is IShieldProficiency)
                Creature.Proficiencies.Remove((IShieldProficiency)this);

            Creature.Classes.Remove(this);
        }
        #endregion

        /// <summary>Indicates the highest level set for this class (may not be all locked)</summary>
        public int CurrentLevel { get; protected set; }

        #region Level Management
        /// <summary>Increases CurrentLevel.  Pushes a PowerDieGroup</summary>
        public void IncreaseLevel(PowerDieCalcMethod method)
        {
            if (CanIncreaseLevel())
            {
                CurrentLevel++;

                // increase PD
                Creature.AdvancementLog.Push(this, CurrentLevel, method);
                _LevelDelta.Push(Creature.AdvancementLog.Count);

                // do class-specific level-dependent features
                OnIncreaseLevel();
            }
        }

        /// <summary>Ensures nothing will veto the change.</summary>
        public virtual bool CanIncreaseLevel()
        {
            // must have room to expand, creature must have no fractional power dice, and nothing vetoes
            return (CurrentLevel < MaxLevel)
                && Creature.AdvancementLog.All(_ali => _ali.All(_pd => !_pd.IsFractional))
                && Creature.Classes.CanIncrease(this);
        }

        /// <summary>Override to add level-dependent features</summary>
        protected virtual void OnIncreaseLevel()
        {
            // NOTE: there is nothing specific in the abstract class
        }

        /// <summary>
        /// Current level must be unlocked.  Current level must >= MinLevel.  HitDice class must remove the actual hit dice AFTER calling this!  
        /// Caller should check CanRemove() and Remove if needed.
        /// </summary>
        internal void DecreaseLevel()
        {
            // should only pass this if a level of the class is at the top of the level stack
            if (CanDecreaseLevel())
            {
                CurrentLevel--;
                _LevelDelta.Pop();

                // do class-specific level-dependent features
                OnDecreaseLevel();
            }
        }

        #region public virtual bool CanDecreaseLevel()
        /// <summary>
        /// Current level cannot be locked.
        /// Current Level must be >= MinLevel.
        /// The top power die must be for this class and nothing must veto.
        /// </summary>
        public virtual bool CanDecreaseLevel()
        {
            return (LockedLevel < CurrentLevel)
                && (CurrentLevel > 0)
                && Creature.Classes.CanDecrease(this);
        }
        #endregion

        /// <summary>
        /// Override to remove level-dependent features
        /// </summary>
        protected virtual void OnDecreaseLevel()
        {
            // NOTE: there is nothing specific in the abstract class
        }
        #endregion

        /// <summary>Indicates the highest taken level locked into the creature</summary>
        public int LockedLevel => _Locked;

        /// <summary>Effective locked level (may be adjusted by locking other class levels, a lá mystic theurge)</summary>
        public IVolatileValue EffectiveLevel => _EffLvl;

        #region Level Lock/Unlock
        /// <summary>true if the level&lt;=CurrentLevel and level>=LockedLevel+1</summary>
        public virtual bool CanLockLevel(int level)
        {
            // let the PowerDice worry about PD locking
            return ((level <= CurrentLevel) && (level >= LockedLevel + 1)); // == or >= ?
        }

        public virtual IEnumerable<AdvancementRequirement> Requirements(int level) { yield break; }
        protected virtual void OnLockOneLevel() { }
        protected virtual void OnUnlockOneLevel() { }

        public virtual bool CanUnlockOneLevel(int level)
            => (level > 0);

        #region internal void LockOneLevel()
        /// <summary>
        /// Increases lockedLevel, updates modifier values and calls virtual extension function.
        /// Class levels may be interspersed with each other, so locking internally
        /// </summary>
        internal void LockOneLevel()
        {
            if (CanLockLevel(LockedLevel + 1))
            {
                _Locked++;

                // coordinate modifiers
                if (_BaseAttackModifier.Value != BaseAttackBonus)
                {
                    _BaseAttackModifier.Value = BaseAttackBonus;
                }

                if (_FortitudeModifier.Value != FortitudeBonus)
                {
                    _FortitudeModifier.Value = FortitudeBonus;
                }

                if (_ReflexModifier.Value != ReflexBonus)
                {
                    _ReflexModifier.Value = ReflexBonus;
                }

                if (_WillModifier.Value != WillBonus)
                {
                    _WillModifier.Value = WillBonus;
                }

                OnLockOneLevel();
            }
        }
        #endregion

        #region internal void UnlockOneLevel()
        /// <summary>
        /// Steps back all modifiers to previous locked level.  Removes any class features.  Does NOT remove power die!
        /// Class levels may be interspersed with each other, so rolling back internally
        /// </summary>
        internal void UnlockOneLevel()
        {
            if (CanUnlockOneLevel(LockedLevel))
            {
                _Locked--;

                // coordinate modifiers
                if (_BaseAttackModifier.Value != BaseAttackBonus)
                {
                    _BaseAttackModifier.Value = BaseAttackBonus;
                }

                if (_FortitudeModifier.Value != FortitudeBonus)
                {
                    _FortitudeModifier.Value = FortitudeBonus;
                }

                if (_ReflexModifier.Value != ReflexBonus)
                {
                    _ReflexModifier.Value = ReflexBonus;
                }

                if (_WillModifier.Value != WillBonus)
                {
                    _WillModifier.Value = WillBonus;
                }

                OnUnlockOneLevel();
            }
        }
        #endregion

        #endregion

        public virtual IEnumerable<Language> CommonLanguages() { yield break; }
        public virtual IEnumerable<IFeature> Features(int level) { yield break; }

        public ClassInfo ToClassInfo()
            => this.GetClassInfo();
    }
}
