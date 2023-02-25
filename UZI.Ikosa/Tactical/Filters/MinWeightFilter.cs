using System;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class MinWeightFilter : ILocatorFilter
    {
        public MinWeightFilter(double minWeight)
        {
            MinWeight = minWeight;
        }

        public double MinWeight { get; private set; }

        #region IFilter Members
        public bool IsIncluded(Locator locator)
        {
            foreach (CoreObject _base in locator.AllConnectedOf<CoreObject>())
            {
                if (_base.Weight >= MinWeight)
                    return true;
            }
            return false;
        }
        #endregion
    }
}
