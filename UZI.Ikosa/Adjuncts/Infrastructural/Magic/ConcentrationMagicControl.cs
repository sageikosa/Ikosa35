using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Provides link between an actor and a concentration magic effect so the spell can be concentrated on to maintain.
    /// If either leaves the plane (including ethereal) the link is broken.
    /// </summary>
    [Serializable]
    public class ConcentrationMagicControl : AdjunctGroup
    {
        /// <summary>
        /// Provides link between an actor and a concentration magic effect so the spell can be concentrated on to maintain.
        /// If either leaves the plane (including ethereal) the link is broken.
        /// </summary>
        public ConcentrationMagicControl()
            : base(typeof(ConcentrationMagicControl))
        {
        }

        public ConcentrationMagicMaster Master => Members.OfType<ConcentrationMagicMaster>().FirstOrDefault();
        public ConcentrationMagicEffect Target => Members.OfType<ConcentrationMagicEffect>().FirstOrDefault();

        public override void ValidateGroup()
        {
            this.ValidateOneToOnePlanarGroup();
        }
    }
}
