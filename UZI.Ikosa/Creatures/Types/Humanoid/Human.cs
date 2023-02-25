using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.TypeListers;
using System.Linq;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Human : BaseHumanoidSpecies
    {
        public Human()
            : base()
        {
        }

        public override Species TemplateClone(Creature creature)
            => new Human();

        #region private data
        private FeatBase _Feat;
        #endregion

        #region public override IEnumerable<AdvancementRequirement> Requirements()
        public override IEnumerable<AdvancementRequirement> Requirements(int level)
        {
            if (level == 1)
            {
                yield return new AdvancementRequirement(new RequirementKey(@"Human.Bonus"), @"Extra Feat",
                    @"Human receives an extra feat", HumanBonusFeats, SetBonusFeat, CheckBonusFeat)
                { CurrentValue = BonusFeature() };
            }
            yield break;
        }
        #endregion

        public override IEnumerable<Feature> Features(int level)
        {
            if ((_Feat != null) && (level == 1))
            {
                yield return new Feature(_Feat.Name, _Feat.Benefit);
            }
            yield break;
        }

        #region private IFeature BonusFeature()
        private IFeature BonusFeature()
        {
            if (_Feat != null)
            {
                return new Feature(_Feat.Name, _Feat.Benefit);
            }
            return null;
        }
        #endregion

        #region private IEnumerable<IAdvancementOption> HumanBonusFeats(IResolveRequirement target, RequirementKey key)
        private IEnumerable<IAdvancementOption> HumanBonusFeats(IResolveRequirement target, RequirementKey key)
        {
            foreach (var _item in from _available in FeatLister.AvailableFeats(Creature, this, 1)
                                  orderby _available.Name
                                  select _available)
            {
                if (_item.Feat != null)
                {
                    // GenericAdvancementOption<> carries to the selection UI as an IAdvancementOption, ...
                    // ...but back to SetBonusFeat as a GenericAdvancementOption<>
                    yield return new AdvancementParameter<FeatBase>(target, _item.Name, _item.Benefit, _item.Feat);
                }
                else
                {
                    // ParameterizedAdvancementOption carries to the selection UI as an IParameterizedAdvancementOption 
                    // (with option values of GenericAdvancementOption<Type>)
                    if (_item is ParameterizedFeatListItem _pItem)
                    {
                        // convert feat type list to generic advancement option list
                        yield return new GenericTypeAdvancementOption(target, _pItem.Name, _pItem.Benefit, _pItem.GenericType,
                            from p in _pItem.AvailableParameters
                            select (IAdvancementOption)(new AdvancementParameter<Type>(p.Name, p.Description, p.Type)));
                    }
                }
            }
            yield break;
        }
        #endregion

        #region private bool DoBindBonusFeat(FeatBase bonusFeat)
        private bool DoBindBonusFeat(FeatBase bonusFeat)
        {
            if (_Feat != null)
            {
                // tries to remove an existing bonus feat for this level
                if (_Feat.CanRemove())
                {
                    // even if the new feat cannot be added, the old will be removed
                    _Feat.UnbindFromCreature();
                }
                else
                    return false;
            }

            if (bonusFeat.CanAdd(Creature))
            {
                bonusFeat.BindTo(Creature);
                _Feat = bonusFeat;
                return true;
            }
            else
            {
                // indicate this feat cannot be added
                return false;
            }
        }
        #endregion

        #region private bool SetBonusFeat(RequirementKey key, IAdvancementOption advOption)
        private bool SetBonusFeat(RequirementKey key, IAdvancementOption advOption)
        {
            // must be a simple feat
            var _levelKey = key as RequirementKey;
            if (advOption is AdvancementParameter<FeatBase> _featOption)
            {
                var _newFeat = _featOption.ParameterValue;
                return DoBindBonusFeat(_newFeat);
            }
            else
            {
                // must have been a parameterized feat, get the option, and its target
                if (advOption is AdvancementParameter<Type> _option)
                {
                    var _newFeat = (FeatBase)Activator.CreateInstance(_option.ParameterValue, this, 0);
                    return DoBindBonusFeat(_newFeat);
                }

                // !!! sometimes a GenericTypeAdvancementOption worms itself in here when clicked with no options
                return false;
            }
        }
        #endregion

        #region private bool CheckBonusFeat(RequirementKey key)
        private bool CheckBonusFeat(RequirementKey key)
        {
            if (_Feat != null)
            {
                return _Feat.MeetsRequirementsAtPowerLevel;
            }
            return false;
        }
        #endregion

        public override Type FavoredClass() => null;
        public override string Name => @"Human";
        public override IEnumerable<Language> CommonLanguages() { yield break; }
        public override bool IsCharacterCapable => true;

        #region protected override void OnPreClearDock()
        protected override void OnDisconnectSpecies()
        {
            if (_Feat != null)
                _Feat.UnbindFromCreature();
            base.OnDisconnectSpecies();
        }
        #endregion

        [Serializable]
        public class HumanSkillPointDelta : IQualifyDelta
        {
            public HumanSkillPointDelta()
            {
                _Terminator = new TerminateController(this);
                _Delta = new QualifyingDelta(2, typeof(Racial), @"Human Skill Point Bonus");
            }

            #region data
            private readonly IDelta _Delta;
            private readonly TerminateController _Terminator;
            #endregion

            #region ISupplyQualifyDelta Members
            public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            {
                if ((qualify is Interaction _iAct)
                    && (_iAct.InteractData is Intelligence.SkillPointCalc))
                {
                    yield return _Delta;
                }
                yield break; ;
            }
            #endregion

            #region ITerminating Members
            /// <summary>
            /// Tells all modifiable values using this modifier to release it.  
            /// Note: this does not destroy the modifier and it can be re-used.
            /// </summary>
            public void DoTerminate()
            {
                _Terminator.DoTerminate();
            }

            #region IControlTerminate Members
            public void AddTerminateDependent(IDependOnTerminate subscriber)
            {
                _Terminator.AddTerminateDependent(subscriber);
            }

            public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            {
                _Terminator.RemoveTerminateDependent(subscriber);
            }

            public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
            #endregion
            #endregion
        }

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield return new CreatureSpeciesSubType<Human>(this, @"Human");
            yield break;
        }
        #endregion

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
        {
            var _body = new HumanoidBody(HideMaterial.Static, Size.Medium, 1, false)
            {
                BaseHeight = 5,
                BaseWidth = 2.5,
                BaseLength = 2.5,
                BaseWeight = 120
            };
            return _body;
        }
        #endregion

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            var _land = new LandMovement(30, Creature, this);
            yield return _land;
            yield return new ClimbMovement(0, Creature, this, false, _land);
            yield return new SwimMovement(0, Creature, this, false, _land);
            yield break;
        }
        #endregion

        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            yield break;
        }

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Common(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            yield return new Senses.Vision(false, this);
            yield return new Senses.Hearing(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // skill point boost
            yield return new ExtraordinaryTrait(this, @"Human Versatility", @"+1 skill point per level",
                TraitCategory.Quality, new QualifyDeltaTrait(this, new HumanSkillPointDelta(),
                Creature.Abilities.Intelligence));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);
            yield break;
        }
        #endregion
    }
}