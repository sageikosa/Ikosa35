using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    /// <summary>
    /// ClassSelection → AbilityBoost → HealthPoints → SkillAssignment 
    /// → FeatSelection → ClassRequirements → Complete
    /// </summary>
    public enum AdvancementStep
    {
        /// <summary>0 = Every advancement log item selects a class</summary>
        ClassSelection,
        /// <summary>1 = First power die step if ((next_power_die_count % 4) == 0)</summary>
        AbilityBoost,
        /// <summary>2 = Every power die get additional health points</summary>
        HealthPoints,
        /// <summary>3 = Every power die get additional skill points</summary>
        SkillAssignment,
        /// <summary>4 = Last power die step if ((next_power_die_count % 3) == 0)</summary>
        FeatSelection,
        /// <summary>5 = Optional step if level has requirements</summary>
        ClassRequirements,
        Complete
    }
}
