using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo(@"Swim", MnemonicCode.Str, true, 2d)]
    public class SwimSkill : SkillBase
    {
        #region construction
        public SwimSkill(Creature skillUser)
            : base(skillUser)
        {
            // this skill itself provides the encumberance check again
            // since the encumberance check is already on the skill, ...
            // ... it is wrapped in a qualified delta sourced by the skill
            _Extra = new DeltableQualifiedDelta(0, @"Extra Check", this);
            _Extra.Deltas.Add(skillUser.EncumberanceCheck);
            this.Deltas.Add(_Extra);
        }
        #endregion

        #region private data
        private DeltableQualifiedDelta _Extra;
        #endregion

        #region public int? CurrentSwim { get; }
        public int? CurrentSwim
        {
            get
            {
                return Creature.Adjuncts.OfType<Swimming>()
                    .Where(_c => _c.IsActive && !_c.IsCheckExpired)
                    .Max(_c => (int?)_c.SuccessCheckTarget.Result);
            }
        }
        #endregion
    }
}
