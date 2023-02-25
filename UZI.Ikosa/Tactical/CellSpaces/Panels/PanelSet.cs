using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class PanelSet<PType> where PType : BasePanel
    {
        public PanelSet()
        {
            _Panels = new Dictionary<AnchorFace, PType>();
        }

        private Dictionary<AnchorFace, PType> _Panels;

        public bool HasPanel(AnchorFace face)
        {
            return _Panels.ContainsKey(face);
        }

        public PType this[int face]
        {
            get { return this[(AnchorFace)face]; }
            set { this[(AnchorFace)face] = value; }
        }

        public PType this[AnchorFace face]
        {
            get
            {
                if (_Panels.ContainsKey(face))
                    return _Panels[face];
                return null;
            }
            set
            {
                _Panels[face] = value;
            }
        }

        public IEnumerable<KeyValuePair<AnchorFace, PType>> AllPanels
        {
            get { return _Panels.Select(_kvp => _kvp); }
        }

        /// <summary>Yields the reference (or null) for each indexed panel in AnchorFace enum order</summary>
        public IEnumerable<PType> ToAnchorFaceMap()
        {
            yield return this[AnchorFace.XLow];
            yield return this[AnchorFace.XHigh];
            yield return this[AnchorFace.YLow];
            yield return this[AnchorFace.YHigh];
            yield return this[AnchorFace.ZLow];
            yield return this[AnchorFace.ZHigh];
            yield break;
        }
    }
}
