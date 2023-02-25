using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class PowerActionTransit<PowerSrc> : InteractData
        where PowerSrc : IPowerActionSource
    {
        #region construction
        public PowerActionTransit(PowerSrc powerSrc, ICapabilityRoot root, PowerAffectTracker tracker, CoreActor actor,
            IGeometricContext anchor, PlanarPresence anchorPresence, IEnumerable<AimTarget> allTargets)
            : base(actor)
        {
            _Src = powerSrc;
            _Anchor = anchor;
            _Presence = anchorPresence;
            _Targets = allTargets.ToList();
            _Root = root;
            _Tracker = tracker;
        }
        #endregion

        #region state
        private PowerSrc _Src;
        private IGeometricContext _Anchor;
        private PlanarPresence _Presence;
        private List<AimTarget> _Targets;
        private ICapabilityRoot _Root;
        private PowerAffectTracker _Tracker;
        #endregion

        public PowerSrc PowerSource => _Src;
        public ICapabilityRoot CapabilityRoot => _Root;
        public PowerAffectTracker PowerTracker => _Tracker;

        /// <summary>Location of the anchor</summary>
        public IGeometricContext Anchor => _Anchor;

        public PlanarPresence AnchorPresence => _Presence;

        /// <summary>Sometimes need to know all the target information</summary>
        public List<AimTarget> AllTargets => _Targets;
    }
}
