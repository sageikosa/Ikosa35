using System;
using System.Collections.Generic;
using System.ServiceModel;
using Uzi.Core.Contracts.Faults;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [ServiceContract(
        Namespace = Statics.ServiceNamespace,
        CallbackContract = typeof(IIkosaCallback)
        )]
    [ServiceKnownType(@"KnownTypes", typeof(IkosaKnownTypes))]
    public interface IIkosaServices
    {
        // ---------- callback control
        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void RegisterCallback(string[] id);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void DeRegisterCallback();

        // ---------- prerequisites
        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        List<PrerequisiteInfo> GetPreRequisites(string id);

        [OperationContract(IsOneWay = true)]
        void SetPreRequisites(PrerequisiteInfo[] prereqInfos);

        // ---------- actions
        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        List<ActionInfo> GetActions(string id);

        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        bool CanStartAction(ActivityInfo activity);

        [OperationContract(IsOneWay = true)]
        void EndTurn(string id);

        [OperationContract(IsOneWay = true)]
        void DoAction(ActivityInfo activity);

        [OperationContract(IsOneWay = true)]
        void CancelAction(string creatureID);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        LocalActionBudgetInfo GetActionBudget(string id);

        // ---------- initiative control

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        LocalTurnTrackerInfo GetTurnTracker(string id);

        // TODO: tracker mode switching

        // ---------- process
        [OperationContract]
        Guid? CurrentStepID();

        [OperationContract(IsOneWay = true)]
        void DoProcess();

        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        List<string> GetWaitingOnUsers();

        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        ulong GetSerialState();

        // ---------- creature info
        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        CreatureInfo GetCreature(string id);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        List<Info> GetObjectLoadInfo(string id);

        // TODO: possession location info
        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        List<PossessionInfo> GetPossessionInfo(string id);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        List<ItemSlotInfo> GetSlottedItems(string id);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        List<AwarenessInfo> GetAwarenessInfo(string critterID, string sensorHostID);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        IEnumerable<ExtraInfoInfo> GetExtraInfos(string creatureID, string sensorHostID);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        List<AwarenessInfo> GetMasterAwarenesses(string[] ids);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void SetActiveInfo(string critterID, string objectID, string infoID);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        List<IdentityInfo> GetIdentityInfos(string critterID, string objectID);

        // TODO: extraInfoSet
        // TODO: infoKeys?

        // ---------- take 10
        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        Take10Info SetTake10(string id, DeltableInfo target, double duration);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        Take10Info SetAbilitiesTake10(string id, double duration);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        Take10Info SetSkillsTake10(string id, double duration);
    }
}
