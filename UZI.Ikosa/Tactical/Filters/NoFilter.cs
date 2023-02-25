using System;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class NoFilter : ILocatorFilter
    {
        #region IFilter Members
        public bool IsIncluded(Uzi.Ikosa.Tactical.Locator locator)
        {
            return true;
        }
        #endregion
    }
}
