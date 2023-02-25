using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Uzi.Visualize.Contracts.Tactical;
using Uzi.Core.Contracts.Faults;

namespace Uzi.Visualize.Contracts
{
    // NOTE: If you change the interface name "IVisualizationService" here, you must also update the reference to "IVisualizationService" in App.config.
    [ServiceContract(Namespace = Statics.ServiceNamespace, CallbackContract = typeof(IVisualizationCallback))]
    [ServiceKnownType(@"KnownTypes", typeof(VisualizationKnownTypes))]
    public interface IVisualizationService
    {
        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        void RegisterCallback(string[] creatureIDs);

        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        void DeRegisterCallback();

        [OperationContract]
        BrushCollectionInfo GetRootBrushCollection();

        [OperationContract]
        BrushCollectionInfo GetBrushCollection(string name);

        [OperationContract]
        Model3DInfo GetModel3D(string name);

        [OperationContract]
        MetaModelFragmentInfo GetModel3DFragmentForModel(string metaModelName, string fragmentName);

        [OperationContract]
        string[] MetaModelFragmentNames(); // NOTE: for local map

        [OperationContract]
        MetaModelFragmentInfo GetModel3DFragment(string fragmentName); // NOTE: for local map

        // iconography
        [OperationContract]
        string[] IconNames(); // NOTE: for local map

        [OperationContract]
        IconInfo GetIcon(string name); // NOTE: for local map

        // bitmap images
        [OperationContract]
        string[] BitmapImageNames(); // NOTE: for local map

        [OperationContract]
        BitmapImageInfo GetBitmapImage(string name); // NOTE: for local map

        [OperationContract]
        BitmapImageInfo GetBitmapImageForModel(string modelName, string name);

        [OperationContract]
        BitmapImageInfo GetBitmapImageForBrushCollection(string brushCollectionName, string name);

        // NOTE: was GetBitmapImageForMetaModelBrushesCollection
        [OperationContract]
        BitmapImageInfo GetBitmapImageForMetaModelBrushesCollection(string metaModelName, string name);

        [OperationContract]
        TileSetInfo GetTileSet(string cellMaterial, string tiling);

        [OperationContract]
        IEnumerable<FreshnessTag> GetRooms(string creatureID, string sensorHostID);

        [OperationContract]
        RoomInfo GetRoom(string creatureID, string sensorHostID, string roomID);

        [OperationContract]
        IEnumerable<BackgroundCellGroupInfo> GetBackgrounds();

        [OperationContract]
        IEnumerable<CellSpaceInfo> GetCellSpaces();

        // TODO: other map freshness...models, tilesets, images

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        CubicInfo GetMapViewport(string creatureID, string sensorHostID);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        IEnumerable<SensorHostInfo> GetSensorHosts(string creatureID);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        SensorHostInfo SetSensorHostHeading(string creatureID, string sensorHostID, int heading, int incline);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        SensorHostInfo AdjustSensorHostAiming(string creatureID, string sensorHostID, short zOff, short yOff, short xOff);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        SensorHostInfo SetSensorHostAimExtent(string creatureID, string sensorHostID, double relativeLongitude, double latitude);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        SensorHostInfo SetSensorHostThirdCamera(string creatureID, string sensorHostID, int relativeHeading, int incline);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        IEnumerable<ShadingInfo> GetShadingInfos(string creatureID, string sensorHostID);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        IEnumerable<PresentationInfo> GetObjectPresentations(string creatureID, string sensorHostID);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        IEnumerable<SoundAwarenessInfo> GetSoundAwarenesses(string creatureID, string sensorHostID);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        IEnumerable<TransientVisualizerInfo> GetTransientVisualizers(string creatureID, string sensorHostID);
    }
}