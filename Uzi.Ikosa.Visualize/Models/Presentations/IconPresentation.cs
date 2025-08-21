using System.Collections.Generic;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Visualize
{
    public class IconPresentation : Presentation
    {
        public IconPresentation()
        {
            _refs = [];
        }

        #region state
        private List<IconReferenceInfo> _refs;
        #endregion

        public List<IconReferenceInfo> IconRefs => _refs;
    }
}
