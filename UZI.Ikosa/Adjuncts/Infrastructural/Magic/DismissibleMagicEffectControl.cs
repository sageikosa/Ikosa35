using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Provides link between an actor and a durable magic effect so the spell can be dismissed.
    /// If either leaves the plane (including ethereal) the link is broken.
    /// </summary>
    [Serializable]
    public class DismissibleMagicEffectControl : AdjunctGroup
    {
        /// <summary>
        /// Provides link between an actor and a durable magic effect so the spell can be dismissed.
        /// If either leaves the plane (including ethereal) the link is broken.
        /// </summary>
        public DismissibleMagicEffectControl()
            : base(typeof(DismissibleMagicEffectControl))
        {
        }

        public DismissibleMagicEffectMaster Master => Members.OfType<DismissibleMagicEffectMaster>().FirstOrDefault();
        public DismissibleMagicEffectTarget Target => Members.OfType<DismissibleMagicEffectTarget>().FirstOrDefault();

        public static void CreateControl(DurableMagicEffect durable, CoreActor actor, IAdjunctable target)
        {
            // add a dismiss controller if needed
            if ((actor != null) && (target != null))
            {
                var _control = new DismissibleMagicEffectControl();
                actor?.AddAdjunct(new DismissibleMagicEffectMaster(_control));
                target?.AddAdjunct(new DismissibleMagicEffectTarget(durable, _control));
            }
        }

        public override void ValidateGroup()
        {
            this.ValidateOneToOnePlanarGroup();
        }
    }
}
