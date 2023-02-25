using System;
using System.Collections.Generic;

namespace Uzi.Ikosa
{
    [AttributeUsage(AttributeTargets.Class), Serializable]
    public class ParameterizedFeatInfoAttribute : FeatInfoAttribute
    {
        public ParameterizedFeatInfoAttribute(string name, string benefit, Type provider)
            : base(name)
        {
            Benefit = benefit;
            Provider = provider;
        }

        public string Benefit { get; private set; }
        public Type Provider { get; private set; }
        public IEnumerable<FeatParameter> GetAvailableTypes(ParameterizedFeatListItem target, Creature creature, int powerDie)
        {
            if (Provider != null)
            {
                IFeatParameterProvider _provider = (IFeatParameterProvider)Activator.CreateInstance(Provider);
                foreach (FeatParameter _fParam in _provider.AvailableParameters(target, creature, powerDie))
                    yield return _fParam;
            }
            yield break;
        }
    }

    public interface IFeatParameterProvider
    {
        IEnumerable<FeatParameter> AvailableParameters(ParameterizedFeatListItem target, Creature creature, int powerDie);
    }
}
