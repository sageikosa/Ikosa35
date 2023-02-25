using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class Feature : IFeature
    {
        public Feature(string name, string description)
        {
            Name = name;
            Description = description;
        }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public FeatureInfo ToFeatureInfo()
            => new FeatureInfo
            {
                Name = Name,
                Description = Description
            };
    }
}
