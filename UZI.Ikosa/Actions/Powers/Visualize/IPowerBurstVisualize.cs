using Uzi.Core;

namespace Uzi.Ikosa.Actions
{
    public interface IPowerBurstVisualize : ICapability
    {
        string GetBurstMaterialKey();

        VisualizeSplashType GetSplashType();
        string GetSplashMaterialKey();
    }
}
