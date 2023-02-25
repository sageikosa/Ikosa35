using System;
using Uzi.Core;

namespace Uzi.Ikosa.Advancement
{
    [AttributeUsage(AttributeTargets.Class), Serializable]
    public abstract class RequirementAttribute: Attribute
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual bool MeetsRequirement(Creature creature, int powerLevel)
        {
            return MeetsRequirement(creature);
        }
        public virtual bool MeetsRequirement(Creature creature)
        {
            return true;
        }
        public abstract RequirementMonitor CreateMonitor(IRefreshable target, Creature owner);
    }
}
