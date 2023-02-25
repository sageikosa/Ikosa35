using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Uzi.Core.Contracts.Faults;
using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Contracts
{
    [ServiceContract(Namespace = Statics.ServiceNamespace, CallbackContract = typeof(IMasterControlCallback))]
    [ServiceKnownType(@"KnownTypes", typeof(IkosaKnownTypes))]
    public interface IMasterControl
    {
        // ---------- callback control
        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void RegisterCallback();

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void DeRegisterCallback();

        // ---------- advancement control
        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void AllowCreatureAdvancement(Guid id);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void RevokeCreatureAdvancement(Guid id);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        List<CreatureLoginInfo> GetAdvancementCreatures(bool isAdvancing);

        // ---------- user control
        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void KickUser(string user);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void SetUserDisabled(string user, bool isDisabled);

        // ---------- flow control
        [OperationContract(IsOneWay = true)]
        //[FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void SetFlowState(FlowState flowState);

        [OperationContract(IsOneWay = true)]
        //[FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void SetPause(bool isPaused);

        // -- auto = time ticks progress roughly with real-time clock
        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        void SetIsTimeTickAuto(bool isAuto);

        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        bool PushTimeTick();

        // ---------- turn-tracker controls
        [OperationContract(IsOneWay = true)]
        //[FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void TurnTrackerStart(Guid[] creatures);

        [OperationContract(IsOneWay = true)]
        //[FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void TurnTrackerAdd(Guid[] creatures);

        [OperationContract(IsOneWay = true)]
        //[FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void TurnTrackerDrop(Guid[] creatures);

        [OperationContract(IsOneWay =true)]
        //[FaultContract(typeof(SecurityFault))]
        void TurnTrackerStop(string standDownGroupName);

        // ---------- needs-turn-tick control
        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        void AddNeedsTurnTick(Guid creatureID);

        // TODO: surprise round tracker...

        // ---------- stand down groups
        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        List<StandDownGroupInfo> GetStandDownGroups();

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        StandDownGroupInfo AddToStandDownGroup(Guid standDowngroupID, string groupName, Guid[] creatures);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        StandDownGroupInfo RemoveFromStandDownGroup(Guid standDowngroupID, Guid[] creatures);

        // ---------- social group inspection
        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        List<TeamGroupInfo> GetTeams();

        // TODO: social group inspection...
    }
}
