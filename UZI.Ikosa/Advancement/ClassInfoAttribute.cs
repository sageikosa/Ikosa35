using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Advancement
{
    [AttributeUsage(AttributeTargets.Class), Serializable]
    public class ClassInfoAttribute : Attribute
    {
        public ClassInfoAttribute(string className, byte powerDieSize, double baseAttackProgression,
            int skillPointsPerLevel, bool goodFortitude, bool goodReflex, bool goodWill)
        {
            ClassName = className;
            PowerDieSize = powerDieSize;
            BaseAttackProgression = baseAttackProgression;
            SkillPointsPerLevel = skillPointsPerLevel;
            GoodFortitude = goodFortitude;
            GoodReflex = goodReflex;
            GoodWill = goodWill;
        }

        public string ClassName { get; private set; }
        public byte PowerDieSize { get; private set; }
        public double BaseAttackProgression { get; private set; }
        public int SkillPointsPerLevel { get; private set; }
        public bool GoodFortitude { get; private set; }
        public bool GoodReflex { get; private set; }
        public bool GoodWill { get; private set; }

        public static ClassInfo GetClassInfo(Type classType, Creature critter)
        {
            var _currClass = critter?.Classes.FirstOrDefault(_c => _c.GetType() == classType);
            var _currentLevel = _currClass?.CurrentLevel ?? 0;
            var _effectiveLevel = _currClass?.EffectiveLevel?.ToVolatileValueInfo();
            return _currClass?.GetClassInfo()
                ?? classType.GetCustomAttributes(typeof(ClassInfoAttribute), false)
                .OfType<ClassInfoAttribute>()
                .Select(_cia => new ClassInfo
                {
                    FullName = classType.FullName,
                    ClassName = _cia.ClassName,
                    PowerDieSize = _cia.PowerDieSize,
                    BaseAttackProgression = _cia.BaseAttackProgression,
                    SkillPointsPerLevel = _cia.SkillPointsPerLevel,
                    GoodFortitude = _cia.GoodFortitude,
                    GoodReflex = _cia.GoodReflex,
                    GoodWill = _cia.GoodWill
                })
                .FirstOrDefault()
                ?? new ClassInfo
                {
                    FullName = classType.FullName,
                    ClassName = classType.Name,
                    PowerDieSize = 0,
                    BaseAttackProgression = 0.5d,
                    SkillPointsPerLevel = 2,
                    GoodFortitude = false,
                    GoodReflex = false,
                    GoodWill = false
                };
        }
    }
}
