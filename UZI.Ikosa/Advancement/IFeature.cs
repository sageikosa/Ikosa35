using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Advancement
{
    public interface IFeature
    {
        string Name { get; }
        string Description { get; }
        FeatureInfo ToFeatureInfo();
    }
}
