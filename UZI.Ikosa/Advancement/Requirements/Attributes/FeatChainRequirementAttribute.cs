using System;
using Uzi.Ikosa.Feats;
using Uzi.Core;

namespace Uzi.Ikosa.Advancement
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true), Serializable]
    public class FeatChainRequirementAttribute: RequirementAttribute
    {
        public FeatChainRequirementAttribute(Type featType)
        {
            if (featType.IsSubclassOf(typeof(FeatBase)))
            {
                FeatType = featType;
            }
            else throw new ArgumentOutOfRangeException("featType", "Must derive from FeatBase");
        }

        public readonly Type FeatType;

        public override string Name
        {
            get { return FeatBase.FeatName(FeatType); }
        }

        public override string Description
        {
            get { return string.Format(@"Must already have the {0} feat.", Name); }
        }

        public override bool MeetsRequirement(Creature creature, int powerLevel)
        {
            return (creature.Feats.Contains(FeatType, powerLevel));
        }

        public override bool MeetsRequirement(Creature creature)
        {
            return (creature.Feats.Contains(FeatType));
        }

        public override RequirementMonitor CreateMonitor(IRefreshable target, Creature owner)
        {
            return new FeatChainRequirementMonitor(this, target, owner);
        }
    }
}
