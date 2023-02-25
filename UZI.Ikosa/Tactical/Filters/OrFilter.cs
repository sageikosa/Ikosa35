using System;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class OrFilter : ILocatorFilter
    {
        public OrFilter(params ILocatorFilter[] filters)
        {
            Filters = filters;
        }

        public ILocatorFilter[] Filters { get; private set; }

        #region IFilter Members
        public bool IsIncluded(Locator locator)
        {
            foreach (ILocatorFilter _filter in Filters)
            {
                if (_filter.IsIncluded(locator))
                    return true;
            }
            return false;
        }
        #endregion
    }
}
