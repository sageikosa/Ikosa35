using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Interactions
{
    public class GraspData : InteractData
    {
        #region construction
        public GraspData(CoreActor actor, ISensorHost viewer, Locator observerLoc, IGeometricRegion targetGeometry)
            : base(actor)
        {
            _Viewer = viewer;
            _ObserverLocator = observerLoc;
            _TargetGeom = targetGeometry;
        }
        #endregion

        #region private data
        private ISensorHost _Viewer;
        private Locator _ObserverLocator;
        private IGeometricRegion _TargetGeom;
        #endregion

        public ISensorHost Viewer => _Viewer;
        public IGeometricRegion TargetGeometry => _TargetGeom;

        public Locator ObserverLocator
            => _ObserverLocator ??= Locator.FindFirstLocator(_Viewer);

        private static GraspHandler _Static = new GraspHandler();

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Static;
            yield break;
        }
    }

    public class GraspFeedback : InteractionFeedback
    {
        public GraspFeedback(object source)
            : base(source)
        {
        }

        public IEnumerable<Info> Information { get; set; }

        public IEnumerable<Guid> ActionAwarnesses { get; set; }
    }
}
