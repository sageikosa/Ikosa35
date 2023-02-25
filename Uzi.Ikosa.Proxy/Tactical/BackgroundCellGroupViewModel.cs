using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public class BackgroundCellGroupViewModel : IGetCellStructure
    {
        public BackgroundCellGroupViewModel(BackgroundCellGroupInfo info, LocalMapInfo map)
        {
            BackgroundCellGroupInfo = info;
            TemplateSpace = CellSpaceViewModel.GetViewModel(info.TemplateSpace, map);
        }

        public LocalCellGroupInfo Info => BackgroundCellGroupInfo;
        public BackgroundCellGroupInfo BackgroundCellGroupInfo { get; private set; }
        public CellSpaceViewModel TemplateSpace { get; private set; }

        public CellStructureInfo? GetContainedCellSpace(int z, int y, int x)
            => BackgroundCellGroupInfo.ContainsCell(z, y, x) ? new CellStructureInfo { CellSpace = TemplateSpace } : (CellStructureInfo?)null;
    }
}
