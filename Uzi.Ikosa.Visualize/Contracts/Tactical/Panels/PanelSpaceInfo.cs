using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class PanelSpaceInfo : CellSpaceInfo
    {
        #region Construction
        public PanelSpaceInfo()
            : base()
        {
        }

        public PanelSpaceInfo(IPanelCellSpace panelSpace)
            : base(panelSpace)
        {
            Panel1s = panelSpace.Panel1Map.Select(_p => _p != null ? new NormalPanelInfo(_p) : null).ToArray();
            Panel2s = panelSpace.Panel2Map.Select(_p => _p != null ? new NormalPanelInfo(_p) : null).ToArray();
            Panel3s = panelSpace.Panel3Map.Select(_p => _p != null ? new NormalPanelInfo(_p) : null).ToArray();
            Corners = panelSpace.CornerMap.Select(_cp => _cp != null ? new CornerPanelInfo(_cp) : null).ToArray();
            LFrames = panelSpace.LFrameMap.Select(_lf => _lf != null ? new LFramePanelInfo(_lf) : null).ToArray();
            Slopes = panelSpace.SlopeCompositeMap.Select(_sc => _sc != null ? new SlopeCompositeInfo(_sc) : null).ToArray();
            if (panelSpace.DiagonalPanel != null) Diagonal = new DiagonalCompositeInfo(panelSpace.DiagonalPanel);
            if (panelSpace.Fill0Panel != null) Fill0 = new MaterialFillInfo(panelSpace.Fill0Panel);
            if (panelSpace.Fill1Panel != null) Fill1 = new MaterialFillInfo(panelSpace.Fill1Panel);
            if (panelSpace.Fill2Panel != null) Fill2 = new MaterialFillInfo(panelSpace.Fill2Panel);
            if (panelSpace.Fill3Panel != null) Fill3 = new MaterialFillInfo(panelSpace.Fill3Panel);
        }
        #endregion

        [DataMember]
        public NormalPanelInfo[] Panel1s { get; set; }
        [DataMember]
        public NormalPanelInfo[] Panel2s { get; set; }
        [DataMember]
        public NormalPanelInfo[] Panel3s { get; set; }
        [DataMember]
        public CornerPanelInfo[] Corners { get; set; }
        [DataMember]
        public LFramePanelInfo[] LFrames { get; set; }
        [DataMember]
        public SlopeCompositeInfo[] Slopes { get; set; }
        [DataMember]
        public DiagonalCompositeInfo Diagonal { get; set; }
        [DataMember]
        public MaterialFillInfo Fill0 { get; set; }
        [DataMember]
        public MaterialFillInfo Fill1 { get; set; }
        [DataMember]
        public MaterialFillInfo Fill2 { get; set; }
        [DataMember]
        public MaterialFillInfo Fill3 { get; set; }
    }
}
