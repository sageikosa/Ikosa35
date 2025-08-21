using System.ServiceModel;

namespace Uzi.Visualize.Contracts
{
    [ServiceContract(Namespace = Statics.ServiceNamespace)]
    public interface IVisualizationCallback
    {
        // these first four can probably be signalled by an editor attached to a running host
        // the Visualization service should have functions to broadcast these changes to callback clients
        [OperationContract(IsOneWay = true)]
        void BrushCollectionChanged(string name);

        [OperationContract(IsOneWay = true)]
        void Model3DChanged(string name);

        [OperationContract(IsOneWay = true)]
        void ImageChanged(string name, string modelName, string brushCollectionName);

        [OperationContract(IsOneWay = true)]
        void TileSetChanged(string cellMaterial, string tiling);

        /// <summary>When entire map changes</summary>
        [OperationContract(IsOneWay = true)]
        void MapChanged();
    }
}
