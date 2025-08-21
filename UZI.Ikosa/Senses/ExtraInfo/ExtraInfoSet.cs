using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Uzi.Core.Contracts;
using System.Security.Policy;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class ExtraInfoSet : ICreatureBound
    {
        #region Construction
        public ExtraInfoSet(Creature critter)
        {
            _Critter = critter;
            _Sources = [];
            _Markers = [];
        }
        #endregion

        #region state
        private Creature _Critter;
        private Collection<ExtraInfo> _Markers;
        private Collection<object> _Sources;
        #endregion

        public Creature Creature => _Critter;
        public int Count => _Markers.Count;
        public bool Contains(ExtraInfo marker) => _Markers.Contains(marker);
        public IEnumerable<ExtraInfoSource> Sources => _Sources.OfType<ExtraInfoSource>();

        #region public void Add(ExtraInfo marker)
        public void Add(ExtraInfo marker)
        {
            // need to add
            if (!_Markers.Contains(marker))
            {
                // track the marker
                _Markers.Add(marker);

                // and need to add a source
                if (!_Sources.OfType<ExtraInfoSource>().Any(_s => _s.ID == marker.InfoSource.ID))
                {
                    _Sources.Add(marker.InfoSource);
                }

                // action provider
                if (marker.ActionProvider != null)
                {
                    Creature.Actions.Providers.Add(marker, marker.ActionProvider);
                }

                // TODO: awaken creature from restful sleep when info becomes available
            }
        }
        #endregion

        #region public void Remove(ExtraInfo marker, bool noAlert)
        public void Remove(ExtraInfo marker, bool alert)
        {
            // currently tracking
            if (_Markers.Contains(marker))
            {
                // remove the marker
                _Markers.Remove(marker);

                // remove unused sources
                if (!_Markers.Any(_m => _m.InfoSource.ID == marker.InfoSource.ID))
                {
                    var _source = Sources.FirstOrDefault(_s => _s.ID == marker.InfoSource.ID);
                    if (_source != null)
                    {
                        _Sources.Remove(_source);
                    }
                }

                // remove action providers
                if (marker.ActionProvider != null)
                {
                    Creature.Actions.Providers.Remove(marker);
                }
            }
            // TODO: handle alert flag == false (indicating to "silently" remove the information from the set)
        }
        #endregion

        #region public void RemoveSource(Guid id)
        public void RemoveSource(Guid id)
        {
            // markers as providers to remove
            var _providers = (from _m in _Markers
                              where (_m.InfoSource.ID == id) && (_m.ActionProvider != null)
                              select _m).ToList();

            // remove markers from tail down
            for (int _mx = _Markers.Count - 1; _mx >= 0; _mx--)
            {
                // if the marker is sourced to the removing source
                if (_Markers[_mx].InfoSource.ID == id)
                {
                    _Markers.RemoveAt(_mx);
                }
            }

            // remove source
            var _source = Sources.FirstOrDefault(_s => _s.ID == id);
            if (_source != null)
            {
                _Sources.Remove(_source);
            }

            // action providers
            foreach (var _m in _providers)
            {
                Creature.Actions.Providers.Remove(_m);
            }
        }
        #endregion

        public IEnumerable<ExtraInfo> All
            => _Markers.Select(_m => _m);
    }
}
