using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class ImprovedUncannyDodge : Adjunct, IMonitorChange<InteractionAlteration>
    {
        public ImprovedUncannyDodge(IPowerClass powerClass)
            : base(powerClass)
        {
            // TODO:
        }

        public IPowerClass PowerClass
            => Source as IPowerClass;

        public override object Clone()
            => new ImprovedUncannyDodge(PowerClass);

        private Creature Critter
            => Anchor as Creature;

        // IMonitorChange<InteractionAlteration> members
        public void PreTestChange(object sender, AbortableChangeEventArgs<InteractionAlteration> args)
        {
            if (args.Action == AlterationSet.TargetAction)
            {
                var _flank = args.NewValue as FlankingAlteration;
                if (_flank != null)
                {
                    var _attacker = _flank.AttackData.Attacker as Creature;
                    if (_attacker != null)
                    {
                        // get all sneak attack effects
                        var _sneak = _attacker.Adjuncts.OfType<SneakAttack>().ToList();
                        if (_sneak.Any())
                        {
                            // qualifier to calculate (in case of qualified deltas)
                            var _qualifier = new Interaction(_flank.Flanker, this, Critter, _flank.AttackData);

                            // uncanny dodge levels stack to determine effective level sneaker has to overcome
                            var _total = (from _ud in Anchor.Adjuncts.OfType<ImprovedUncannyDodgeSupplier>()
                                          where _ud.IsActive && (_ud.PowerClass?.IsPowerClassActive ?? false)
                                          select _ud.PowerClass)
                                          .Sum(_pc => _pc.ClassPowerLevel.QualifiedValue(_qualifier));

                            // sneaker must overcome uncanny dodge class levels by 4
                            if (_sneak.Any(_s => _s.PowerClass.ClassPowerLevel.QualifiedValue(_qualifier) >= (_total + 4)))
                            {
                                // sneak attack power trumps improved uncanny dodge
                                return;
                            }
                        }
                    }

                    // improved uncanny dodge negates flanking
                    args.DoAbort(@"Improved Uncanny Dodge", this);
                }
            }
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<InteractionAlteration> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<InteractionAlteration> args)
        {
        }
    }

    [Serializable]
    public class ImprovedUncannyDodgeSupplier : Adjunct
    {
        public ImprovedUncannyDodgeSupplier(IPowerClass powerClass)
            : base(powerClass)
        {
        }

        public IPowerClass PowerClass
            => Source as IPowerClass;

        public override object Clone()
            => new ImprovedUncannyDodgeSupplier(PowerClass);
    }
}
