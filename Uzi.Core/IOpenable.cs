using System.Linq;

namespace Uzi.Core
{
    public interface IOpenable : ICoreObject, IAdjunctable, IControlChange<OpenStatus>
    {
        bool CanChangeOpenState(OpenStatus testState);

        /// <summary>
        /// generally use extension method GetOpenStatus to provide an OpenStatus to set here
        /// </summary>
        OpenStatus OpenState { get; set; }
        double OpenWeight { get; set; }
    }

    public static class IOpenableStatic
    {
        /// <summary>Perform an interaction to get an open status based on a proposed value</summary>
        public static OpenStatus GetOpenStatus(this IOpenable openable, CoreActor actor, object source, double value)
        {
            var _interact = new Interaction(actor, source, openable, new OpenCloseInteractData(actor, value));
            openable?.HandleInteraction(_interact);
            return new OpenStatus(source, _interact.Feedback.OfType<ValueFeedback<double>>().FirstOrDefault()?.Value ?? openable?.OpenState.Value ?? 0);
        }
    }
}
