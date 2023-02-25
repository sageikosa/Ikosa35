using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// When performing an active search check
    /// </summary>
    public class SearchData : Observe
    {
        // NOTE: search is targetted to a particular set of locators
        public SearchData(Creature viewer, Deltable checkValue, bool isAutoCheck)
            : base(viewer, viewer)
        {
            CheckValue = checkValue;
            IsAutoCheck = isAutoCheck;
        }

        public Deltable CheckValue { get; private set; }
        public bool IsAutoCheck { get; private set; }
    }
}
