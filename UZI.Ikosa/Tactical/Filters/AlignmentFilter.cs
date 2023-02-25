using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Tactical
{
    public class AlignmentFilter : ILocatorFilter
    {
        public AlignmentFilter(Alignment axialAlignment)
        {
            Alignment = axialAlignment;
        }

        public Alignment Alignment { get; private set; }

        #region IFilter Members
        public bool IsIncluded(Locator locator)
        {
            foreach (CoreObject _base in locator.AllConnectedOf<CoreObject>())
            {
                if (_base.GetAlignment().DetectsAs(Alignment))
                    return true;
            }
            return false;
        }
        #endregion
    }
}
