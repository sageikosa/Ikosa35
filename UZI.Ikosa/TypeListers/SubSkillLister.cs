using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Skills;
using System.Reflection;
using System.Collections.Concurrent;

namespace Uzi.Ikosa.TypeListers
{
    public class SubSkillLister
    {
        public static IEnumerable<SkillBase> SubSkills<SubBase>(Type mainSkill, Creature creature)
            where SubBase : SubSkillBase
        {
            Type _subType = typeof(SubBase);
            return from _focus in Assembly.GetAssembly(typeof(SkillBase)).GetTypes()
                   where _focus.IsPublic && !_focus.IsAbstract && _focus.IsSubclassOf(_subType)
                   select (SkillBase)Activator.CreateInstance(mainSkill.MakeGenericType(_focus), creature);
        }

        private static ConcurrentDictionary<Type, List<Type>> _SubSkills
            = new ConcurrentDictionary<Type, List<Type>>();

        public static IEnumerable<Type> SubSkillTypes<SubBase>(Type mainSkill)
            where SubBase : SubSkillBase
        {
            var _subType = typeof(SubBase);
            return _SubSkills.GetOrAdd(
                mainSkill,
                (ms) => (from _focus in Assembly.GetAssembly(typeof(SkillBase)).GetTypes()
                         where _focus.IsPublic && !_focus.IsAbstract && _focus.IsSubclassOf(_subType)
                         select mainSkill.MakeGenericType(_focus)).ToList())
                         .Select(_s => _s);
        }
    }
}
