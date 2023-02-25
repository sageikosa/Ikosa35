using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Interaction to observe a core object of some kind
    /// </summary>
    public class Observe : InteractData
    {
        #region construction
        public Observe(CoreActor actor, ISensorHost viewer)
            : base(actor)
        {
            _Viewer = viewer;
        }

        public Observe(CoreActor actor, ISensorHost viewer, Locator targetLoc, Locator observerLoc, double distance)
            : this(actor, viewer)
        {
            _TargetLocator = targetLoc;
            _ObserverLocator = observerLoc;
            _Distance = distance;
        }
        #endregion

        #region private data
        private ISensorHost _Viewer;
        private Locator _TargetLocator;
        private Locator _ObserverLocator;
        private double? _Distance;
        #endregion

        public ISensorHost Viewer => _Viewer;

        public Locator GetTargetLocator(IInteract target)
            => _TargetLocator ??= Locator.FindFirstLocator(target);

        public Locator ObserverLocator
            => _ObserverLocator ??= Locator.FindFirstLocator(_Viewer);

        #region public double GetDistance(IInteract target)
        public double GetDistance(IInteract target)
        {
            if (!_Distance.HasValue)
            {
                _Distance = GetTargetLocator(target).GeometricRegion.NearDistance(ObserverLocator.GeometricRegion);
            }
            return _Distance.Value;
        }
        #endregion

        private static ObserveHandler _Static = new ObserveHandler();

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
            => _Static.ToEnumerable();
    }
}
