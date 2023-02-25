using System;
using System.Collections.Generic;
using Uzi.Ikosa.Magic;
using System.Reflection;

namespace Uzi.Ikosa
{
    public class MagicStyleLister : IFeatParameterProvider
    {
        public IEnumerable<FeatParameter> AvailableParameters(ParameterizedFeatListItem target, Creature creature, int powerDie)
        {
            foreach (Type _type in Assembly.GetAssembly(typeof(MagicStyle)).GetTypes())
            {
                if (_type.IsSubclassOf(typeof(MagicStyle)) && (!_type.IsAbstract) && (_type.IsPublic))
                {
                    yield return new FeatParameter(target, _type, _type.Name, @"Magic Style", powerDie);
                }
            }
            yield break;
        }
    }
}
