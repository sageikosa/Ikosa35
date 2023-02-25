using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public class VisualizationServiceClient : ServiceClient<IVisualizationService, IVisualizationCallback>
    {
        public VisualizationServiceClient(string address, IVisualizationCallback callback, string userName, string password)
            : base(address, callback, userName, password)
        {
        }

        public IEnumerable<CellSpaceViewModel> GetCellSpaceViewModels(LocalMapInfo map)
        {
            return Service.GetCellSpaces().Select(_s => CellSpaceViewModel.GetViewModel(_s, map));
        }
    }
}
