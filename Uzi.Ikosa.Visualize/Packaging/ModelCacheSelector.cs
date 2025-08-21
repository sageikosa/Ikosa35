using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Visualize.Packaging
{
    public class ModelCacheSelector : IEquatable<ModelCacheSelector>
    {
        public ModelCacheSelector(List<VisualEffect> effects, Dictionary<string, int> externalValues)
        {
            _Effects = effects ?? [];
            _ExternalValues = externalValues;
        }

        private List<VisualEffect> _Effects;
        private Dictionary<string, int> _ExternalValues;

        public List<VisualEffect> Effects => _Effects;
        public Dictionary<string, int> ExternalValues => _ExternalValues;

        /// <summary>provides a selector with a cloned dictionary</summary>
        public ModelCacheSelector GetCacheKey()
            => new ModelCacheSelector(Effects, ExternalValues.ToDictionary(_kvp => _kvp.Key, _kvp => _kvp.Value));

        public bool Equals(ModelCacheSelector other)
        {
            if (Effects.SequenceEqual(other.Effects))
            {
                if (ExternalValues.Count == other.ExternalValues.Count)
                {
                    foreach (var _kvp in ExternalValues)
                    {
                        if (!other.ExternalValues.ContainsKey(_kvp.Key)
                            || (other.ExternalValues[_kvp.Key] != _kvp.Value))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
