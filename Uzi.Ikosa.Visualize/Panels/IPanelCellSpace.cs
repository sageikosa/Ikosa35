using System.Collections.Generic;

namespace Uzi.Visualize
{
    public interface IPanelCellSpace : ICellSpace
    {
        IEnumerable<IBasePanel> Panel1Map { get; }
        IEnumerable<IBasePanel> Panel2Map { get; }
        IEnumerable<IBasePanel> Panel3Map { get; }
        IEnumerable<ICornerPanel> CornerMap { get; }
        IEnumerable<ILFramePanel> LFrameMap { get; }
        IEnumerable<ISlopeComposite> SlopeCompositeMap { get; }
        IBasePanel DiagonalPanel { get; }
        IBasePanel Fill0Panel { get; }
        IBasePanel Fill1Panel { get; }
        IBasePanel Fill2Panel { get; }
        IBasePanel Fill3Panel { get; }
    }
}
