using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class CreatureGroupModel<TeamInfo, CreatureInfo> : ViewModelBase
        where TeamInfo: TeamGroupInfo
        where CreatureInfo: CreatureTrackerModel
    {
        public CreatureGroupModel()
        {
            _Creatures = new ObservableCollection<CreatureInfo>();
        }

        #region data
        private readonly ObservableCollection<CreatureInfo> _Creatures;
        #endregion

        public ObservableCollection<CreatureInfo> Creatures => _Creatures;
        public TickTrackerModeBase TickTrackerMode { get; set; }

        public TeamInfo TeamGroupInfo { get; set; }
        public bool IsExpanded { get; set; }

        public CreatureInfo FindCreature(Guid id)
            => Creatures?.FirstOrDefault(_ctm => _ctm.ID == id);

        #region public void Conformulate(List<CreatureInfo> conformCreatures)
        public void Conformulate(List<CreatureInfo> conformCreatures)
        {
            // creatures not in service
            foreach (var _critter in (from _c in Creatures
                                      where !conformCreatures.Any(_cc => _cc.ID == _c.ID)
                                      select _c).ToList())
            {
                Creatures.Remove(_critter);
            }

            // conformulate creatures that match
            foreach (var _critter in (from _c in Creatures
                                      join _cc in conformCreatures
                                      on _c.ID equals _cc.ID
                                      select new { Creature = _c, ConformCreature = _cc }).ToList())
            {
                _critter.ConformCreature.Group = this;
                _critter.Creature.Conformulate(_critter.ConformCreature);
            }

            // add creatures
            var _addOnly = Creatures.Count == 0;
            foreach (var _critter in (from _cc in conformCreatures
                                      where !Creatures.Any(_c => _cc.ID == _c.ID)
                                      select _cc).ToList())
            {
                _critter.Group = this;
                _critter.Conformulate(_critter);
                if (_addOnly)
                {
                    Creatures.Add(_critter);
                }
                else
                {
                    Creatures.Insert(
                        Creatures.Union(_critter.ToEnumerable()).OrderBy(_c => _c.CreatureTrackerInfo.CreatureLoginInfo.Name).ToList().IndexOf(_critter),
                        _critter);
                }
            }
        }
        #endregion

        // TODO: all in tracker, some in tracker, none in tracker
        // TODO: distance of closest CreatureTrackerModel
    }
}
