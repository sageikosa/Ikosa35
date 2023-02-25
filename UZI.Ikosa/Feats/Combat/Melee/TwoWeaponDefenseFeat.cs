using System;
using System.Linq;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Adjuncts;
using System.Collections.Generic;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    AbilityRequirement(Abilities.MnemonicCode.Dex, 15),
    FighterBonusFeat,
    FeatChainRequirement(typeof(TwoWeaponFightingFeat)),
    FeatInfo(@"Two-Weapon Defense")
    ]
    public class TwoWeaponDefenseFeat : FeatBase, IQualifyDelta
    {
        public TwoWeaponDefenseFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _Term = new TerminateController(this);
        }

        private readonly TerminateController _Term;

        public override string Benefit
        {
            get { return @"+1 Shield AR when wielding two weapons or a double weapon.  +2 if fighting defensively"; }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Creature.NormalArmorRating.Deltas.Add(this);

            // NOTE: consider the need for two/double ghost-touch weapons acting as a shield
            // Creature.IncorporealArmorRating.Deltas.Add(this);
        }

        protected override void OnDeactivate()
        {
            _Term.DoTerminate();
            base.OnDeactivate();
        }

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // make sure an attack
            if (qualify is Interaction _iAct)
            {
                if (_iAct.InteractData is AttackData _atk)
                {
                    // get distinct wielded weapons, and flag any active double weapon
                    var _weapons = (from _w in Creature.Body.ItemSlots.HeldObjects.OfType<IWeapon>()
                                    where _w.IsActive && !(_w is NaturalWeapon)
                                    let _dbl = _w as DoubleMeleeWeaponBase
                                    let _actDbl = (_dbl != null) && (_dbl.SecondarySlot != null) && !_dbl.UseAsTwoHanded
                                    select new { Weapon = _w, ActiveDouble = _actDbl }).ToList();
                    // ensure more than one weapon, or at least one active double weapon
                    if ((_weapons.Count > 1) || _weapons.Any(_wpn => _wpn.ActiveDouble))
                    {
                        // defensive combat gets extra shielding
                        var _dVal = Creature.HasActiveAdjunct<DefensiveCombat>() ? 2 : 1;
                        yield return new QualifyingDelta(_dVal, typeof(ShieldBase), @"Two Weapon Defense");
                    }
                }
            }
            yield break;
        }

        #endregion

        #region IControlTerminate Members

        public void DoTerminate()
        {
            _Term.DoTerminate();
        }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;

        #endregion
    }
}
