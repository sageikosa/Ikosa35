using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Advancement
{
    public interface IAdvancementClass : ICreatureBind
    {
        IEnumerable<Type> ClassSkills();
        void IncreaseLevel(PowerDieCalcMethod method);
        bool IsClassSkill(SkillBase skill);
        string ClassName { get; }
        int SkillPointsPerLevel { get; }
        byte PowerDieSize { get; set; }
        double BABProgression { get; }
        bool HasGoodFortitude { get; }
        bool HasGoodReflex { get; }
        bool HasGoodWill { get; }
        int BaseAttackBonus { get; }
        int BaseAttackAt(int level);
        int FortitudeBonus { get; }
        int ReflexBonus { get; }
        int WillBonus { get; }
        int CurrentLevel { get; }
        int MaxLevel { get; }
        decimal OptionalFraction { get; }
        int LockedLevel { get; }
        IVolatileValue EffectiveLevel { get; }
        bool CanRemove();
        bool CanLockLevel(int level);
        IEnumerable<AdvancementRequirement> Requirements(int level);
        IEnumerable<IFeature> Features(int level);
        bool CanUnlockOneLevel(int level);
        ClassInfo ToClassInfo();
        string Key { get; }
    }

    public static class IAdvancementClassHelper
    {
        public static ClassInfo GetClassInfo(this IAdvancementClass self)
            => new ClassInfo
            {
                FullName = self.GetType().FullName,
                ClassName = self.ClassName,
                PowerDieSize = self.PowerDieSize,
                BaseAttackProgression = self.BABProgression,
                SkillPointsPerLevel = self.SkillPointsPerLevel,
                GoodFortitude = self.HasGoodFortitude,
                GoodReflex = self.HasGoodReflex,
                GoodWill = self.HasGoodWill,
                CurrentLevel = self.CurrentLevel,
                EffectiveLevel = self.EffectiveLevel.ToVolatileValueInfo()
            };
    }
}
