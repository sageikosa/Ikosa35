using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Any object in an area of magical darkness is hit with this adjunct.
    /// </summary>
    [Serializable]
    public class DarknessShrouded : Adjunct
    {
        #region Construction
        /// <summary>Setup the DarknessShrouded adjunct.</summary>
        public DarknessShrouded(object source)
            : base(source)
        {
        }
        #endregion

        public override bool IsProtected => true;

        public override object Clone()
            => new DarknessShrouded(Source);
    }
}
