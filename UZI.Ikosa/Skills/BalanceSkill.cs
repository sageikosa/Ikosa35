using System;
using System.Linq;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Skills
{
    /// <summary>DEX; untrained; check</summary>
    [Serializable, SkillInfo("Balance", "DEX", true, 1d)]
    public class BalanceSkill : SkillBase 
    {
        public BalanceSkill(Creature skillUser)
            : base(skillUser)
        {
        }

        public int? CurrentBalance
        {
            get
            {
                return Creature.Adjuncts.OfType<Balancing>()
                    .Where(_c => _c.IsActive && !_c.IsCheckExpired)
                    .Max(_c => (int?)_c.SuccessCheckTarget.Result);
            }
        }

        /// <summary>Returns true if accelerated balancing, or if not balancing</summary>
        public bool IsSpeedNormal
        {
            get
            {
                var _balance = Creature.Adjuncts.OfType<Balancing>().FirstOrDefault(_a => _a.IsActive);
                if (_balance == null)
                    return true;
                return _balance.SuccessCheckTarget.IsUsingPenalty;
            }
        }
    }
}
