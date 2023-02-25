using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Items.Shields
{
    [Serializable]
    public class ShieldDisabler : Adjunct, ICanReactBySideEffect
    {
        public ShieldDisabler(ShieldBase source)
            : base(source)
        {
        }

        public ShieldBase Shield => Source as ShieldBase;

        public bool IsFunctional => true;

        public override object Clone()
            => new ShieldDisabler(Shield);

        public void ReactToProcessBySideEffect(CoreProcess process)
        {
            // attack activity
            if ((process is CoreActivity _activity)
                && (_activity.Action is ActionBase _action)
                && (_action is ISupplyAttackAction _atk))
            {
                // for either a weapon in buckler hand
                if ((_atk.Attack.Weapon == Shield) && !Shield.CreaturePossessor.Feats.Contains(typeof(ImprovedShieldBash)))
                {
                    var _time = ((Shield.Setting as ITacticalMap)?.CurrentTime ?? 0) + Round.UnitFactor;
                    Shield.AddAdjunct(new CancelShield(Shield, _time));
                }
            }
        }
    }
}
