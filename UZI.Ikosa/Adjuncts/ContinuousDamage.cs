using System;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class ContinuousDamage : Adjunct
    {
        #region construction
        public ContinuousDamage(object source, int amount)
            : base(source)
        {
            _Amount = amount;
        }
        #endregion

        #region private data
        private int _Amount;
        #endregion

        public int Amount { get { return _Amount; } }

        public override object Clone()
        {
            return new ContinuousDamage(Source, Amount);
        }

        public static int Total(IAdjunctable anchor)
        {
            if (anchor != null)
            {
                return anchor.Adjuncts.OfType<ContinuousDamage>().Sum(_cd => _cd.Amount);
            }
            return 0;
        }
    }
}
