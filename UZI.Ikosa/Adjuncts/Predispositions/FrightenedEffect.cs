using System;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>-2 on attack rolls, saves, skill checks and ability checks.  Must flee.</summary>
    [Serializable]
    public class FrightenedEffect : ShakenEffect
    {
        public FrightenedEffect(object source)
            : base(source)
        {
        }

        protected override string ConditionString => Condition.Frightened;
        public override string Description => @"Frightened (must flee).";
    }
}
