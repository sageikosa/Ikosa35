using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public abstract class SitePath : ModuleElement
    {
        private readonly StoryInformation _StoryInfo;
        private readonly Guid _Source;
        private readonly Guid _Target;
        private readonly Dictionary<Guid, List<Vector3D>> _Inflections;
        private decimal _RevFactor;
        private decimal _LosePathChance;

        protected SitePath(Description description, Guid sourceLink, Guid targetLink, decimal reverseFactor)
            : base(description)
        {
            _StoryInfo = new StoryInformation();
            _Source = sourceLink;
            _Target = targetLink;
            _Inflections = [];
            _RevFactor = reverseFactor;
            _LosePathChance = 0m;
        }

        public StoryInformation StoryInformation => _StoryInfo;
        public Guid SourceLinkID => _Source;
        public Guid TargetLinkID => _Target;

        public bool HasSiteLink(Guid linkID)
            => _Source == linkID || _Target == linkID;

        /// <summary>Relative offsets along path between Source and Target, indexed by Region</summary>
        public Dictionary<Guid, List<Vector3D>> Inflections => _Inflections;

        public decimal ReverseFactor => _RevFactor;
        public decimal LosePathChance { get => _LosePathChance; set => _LosePathChance = value; }
    }
}
