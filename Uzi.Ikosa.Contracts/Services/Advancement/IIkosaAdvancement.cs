using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Uzi.Core.Contracts.Faults;

namespace Uzi.Ikosa.Contracts
{
    [ServiceContract(Namespace = Statics.ServiceNamespace)]
    [ServiceKnownType(@"KnownTypes", typeof(IkosaKnownTypes))]
    public interface IIkosaAdvancement
    {
        // available advancement creatures (for user, including master)
        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        List<AdvanceableCreature> GetAdvanceableCreatures();

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        List<ClassInfo> GetAvailableClasses(Guid id);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        List<AdvancementOptionInfo> GetAvailableFeats(Guid id, int powerLevel);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        List<SkillInfo> GetAvailableSkills(Guid id, int powerLevel);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        AbilitySetInfo GetBoostableAbilities(Guid id, int powerLevel);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        AdvanceableCreature PushClassLevel(Guid id, ClassInfo classInfo);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        AdvanceableCreature PopClassLevel(Guid id);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        AdvanceableCreature LockClassLevel(Guid id);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        AdvanceableCreature UnlockClassLevel(Guid id);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        AdvanceableCreature SetAdvancementOption(Guid id, AdvancementRequirementInfo requirement, AdvancementOptionInfo option);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        AdvanceableCreature SetHealthPoints(Guid id, int powerLevel, int hitPoints);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        AdvanceableCreature SetAbilityBoost(Guid id, int powerLevel, string mnemonic);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        AdvanceableCreature SetFeat(Guid id, int powerLevel, AdvancementOptionInfo feat);

        [OperationContract]
        [FaultContract(typeof(SecurityFault)), FaultContract(typeof(InvalidArgumentFault))]
        AdvanceableCreature SetSkillPoints(Guid id, int powerLevel, SkillInfo skill, int points);
    }
}
