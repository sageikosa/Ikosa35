using System.Collections.Generic;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using System.Linq;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// :SpellTransit :InteractData with a SpellEffect
    /// </summary>
    public class MagicPowerEffectTransit<MagicPowerSrc> : PowerActionTransit<MagicPowerSrc>
        where MagicPowerSrc : MagicPowerActionSource
    {
        public MagicPowerEffectTransit(MagicPowerSrc magicSource, ICapabilityRoot root, PowerAffectTracker tracker,
            IEnumerable<MagicPowerEffect> spellEffects, CoreActor actor, IGeometricContext anchor, 
            PlanarPresence anchorPresence, IEnumerable<AimTarget> allTargets)
            : base(magicSource, root, tracker, actor, anchor, anchorPresence, allTargets)
        {
            _Effects = spellEffects.ToList();
        }

        private List<MagicPowerEffect> _Effects;

        public List<MagicPowerEffect> MagicPowerEffects => _Effects;
    }
}
