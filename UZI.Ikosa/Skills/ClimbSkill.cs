using System;
using System.Linq;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Skills
{
    /// <summary>STR; untrained; check</summary>
    [Serializable, SkillInfo(@"Climb", MnemonicCode.Str, true, 1d)]
    public class ClimbSkill : SkillBase
    {
        #region construction
        public ClimbSkill(Creature skillUser)
            : base(skillUser)
        {
        }
        #endregion

        #region public int? CurrentClimb { get; }
        public int? CurrentClimb
        {
            get
            {
                return Creature.Adjuncts.OfType<Climbing>()
                    .Where(_c => _c.IsActive && !_c.IsCheckExpired)
                    .Max(_c => (int?)_c.SuccessCheckTarget.Result);
            }
        }
        #endregion
    }
}
