using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CreatureInfo
    {
        [DataMember]
        public AbilitySetInfo Abilities { get; set; }
        [DataMember]
        public double Age { get; set; }
        [DataMember]
        public double AgeInYears { get; set; }
        [DataMember]
        public string Alignment { get; set; }
        [DataMember]
        public DeltableInfo BaseAttack { get; set; }
        [DataMember]
        public BodyInfo Body { get; set; }
        [DataMember]
        public CarryCapacityInfo CarryingCapacity { get; set; }
        [DataMember]
        public Collection<ClassInfo> Classes { get; set; }

        // TODO: combat action list

        [DataMember]
        public Collection<string> Conditions { get; set; }
        [DataMember]
        public string CreatureType { get; set; }
        [DataMember]
        public Collection<string> DamageReductions { get; set; }
        [DataMember]
        public string Devotion { get; set; }
        [DataMember]
        public EncumberanceInfo Encumberance { get; set; }
        [DataMember]
        public Collection<string> EnergyResistances { get; set; }
        [DataMember]
        public Collection<FeatInfo> Feats { get; set; }
        [DataMember]
        public DeltableInfo FortitudeSave { get; set; }
        [DataMember]
        public string Gender { get; set; }
        [DataMember]
        public HealthPointInfo HealthPoints { get; set; }
        [DataMember]
        public DeltableInfo IncorporealArmorRating { get; set; }
        [DataMember]
        public DeltableInfo Initiative { get; set; }
        [DataMember]
        public Collection<string> Languages { get; set; }
        [DataMember]
        public double LoadWeight { get; set; }
        [DataMember]
        public int MaxDexterityToArmorRating { get; set; }
        [DataMember]
        public DeltableInfo MeleeDeltable { get; set; }
        [DataMember]
        public Collection<MovementInfo> Movements { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public DeltableInfo NormalArmorRating { get; set; }
        [DataMember]
        public DeltableInfo OpposedDeltable { get; set; }
        [DataMember]
        public Collection<string> Proficiencies { get; set; }
        [DataMember]
        public DeltableInfo RangedDeltable { get; set; }
        [DataMember]
        public DeltableInfo ReflexSave { get; set; }
        [DataMember]
        public Collection<string> Senses { get; set; }
        [DataMember]
        public Collection<SkillInfo> BasicSkills { get; set; }
        [DataMember]
        public Collection<SkillInfo> ParameterizedSkills { get; set; }
        [DataMember]
        public Take10Info SkillTake10 { get; set; }
        [DataMember]
        public string Species { get; set; }
        [DataMember]
        public DeltableInfo SpellResistance { get; set; }
        [DataMember]
        public Collection<string> SubTypes { get; set; }
        [DataMember]
        public Collection<string> Templates { get; set; }
        [DataMember]
        public DeltableInfo TouchArmorRating { get; set; }
        [DataMember]
        public Collection<TraitInfo> Traits { get; set; }
        [DataMember]
        public double Weight { get; set; }
        [DataMember]
        public DeltableInfo WillSave { get; set; }
        [DataMember]
        public string[] ImageKeys { get; set; }
    }
}