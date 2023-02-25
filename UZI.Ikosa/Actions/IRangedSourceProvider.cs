using Uzi.Core;

namespace Uzi.Ikosa.Actions
{
    public interface IRangedSourceProvider
    {
        IRangedSource GetRangedSource(CoreActor actor, ActionBase action, RangedAim aim, IInteract target);
    }
}
