using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.TypeListers;

namespace Uzi.Ikosa.Skills
{
    [Serializable]
    public class SkillSet : ICreatureBound, IEnumerable<SkillBase>
    {
        protected Dictionary<Type, SkillBase> _Skillset = [];
        protected Creature _Creature;

        #region public SkillSet(Creature skillUser)
        public SkillSet(Creature skillUser)
        {
            // hold ref to creature
            _Creature = skillUser;

            // Create Skills
            Add(new AppraiseSkill(skillUser));
            Add(new BalanceSkill(skillUser));
            var _bluff = new BluffSkill(skillUser);
            Add(_bluff);
            Add(new ClimbSkill(skillUser));
            Add(new ConcentrationSkill(skillUser));
            Add(new DecipherScriptSkill(skillUser));
            var _diplomacy = new DiplomacySkill(skillUser);
            Add(_diplomacy);
            Add(new DisableMechanismSkill(skillUser));
            Add(new DisguiseSkill(skillUser));
            Add(new EscapeArtistSkill(skillUser));
            Add(new ForgerySkill(skillUser));
            Add(new GatherInformationSkill(skillUser));
            Add(new HandleAnimalSkill(skillUser));
            Add(new HealSkill(skillUser));
            Add(new IntimidateSkill(skillUser));
            var _jump = new JumpSkill(skillUser);
            Add(_jump);
            Add(new LanguageSkill(skillUser));
            Add(new ListenSkill(skillUser));
            Add(new PickLockSkill(skillUser));
            Add(new QuickFingersSkill(skillUser));
            Add(new RideSkill(skillUser));
            Add(new SearchSkill(skillUser));
            Add(new SenseMotiveSkill(skillUser));
            Add(new SilentStealthSkill(skillUser));
            Add(new StealthSkill(skillUser));
            Add(new SpellcraftSkill(skillUser));
            Add(new SpotSkill(skillUser));
            Add(new SurvivalSkill(skillUser));
            Add(new SwimSkill(skillUser));
            var _tumble = new Skills.TumbleSkill(skillUser);
            Add(_tumble);
            Add(new UseMagicItemSkill(skillUser));
            Add(new UseRopeSkill(skillUser));

            // bind fixed synergies
            _diplomacy.Deltas.Add(_bluff);
            Skill<IntimidateSkill>().Deltas.Add(_bluff);
            Skill<QuickFingersSkill>().Deltas.Add(_bluff);
            Skill<RideSkill>().Deltas.Add(Skill<HandleAnimalSkill>());
            _tumble.Deltas.Add(_jump);
            _diplomacy.Deltas.Add(Skill<SenseMotiveSkill>());
            Skill<BalanceSkill>().Deltas.Add(_tumble);
            _jump.Deltas.Add(_tumble);

            // knowledge, craft, profession and perform
            foreach (var _knowSkill in SubSkillLister.SubSkills<KnowledgeFocus>(typeof(KnowledgeSkill<>), Creature))
            {
                /*
                If you have 5 or more ranks in Knowledge (arcana), you get a +2 bonus on Spellcraft checks. 
                If you have 5 or more ranks in Knowledge (architecture and engineering), you get a +2 bonus on Search checks made to find secret doors or hidden compartments. 
                If you have 5 or more ranks in Knowledge (geography), you get a +2 bonus on Survival checks made to keep from getting lost or to avoid natural hazards. 
                If you have 5 or more ranks in Knowledge (history), you get a +2 bonus on bardic knowledge checks. 
                If you have 5 or more ranks in Knowledge (local), you get a +2 bonus on Gather Information checks. 
                If you have 5 or more ranks in Knowledge (nature), you get a +2 bonus on Survival checks made in aboveground natural environments (aquatic, desert, forest, hill, marsh, mountains, or plains). 
                If you have 5 or more ranks in Knowledge (nobility and royalty), you get a +2 bonus on Diplomacy checks. 
                If you have 5 or more ranks in Knowledge (religion), you get a +2 bonus on turning checks against undead. 
                If you have 5 or more ranks in Knowledge (the planes), you get a +2 bonus on Survival checks made while on other planes. 
                If you have 5 or more ranks in Knowledge (dungeoneering), you get a +2 bonus on Survival checks made while underground. 
                If you have 5 or more ranks in Survival, you get a +2 bonus on Knowledge (nature) checks. 
                 */
                Add(_knowSkill);
            }
            foreach (var _craftSkill in SubSkillLister.SubSkills<CraftFocus>(typeof(CraftSkill<>), Creature))
            {
                // TODO: If you have 5 ranks in a Craft skill, you get a +2 bonus on Appraise checks related to items made with that Craft skill. 
                Add(_craftSkill);
            }
            foreach (var _perfSkill in SubSkillLister.SubSkills<PerformFocus>(typeof(PerformSkill<>), Creature))
            {
                Add(_perfSkill);
            }
        }
        #endregion

        protected void Add(SkillBase skill)
        {
            _Skillset.Add(skill.GetType(), skill);
            if ((skill is IActionProvider _provider) && !Creature.Actions.Providers.ContainsKey(skill))
            {
                Creature.Actions.Providers.Add(skill, _provider);
            }
        }

        #region public S Skill<S>() where S : SkillBase
        public S Skill<S>() where S : SkillBase
        {
            if (_Skillset.ContainsKey(typeof(S)))
            {
                return (S)(_Skillset[typeof(S)]);
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region public SkillBase this[Type skillType] { get; }
        public SkillBase this[Type skillType]
        {
            get
            {
                if (_Skillset.ContainsKey(skillType))
                {
                    return _Skillset[skillType];
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        #region public IEnumerable<SkillBase> BasicSkills { get; }
        /// <summary>Returns non-parameterized skills</summary>
        public IEnumerable<SkillBase> BasicSkills
        {
            get
            {
                foreach (SkillBase _skill in this)
                {
                    if (!_skill.GetType().IsGenericType)
                    {
                        yield return _skill;
                    }
                }
                yield break;
            }
        }
        #endregion

        #region public IEnumerable<SkillBase> ParameterizedSkills { get; }
        /// <summary>Returns parameterized skills</summary>
        public IEnumerable<SkillBase> ParameterizedSkills
        {
            get
            {
                foreach (SkillBase _skill in this)
                {
                    if (_skill.GetType().IsGenericType)
                    {
                        yield return _skill;
                    }
                }
                yield break;
            }
        }
        #endregion

        #region public int MaxClassSkillRanks { get; }
        public int MaxClassSkillRanks
        {
            get
            {
                return SkillBase.MaxClassSkillRanksAtPowerLevel(Creature.AdvancementLog.NumberPowerDice);
            }
        }
        #endregion

        #region public double MaxCrossClassSkillRanks { get; }
        public double MaxCrossClassSkillRanks
        {
            get
            {
                return SkillBase.MaxCrossClassSkillRanksAtPowerLevel(Creature.AdvancementLog.NumberPowerDice);
            }
        }
        #endregion

        #region ICreatureBound Members
        public Creature Creature
        {
            get { return _Creature; }
        }

        public Guid ID => throw new NotImplementedException();
        #endregion

        #region IEnumerable<SkillBase> Members
        public IEnumerator<SkillBase> GetEnumerator()
        {
            foreach (var _kvp in _Skillset)
            {
                yield return _kvp.Value;
            }
            yield break;
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
