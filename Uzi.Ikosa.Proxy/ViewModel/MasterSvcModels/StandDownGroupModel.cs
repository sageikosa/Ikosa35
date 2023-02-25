using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class StandDownGroupModel : ViewModelBase
    {
        public StandDownGroupModel()
        {
            _Creatures = new ObservableCollection<CreatureLoginInfo>();
        }

        #region data
        private readonly ObservableCollection<CreatureLoginInfo> _Creatures;
        #endregion

        public ObservableCollection<CreatureLoginInfo> Creatures => _Creatures;

        public StandDownGroupInfo StandDownGroupInfo { get; set; }
        public bool IsExpanded { get; set; }

        public CreatureLoginInfo FindCreature(Guid id)
            => Creatures?.FirstOrDefault(_cli => _cli.ID == id);

        public void Conformulate(List<CreatureLoginInfo> conformCreatures, ProxyModel proxies)
        {
            // creatures not in service
            foreach (var _critter in (from _c in Creatures
                                      where !conformCreatures.Any(_cc => _cc.ID == _c.ID)
                                      select _c).ToList())
            {
                Creatures.Remove(_critter);
            }

            // add creatures
            var _addOnly = Creatures.Count == 0;
            foreach (var _critter in (from _cc in conformCreatures
                                      where !Creatures.Any(_c => _cc.ID == _c.ID)
                                      select _cc).ToList())
            {
                if (_critter.Portrait == null)
                    proxies.AddPortrait(_critter);

                if (_addOnly)
                {
                    Creatures.Add(_critter);
                }
                else
                {
                    Creatures.Insert(
                        Creatures.Union(_critter.ToEnumerable()).OrderBy(_c => _c.Name).ToList().IndexOf(_critter),
                        _critter);
                }
            }
        }

    }
}
