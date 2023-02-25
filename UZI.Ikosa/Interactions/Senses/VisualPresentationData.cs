using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Requests Presentation, Dependency Properties and Transforms</summary>
    public class VisualPresentationData : InteractData
    {
        public VisualPresentationData(CoreActor actor, IGeometricRegion observer, ISensorHost sensorHost, 
            IList<SensoryBase> filteredSenses, Locator target)
            : base(actor)
        {
            _Observer = observer;
            _Sensors = sensorHost;
            _Target = target;
            _Senses = filteredSenses;
        }

        #region state
        private readonly IGeometricRegion _Observer;
        private readonly ISensorHost _Sensors;
        private readonly Locator _Target;
        private readonly IList<SensoryBase> _Senses;
        #endregion

        public IGeometricRegion ObserverRegion => _Observer;
        public ISensorHost Sensors => _Sensors;
        public Locator TargetLocator => _Target;
        public IList<SensoryBase> Senses => _Senses;

        private static VisualModelHandler _Static = new VisualModelHandler();

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
            => _Static.ToEnumerable();
    }

    public abstract class VisualPresentationFeedback : InteractionFeedback
    {
        protected VisualPresentationFeedback(object source)
            : base(source)
        {
        }

        public abstract bool IsPresentable { get; }
        public abstract Presentation Presentation { get; }
    }

    /// <summary>Provides Model Key, Dependency Properties and Transforms</summary>
    public class VisualModelFeedback : VisualPresentationFeedback
    {
        public VisualModelFeedback(object source)
            : base(source)
        {
            _Presentation = new ModelPresentation();
        }

        #region private data
        private ModelPresentation _Presentation;
        #endregion

        public override bool IsPresentable
            => (Presentation != null) && !string.IsNullOrWhiteSpace(ModelPresentation.ModelKey);

        public override Presentation Presentation => _Presentation;
        /// <summary>
        /// 1: size; 2: twist; 3: tilt; 4: heading; 5: custom; 6: global-position; 7: base-face bind; 8: model-offset
        /// </summary>
        public ModelPresentation ModelPresentation => _Presentation;
    }

    /// <summary>Provides Icon Key, Dependency Properties and Transforms</summary>
    public class VisualIconFeedback : VisualPresentationFeedback
    {
        public VisualIconFeedback(object source)
            : base(source)
        {
            _Presentation = new IconPresentation();
        }

        #region private data
        private IconPresentation _Presentation;
        #endregion

        public override bool IsPresentable
            => IconPresentation?.IconRefs.Any() ?? false;

        public override Presentation Presentation => _Presentation;
        public IconPresentation IconPresentation => _Presentation;
    }
}
