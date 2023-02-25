using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class WordAim : AimingMode
    {
        public WordAim(string key, string displayName, Range minModes, Range maxModes)
            : base(key, displayName, minModes, maxModes)
        {
        }

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, AimTargetInfo[] infos, IInteractProvider provider)
        {
            // TODO: this should be different!!! min and max should count the words, not the targets
            return infos
                .OfType<CharacterStringTargetInfo>()
                .Where(_i => _i.Key.Equals(this.Key, StringComparison.OrdinalIgnoreCase))
                .Select(_i => new CharacterStringTarget(_i.Key, _i.CharacterString));
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<WordAimInfo>(action, actor);
            return _info;
        }
    }
}
