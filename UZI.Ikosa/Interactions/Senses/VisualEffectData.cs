using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Requests visual effects to apply to a model</summary>
    public class VisualEffectData : InteractData
    {
        #region construction
        public VisualEffectData(CoreActor actor, IGeometricRegion source, Locator target,
            IList<SensoryBase> filteredSenses, Presentation presentation, PlanarPresence planar)
            : base(actor)
        {
            // if target or actor is ethereal, then there is some kind of ability to perceive
            // therefore, consider both???
            _Planar = planar;
            _Source = source;
            _Target = target;
            _Senses = filteredSenses;
            _Present = presentation;
        }
        #endregion

        #region state
        private PlanarPresence _Planar;
        private IGeometricRegion _Source;
        private Locator _Target;
        private IList<SensoryBase> _Senses;
        private Presentation _Present;
        #endregion

        public PlanarPresence PlanarPresence => _Planar;
        public IGeometricRegion SourceRegion => _Source;
        public Locator TargetLocator => _Target;
        public IList<SensoryBase> Senses => _Senses;
        public Presentation Presentation => _Present;

        private static VisualEffectHandler _Static = new VisualEffectHandler();

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
            => _Static.ToEnumerable();
    }

    /// <summary>Provides visual effects to apply to a model</summary>
    public class VisualEffectFeedback : InteractionFeedback
    {
        #region construction
        public VisualEffectFeedback(object source)
            : base(source)
        {
            _VisualEffects = [];
        }
        #endregion

        #region private data
        private Dictionary<Type, VisualEffect> _VisualEffects;
        #endregion

        public Dictionary<Type, VisualEffect> VisualEffects { get { return _VisualEffects; } }
    }
}
