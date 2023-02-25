using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>Provides geometry builder</summary>
    public interface IGeometryCapable<PowerSrc> : ICapability
        where PowerSrc : IPowerActionSource
    {
        IGeometryBuilder GetBuilder(IPowerUse<PowerSrc> powerUse, CoreActor actor);
    }
}
