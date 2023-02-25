using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class WidenSpellMode : MetaMagicSpellMode, IRegionCapable
    {
        public WidenSpellMode(ISpellMode wrapped)
            : base(wrapped)
        {
        }

        public override IMode GetCapability<IMode>()
        {
            var _baseMode = base.GetCapability<IMode>();
            if (_baseMode != null)
            {
                // if so, replace if it matches one of our supported modes
                switch (_baseMode)
                {
                    case IRegionCapable _:
                        return this as IMode;
                }
            }
            return _baseMode;
        }

        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
            => Wrapped.GetCapability<IRegionCapable>()?
            .Dimensions(actor, casterLevel)
            .Select(_d => _d * 2d) 
            ?? 0d.ToEnumerable();
    }
}
