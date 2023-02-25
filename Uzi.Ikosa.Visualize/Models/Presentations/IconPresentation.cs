using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Visualize
{
    public class IconPresentation : Presentation
    {
        public IconPresentation()
        {
            _refs = new List<IconReferenceInfo>();
        }

        #region state
        private List<IconReferenceInfo> _refs;
        #endregion

        public List<IconReferenceInfo> IconRefs => _refs;
    }
}
