using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class CreatureModel : ViewModelBase
    {
        public CreatureModel(ActorModel actor, CreatureInfo creature)
        {
            _Actor = actor;
            _Creature = creature;

            _AbilitiesTake10 = new Take10VM(actor, Take10VMType.AbilityBase, creature.Abilities.Take10);
            _SkillsTake10 = new Take10VM(actor, Take10VMType.SkillBase, creature.SkillTake10);

            _BasicSkills = (from _s in creature.BasicSkills
                            select new SkillVM(actor, _s)).ToList();
            _ParameterizedSkills = (from _s in creature.ParameterizedSkills
                                    select new SkillVM(actor, _s)).ToList();
            _Abilities = new AbilitySetVM(actor, creature);
            _Health = new HealthBarVM(creature.HealthPoints);
            _Movements = new List<MovementInfo>();
        }

        #region data
        private ActorModel _Actor;
        private CreatureInfo _Creature;
        private Take10VM _AbilitiesTake10;
        private Take10VM _SkillsTake10;
        private List<SkillVM> _BasicSkills;
        private List<SkillVM> _ParameterizedSkills;
        private AbilitySetVM _Abilities;
        private HealthBarVM _Health;
        private List<MovementInfo> _Movements;
        #endregion

        #region public void Conformulate(CreatureInfo creature)
        /// <summary>
        /// Conformulate creature with updates from host
        /// </summary>
        /// <param name="creature"></param>
        public void Conformulate(CreatureInfo creature)
        {
            // creature is direct...
            _Creature = creature;

            // conformulate AbilityTake10, SkillTake10
            _AbilitiesTake10.Conformulate(creature.Abilities.Take10);
            _SkillsTake10.Conformulate(creature.SkillTake10);

            // conformulate Abilities
            _Abilities.Conformulate(creature);

            // conformulate health
            _Health.Conformulate(creature.HealthPoints);

            // conformulate SkillVM (Basic and Parameterized)
            foreach (var _updt in from _bs in creature.BasicSkills
                                  join _s in _BasicSkills
                                  on _bs.Message equals _s.Skill.Message
                                  select new { Skill = _bs, VM = _s })
            {
                _updt.VM.Conformulate(_updt.Skill);
            }

            foreach (var _updt in from _ps in creature.ParameterizedSkills
                                  join _s in _ParameterizedSkills
                                  on _ps.Message equals _s.Skill.Message
                                  select new { Skill = _ps, VM = _s })
            {
                _updt.VM.Conformulate(_updt.Skill);
            }

            // movements not usable in latest
            var _moveChange = false;
            foreach (var _drop in (from _mv in _Movements
                                   where !creature.Movements.Any(_m => _m.ID == _mv.ID && _m.IsUsable)
                                   select _mv).ToList())
            {
                _Movements.Remove(_drop);
                _moveChange = true;
            }

            // conformulate movements that match
            foreach (var _conform in (from _mv in _Movements
                                      join _cm in creature.Movements.Where(_m => _m.IsUsable)
                                      on _mv.ID equals _cm.ID
                                      select new { Move = _mv, ConformMove = _cm }).ToList())
            {
                _conform.Move.Value = _conform.ConformMove.Value;
            }

            // add movements
            //var _addOnly = Creatures.Count == 0;
            foreach (var _new in (from _cm in creature.Movements.Where(_m => _m.IsUsable)
                                  where !_Movements.Any(_m => _cm.ID == _m.ID)
                                  select _cm).ToList())
            {
                //if (_addOnly)
                //{
                _Movements.Add(_new);
                //}
                //else
                //{
                //    Creatures.Insert(
                //        Creatures.Union(_critter.ToEnumerable()).OrderBy(_c => _c.CreatureTrackerInfo.CreatureLoginInfo.Name).ToList().IndexOf(_critter),
                //        _critter);
                //}
                _moveChange = true;
            }


            // notify
            if (_moveChange)
            {
                Actor?.ObservableActor?.MovementCommands.EnsureMovement();
            }
            DoPropertyChanged(nameof(Movements));
            DoPropertyChanged(nameof(CreatureInfo));
            DoPropertyChanged(nameof(ClassString));
        }
        #endregion

        public ActorModel Actor => _Actor;
        public CreatureInfo CreatureInfo => _Creature;
        public HealthBarVM HealthBar => _Health;

        public Take10VM AbilityTake10 => _AbilitiesTake10;
        public Take10VM SkillTake10 => _SkillsTake10;
        public AbilitySetVM Abilities => _Abilities;
        public List<MovementInfo> Movements => _Movements;

        public string ClassString
            => string.Join(@"; ", CreatureInfo.Classes.Select(_c => $@"{_c.ClassName}:{_c.CurrentLevel}"));

        public IEnumerable<SkillVM> BasicSkills
            => _BasicSkills.Select(_svm => _svm);

        public IEnumerable<SkillVM> ParameterizedSkills
            => _ParameterizedSkills.Select(_svm => _svm);
    }
}
