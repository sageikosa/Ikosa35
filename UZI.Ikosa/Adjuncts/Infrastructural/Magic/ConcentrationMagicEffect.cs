using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public abstract class ConcentrationMagicEffect : GroupMemberAdjunct
    {
        protected ConcentrationMagicEffect(MagicPowerEffect magicPowerEffect, ConcentrationMagicControl group)
            : base(magicPowerEffect, group)
        {
        }

        public ConcentrationMagicControl Control => Group as ConcentrationMagicControl;
        public MagicPowerEffect MagicPowerEffect => Source as MagicPowerEffect;

        protected override void OnDeactivate(object source)
        {
            // disable fragile magic if disabled
            if (MagicPowerEffect is FragileMagicEffect _fragile)
            {
                if (_fragile.IsActive)
                {
                    // if fragile was still active, we were ejected by the group, so break the magic power effect
                    _fragile.Activation = new Activation(this, false);
                }
            }
            base.OnDeactivate(source);
        }
    }
}
