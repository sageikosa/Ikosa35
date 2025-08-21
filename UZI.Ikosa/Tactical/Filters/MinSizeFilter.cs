using System;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class MinSizeFilter: ILocatorFilter
    {
        public MinSizeFilter(Size minSize)
        {
            MinSize = minSize;
        }

        public Size MinSize { get; private set; }

        #region IFilter Members
        public bool IsIncluded(Locator locator)
        {
            // if any sized object in the locator is included, the locator is included
            foreach (ISizable _size in locator.AllConnectedOf<ISizable>())
            {
                if (_size.Sizer.Size.Order >= MinSize.Order)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
