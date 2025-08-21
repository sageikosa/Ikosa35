using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public abstract class BaseMonsterClass : AdvancementClass, IControlChange<int>, IPowerClass
    {
        #region construction
        /// <summary>
        /// Construct BaseMonsterClass
        /// </summary>
        /// <param name="powerDie">health-point progression</param>
        /// <param name="maxLevel">highest number of power dice</param>
        /// <param name="sizeRanges">set of base sizes by power dice count</param>
        /// <param name="optionalFraction">1 if fractional power die not supported, otherwise the fraction for fractional</param>
        /// <param name="smallestFraction">if different than optional fraction, multiple fractions possible</param>
        /// <param name="flexibleAspect">true if creature can be long or tall</param>
        protected BaseMonsterClass(byte powerDie, int maxLevel, IEnumerable<SizeRange> sizeRanges,
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(powerDie)
        {
            _MaxLevel = maxLevel;
            _SizeRanges = new List<SizeRange>(sizeRanges.ToList());
            _ValueCtrl = new ChangeController<int>(this, 0);
            _OptionalFraction = optionalFraction;
            _SmallestFraction = smallestFraction;
            _IsLong = false;
            _FlexAspect = flexibleAspect;
            _PowerLevel = new DeltableQualifiedDelta(0, @"Class Power Level", this);
            _PowerLevel.Deltas.Add(_LevelDelta);
        }
        #endregion

        #region private data
        // once set, the size range definition is not visible outside the class
        // most sizeRange collections are provided by static members that should not be changed
        private List<SizeRange> _SizeRanges = null;
        protected int _MaxLevel;
        private decimal _OptionalFraction;
        private decimal _SmallestFraction;
        private bool _IsLong;
        private bool _FlexAspect;
        private DeltableQualifiedDelta _PowerLevel;
        #endregion

        public virtual string ClassIconKey => $@"{ClassName}_class";

        public IEnumerable<SizeRange> SizeRanges()
            => _SizeRanges.Select(_sr => _sr);

        public override decimal OptionalFraction => _OptionalFraction;

        public override decimal SmallestFraction => _SmallestFraction;

        #region public bool IsFractional
        public bool IsFractional
        {
            get
            {
                return Creature.AdvancementLog[this, 1].PowerDie.IsFractional;
            }
            set
            {
                if ((OptionalFraction < 1m)
                    && !UsesOnlyFractionalPowerDie
                    && (CurrentLevel == 1))
                {
                    Creature.AdvancementLog[this, 1].PowerDie.IsFractional = value;
                    StabilizeSize();
                }
            }
        }
        #endregion

        #region public bool IsSmallestFractional
        public bool IsSmallestFractional
        {
            get
            {
                return Creature.AdvancementLog[this, 1].PowerDie.IsSmallestFractional;
            }
            set
            {
                if ((SmallestFraction < OptionalFraction)
                    && !UsesOnlyFractionalPowerDie
                    && (CurrentLevel == 1))
                {
                    Creature.AdvancementLog[this, 1].PowerDie.IsSmallestFractional = value;
                    StabilizeSize();
                }
            }
        }
        #endregion

        #region public bool IsLong
        public bool IsLong
        {
            get
            {
                return _IsLong;
            }
            set
            {
                if (_FlexAspect && (CurrentLevel == 1) && (value != _IsLong))
                {
                    _IsLong = value;
                    StabilizeSize();
                }
            }
        }
        #endregion

        public override int MaxLevel => _MaxLevel;

        #region public override IEnumerable<AdvancementRequirement> Requirements(int level)
        public override IEnumerable<AdvancementRequirement> Requirements(int level)
        {
            // fractional...
            if ((OptionalFraction < 1m)
                && !UsesOnlyFractionalPowerDie
                && (level == 1)
                && (CurrentLevel == 1))
            {
                yield return new AdvancementRequirement(new RequirementKey(@"Fractional"), @"1st PD",
                    @"Determines whether first power die is full or partial",
                    FractionalSupplier, FractionalSetter, FractionalChecker)
                {
                    CurrentValue = new Feature(
                            IsFractional ? (IsSmallestFractional ? @"Smallest" : @"Fractional") : @"Complete",
                            IsFractional ? @"Partial Power Die" : @"Full Power Die")
                };
            }

            // is Long...
            if (_FlexAspect && (level == 1))
            {
                yield return new AdvancementRequirement(new RequirementKey(@"Long"), @"Long instead of Tall",
                    @"determines whether to use the long versus tall reach profile by size",
                    AspectSupplier, AspectSetter, AspectChecker)
                {
                    CurrentValue = new Feature(
                            IsLong ? @"Long" : @"Tall",
                            IsLong ? @"Uses long reach profile" : @"Uses tall reach profile")
                };
            }

            // just in case stuff gets added later
            foreach (var _req in base.Requirements(level))
            {
                yield return _req;
            }

            yield break;
        }
        #endregion

        #region public override IEnumerable<IFeature> Features(int level)
        public override IEnumerable<IFeature> Features(int level)
        {
            if ((level == 1) && IsFractional)
            {
                if (IsSmallestFractional)
                {
                    yield return new Feature($@"Fractional: {SmallestFraction}", @"Counts as partial power die, and has fewer health points");
                }
                else
                {
                    yield return new Feature($@"Fractional: {OptionalFraction}", @"Counts as partial power die, and has fewer health points");
                }
            }

            if ((level == 1) && _FlexAspect)
            {
                yield return new Feature(
                    IsLong ? @"Long" : @"Tall",
                    IsLong ? @"Uses long reach profile" : @"Uses tall reach profile");
            }

            // just in case base has some stuff
            foreach (var _feature in base.Features(level))
            {
                yield return _feature;
            }

            yield break;
        }
        #endregion

        #region private IEnumerable<IAdvancementOption> FractionalSupplier(IResolveRequirement target, RequirementKey key)
        private IEnumerable<IAdvancementOption> FractionalSupplier(IResolveRequirement target, RequirementKey key)
        {
            if (SmallestFraction < OptionalFraction)
            {
                yield return new AdvancementParameter<byte>(target, @"Smallest", @"Smallest Power Die", 2);
            }

            yield return new AdvancementParameter<byte>(target, @"Fractional", @"Partial Power Die", 1);
            yield return new AdvancementParameter<byte>(target, @"Complete", @"Full Power Die", 0);
            yield break;
        }
        #endregion

        #region private bool FractionalSetter(RequirementKey key, IAdvancementOption advOption)
        private bool FractionalSetter(RequirementKey key, IAdvancementOption advOption)
        {
            if (advOption is AdvancementParameter<byte> _fractionalOption)
            {
                switch (_fractionalOption.ParameterValue)
                {
                    case 2:
                        IsSmallestFractional = true;

                        break;
                    case 1:
                        IsFractional = true;
                        IsSmallestFractional = false;
                        break;

                    default:
                        IsFractional = false;
                        break;
                }
                return true;
            }
            return false;
        }
        #endregion

        private bool FractionalChecker(RequirementKey key)
            => true;

        #region private IEnumerable<IAdvancementOption> AspectSupplier(IResolveRequirement target, RequirementKey key)
        private IEnumerable<IAdvancementOption> AspectSupplier(IResolveRequirement target, RequirementKey key)
        {
            yield return new AdvancementParameter<bool>(target, @"Long", @"Use long reach profile", true);
            yield return new AdvancementParameter<bool>(target, @"Tall", @"Use tall reach profile", false);
            yield break;
        }
        #endregion

        #region private bool AspectSetter(RequirementKey key, IAdvancementOption advOption)
        private bool AspectSetter(RequirementKey key, IAdvancementOption advOption)
        {
            if (advOption is AdvancementParameter<bool> _aspectOption)
            {
                IsLong = _aspectOption.ParameterValue;
                return true;
            }
            return false;
        }
        #endregion

        private bool AspectChecker(RequirementKey key)
            => _FlexAspect;

        #region private void StepUpSize(Size newSize)
        private void StepUpSize(Size newSize)
        {
            // get target range
            var _targetRange = _SizeRanges.FirstOrDefault(_sr => _sr.CreatureSize.Order == newSize.Order);

            if (_targetRange != null)
            {
                // add changes
                Creature.Abilities.Dexterity.BaseValue += _targetRange.DexterityUp;
                Creature.Abilities.Strength.BaseValue += _targetRange.StrengthUp;
                Creature.Abilities.Constitution.BaseValue += _targetRange.ConstitutionUp;
                Creature.Body.NaturalArmor.BaseValue += _targetRange.NaturalArmorUp(Creature.Body.NaturalArmor.BaseValue);
                Creature.Body.ReachSquares.BaseValue = _targetRange.NaturalReach(_IsLong);
            }
        }
        #endregion

        #region private void StepDownSize(Size newSize)
        private void StepDownSize(Size newSize)
        {
            // get previous range
            var _sourceRange = _SizeRanges.FirstOrDefault(_sr => _sr.CreatureSize.Order == (newSize.Order + 1));

            // and new range
            var _targetRange = _SizeRanges.FirstOrDefault(_sr => _sr.CreatureSize.Order == newSize.Order);

            if ((_sourceRange != null) && (_targetRange != null))
            {
                // subtract changes
                Creature.Abilities.Dexterity.BaseValue -= _sourceRange.DexterityUp;
                Creature.Abilities.Strength.BaseValue -= _sourceRange.StrengthUp;
                Creature.Abilities.Constitution.BaseValue -= _sourceRange.ConstitutionUp;
                Creature.Body.NaturalArmor.BaseValue -= _sourceRange.NaturalArmorUp(Creature.Body.NaturalArmor.BaseValue);
                Creature.Body.ReachSquares.BaseValue = _targetRange.NaturalReach(_IsLong);
            }
        }
        #endregion

        /// <summary>creature level for sizing (returns 0 for partial power die, -1 for smallest power die)</summary>
        protected int SizingLevel
            => (IsFractional)
            ? (IsSmallestFractional ? -1 : 0)
            : CurrentLevel;

        #region private void StabilizeSize()
        private void StabilizeSize()
        {
            // find expected size
            var _sizeLevel = SizingLevel;
            var _expectedRange = _SizeRanges
                .FirstOrDefault(_sr => (_sr.LowPowerDie <= _sizeLevel) && (_sizeLevel <= _sr.HighPowerDie));
            if (_expectedRange != null)
            {
                // see if size should change
                if (_expectedRange.CreatureSize.Order != Creature.Body.Sizer.Size.Order)
                {
                    // current size
                    var _currSize = Creature.Body.Sizer.Size;
                    if (_expectedRange.CreatureSize.Order > Creature.Body.Sizer.Size.Order)
                    {
                        // need to increase everything
                        for (var _sx = _currSize.Order; _sx < _expectedRange.CreatureSize.Order; _sx++)
                        {
                            // get next size from current
                            _currSize = Size.LargerSize(_currSize);
                            StepUpSize(_currSize);
                        }
                    }
                    else
                    {
                        // need to decrease everything
                        for (var _sx = _currSize.Order; _sx > _expectedRange.CreatureSize.Order; _sx--)
                        {
                            _currSize = Size.SmallerSize(_currSize);
                            StepDownSize(_currSize);
                        }
                    }

                    // set creature size
                    Creature.Body.Sizer.NaturalSize = _expectedRange.CreatureSize;
                }
                else
                {
                    // see if reach profile changed
                    var _reach = _expectedRange.NaturalReach(IsLong);
                    if (_reach != Creature.Body.ReachSquares.EffectiveValue)
                    {
                        Creature.Body.ReachSquares.BaseValue = _reach;
                    }
                }
            }
        }
        #endregion

        #region protected override void OnIncreaseLevel()
        protected override void OnIncreaseLevel()
        {
            // shut down optional fraction on first power die
            if (IsFractional)
            {
                Creature.AdvancementLog[this, CurrentLevel].PowerDie.IsFractional = false;
            }

            _ValueCtrl.DoPreValueChanged(CurrentLevel + 1);
            base.OnIncreaseLevel();
            StabilizeSize();
            _ValueCtrl.DoValueChanged(CurrentLevel);
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(@"CurrentLevel"));
        }
        #endregion

        #region protected override void OnDecreaseLevel()
        protected override void OnDecreaseLevel()
        {
            _ValueCtrl.DoPreValueChanged(CurrentLevel - 1);
            base.OnDecreaseLevel();
            StabilizeSize();
            _ValueCtrl.DoValueChanged(CurrentLevel);
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(@"CurrentLevel"));
        }
        #endregion

        protected override void OnAdd()
        {
            base.OnAdd();
            _PowerLevel.Deltas.Add((IQualifyDelta)Creature.ExtraClassPowerLevel);
        }

        protected override void OnRemove()
        {
            base.OnRemove();
            _PowerLevel.Deltas.Remove((IQualifyDelta)Creature.ExtraClassPowerLevel);
        }

        #region IControlChange<int> Members
        private ChangeController<int> _ValueCtrl;
        public void AddChangeMonitor(IMonitorChange<int> subscriber)
        {
            _ValueCtrl.AddChangeMonitor(subscriber);
        }

        public void RemoveChangeMonitor(IMonitorChange<int> subscriber)
        {
            _ValueCtrl.RemoveChangeMonitor(subscriber);
        }
        #endregion

        [field: NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #region IPowerClass Members

        public IVolatileValue ClassPowerLevel => _PowerLevel;

        public Guid OwnerID
            => (Creature?.ID ?? Guid.Empty);

        public bool IsPowerClassActive { get => true; set { } }

        public PowerClassInfo ToPowerClassInfo()
            => new PowerClassInfo
            {
                OwnerID = OwnerID.ToString(),
                Key = Key,
                ID = ID,
                Message = ClassName,
                IsPowerClassActive = IsPowerClassActive,
                ClassPowerLevel = _PowerLevel.ToDeltableInfo(),
                Icon = new ImageryInfo { Keys = ClassIconKey.ToEnumerable().ToArray() }
            };

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => _PowerLevel.QualifiedDeltas(qualify);

        private TerminateController _TCtrl;
        private TerminateController _Term
            => _TCtrl ??= new TerminateController(this);

        public void DoTerminate()
            => _Term.DoTerminate();

        public void AddTerminateDependent(IDependOnTerminate subscriber)
            => _Term.AddTerminateDependent(subscriber);

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            => _Term.RemoveTerminateDependent(subscriber);

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;

        #endregion

        #region IControlChange<Activation> Members

        public void AddChangeMonitor(IMonitorChange<Activation> monitor)
        {
            // NOTHING
        }

        public void RemoveChangeMonitor(IMonitorChange<Activation> monitor)
        {
            // NOTHING
        }

        #endregion
    }
}
