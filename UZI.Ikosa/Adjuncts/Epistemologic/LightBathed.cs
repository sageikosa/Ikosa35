using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Any object in an area of magical light is hit with this adjunct
    /// </summary>
    [Serializable]
    public class LightBathed: Adjunct
    {
        #region Construction
        /// <summary>
        /// Setup the LightBathed adjunct.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="shadowyRange">distance for the normal vision shadowy range</param>
        public LightBathed(object source)
            : base(source)
        {
        }
        #endregion

        public override object Clone()
        {
            return new LightBathed(Source);
        }
    }
}
