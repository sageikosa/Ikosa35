using Uzi.Ikosa.Interactions;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Magic
{
    /// <summary>
    /// Provides keyed saving throw information.  Keys are provided by submodes or the default save interface.
    /// </summary>
    public interface ISaveCapable : ICapability
    {
        SaveMode GetSaveMode(CoreActor actor, IPowerActionSource actionSource, Interaction workSet, string saveKey);
    }
}
