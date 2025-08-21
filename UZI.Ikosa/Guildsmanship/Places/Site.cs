using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Guildsmanship.Overland;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public abstract class Site : ModuleNode
    {
        private readonly OverRegion _OverRegion;
        private readonly List<SitePathGraphLink> _Networks;
        private readonly StoryInformation _StoryInfo;

        protected Site(Description description)
            : base(description)
        {
            _OverRegion = new OverRegion(description.Clone() as Description);
            _Networks = [];
            _StoryInfo = new StoryInformation();
        }

        public OverRegion OverRegion => _OverRegion;
        public List<SitePathGraphLink> Networks => _Networks;

        // --- what might happen here
        public StoryInformation StoryInformation => _StoryInfo;

        public override IEnumerable<ICorePart> Relationships { get { yield break; } }
    }
}
