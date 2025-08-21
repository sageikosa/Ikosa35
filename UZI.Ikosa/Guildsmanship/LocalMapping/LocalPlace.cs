using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class LocalPlace : ModuleElement
    {
        private readonly StoryInformation _StoryInfo;
        private LocalMapSite _LocalMap;
        private readonly HashSet<Guid> _RoomIDs;

        public LocalPlace(Description description, LocalMapSite localMapSite) 
            : base(description)
        {
            _StoryInfo = new StoryInformation();
            _LocalMap = localMapSite;
            _RoomIDs = [];
        }

        public StoryInformation StoryInformation => _StoryInfo;
        public LocalMapSite LocalMapSite => _LocalMap;
        public HashSet<Guid> RoomIDs => _RoomIDs;
    }
}
