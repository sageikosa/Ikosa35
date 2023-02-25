using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    public class AttackFeedback : InteractionFeedback, ISuccessIndicatorFeedback
    {
        #region construction
        public AttackFeedback(object source, bool hit)
            : base(source)
        {
            Hit = hit;
            CriticalHit = false;
        }

        public AttackFeedback(object source, bool hit, bool critical)
            : base(source)
        {
            Hit = hit;
            CriticalHit = critical;
        }
        #endregion

        /// <summary>Confirmed Hit</summary>
        public bool Hit { get; set; }

        /// <summary>Confirmed Critical Hit</summary>
        public bool CriticalHit { get; set; }

        /// <summary>No lines of effect between source and target</summary>
        public bool NoLines { get; set; }

        #region ISuccessIndicatorFeedback Members

        public bool Success { get { return Hit; } }

        #endregion
    }
}
