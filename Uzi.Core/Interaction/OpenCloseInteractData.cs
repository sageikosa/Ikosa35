using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Core
{
    /// <summary>
    /// Use to determine whether any intrinsic blocking will keep the open/close from happening
    /// </summary>
    [Serializable]
    public class OpenCloseInteractData : InteractData
    {
        /// <summary>
        /// Use to determine whether any intrinsic blocking will keep the open/close from happening
        /// </summary>
        public OpenCloseInteractData(CoreActor actor, double newValue)
            : base(actor)
        {
            OpenState = newValue;
        }

        public double OpenState { get; private set; }

        private static readonly OpenCloseInteractHandler _Handler = new OpenCloseInteractHandler();

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Handler;
            yield break;
        }
    }

    [Serializable]
    public class OpenCloseInteractHandler : IInteractHandler
    {
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(OpenCloseInteractData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is OpenCloseInteractData _openClose)
            {
                workSet.Feedback.Add(new ValueFeedback<double>(this, _openClose.OpenState));
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }
    }
}
