using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class OverrideModelKey : Adjunct, IProcessFeedback
    {
        public OverrideModelKey(object source, string modelKey)
            : base(source)
        {
            _ModelKey = modelKey;
        }

        #region data
        private string _ModelKey;
        #endregion

        #region OnActivate / OnDeactivate
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as IInteractHandlerExtendable)?.AddIInteractHandler(this);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as IInteractHandlerExtendable)?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }
        #endregion

        public string ModelKey => _ModelKey;

        public override bool IsProtected => true;

        public override object Clone()
            => new OverrideModelKey(Source, ModelKey);

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(VisualPresentationData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {            
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => existingHandler is FurnishingVisualHandler;

        public void ProcessFeedback(Interaction workSet)
        {
            if (workSet?.InteractData is VisualPresentationData)
            {
                var _present = workSet.Feedback.OfType<VisualModelFeedback>().FirstOrDefault()?.ModelPresentation;
                if (_present != null)
                {
                    // TODO: check that model can be resolved before allowing use?
                    _present.ModelKey = ModelKey;
                }
            }
        }
    }
}
