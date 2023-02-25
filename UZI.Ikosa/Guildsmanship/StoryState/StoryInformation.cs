using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class StoryInformation
    {
        private readonly Mutabilities _Mutabilities;
        private readonly Dictionary<Guid, EncounterTableLink> _Encounters;
        private readonly Dictionary<Guid, StoryInfoElement> _Infos;

        public StoryInformation()
        {
            _Mutabilities = new Mutabilities();
            _Encounters = new Dictionary<Guid, EncounterTableLink>();
            _Infos = new Dictionary<Guid, StoryInfoElement>();
        }

        public Mutabilities Mutabilities => _Mutabilities;
        public Dictionary<Guid, EncounterTableLink> Encounters => _Encounters;
        public Dictionary<Guid, StoryInfoElement> Info => _Infos;
    }
}
