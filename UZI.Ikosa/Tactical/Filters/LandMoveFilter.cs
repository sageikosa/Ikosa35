using System;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LandMoveFilter : ILocatorFilter
    {
        #region IFilter Members
        public bool IsIncluded(Locator locator)
        {
            return locator.ActiveMovement is LandMovement;
        }
        #endregion
    }
}
