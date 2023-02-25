using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Feats
{
    [Serializable]
    [FighterBonusFeat]
    [AbilityRequirement(MnemonicCode.Str, 13)]
    [FeatChainRequirement(typeof(PowerAttackFeat))]
    [FeatInfo(@"Cleave")]
    public class CleaveFeat : FeatBase, IMonitorChange<Body>, IMonitorChange<ISlottedItem>
    {
        public CleaveFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _Capacity = new Deltable(1);
        }

        #region data
        private Deltable _Capacity;
        #endregion

        public Deltable Capacity => _Capacity;

        public override string Benefit => @"Dealing enough damage to 'drop' a creature allows an immediate extra attack in melee range";

        protected override void OnActivate()
        {
            Creature.BodyDock.AddChangeMonitor(this);
            AttachBody(Creature.Body);
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            DetachBody(Creature.Body);
            Creature.BodyDock.RemoveChangeMonitor(this);
            base.OnDeactivate();
        }

        #region Body swapping
        /// <summary>need to detach body item slots when no longer in use</summary>
        private void DetachBody(Body body)
        {
            foreach (var _slot in body.ItemSlots.AllSlots.OfType<HoldingSlot>())
            {
                if (_slot.SlottedItem is IMeleeWeapon _wpn)
                {
                    foreach (var _cr in (from _h in _wpn.AllHeads
                                         from _a in _h.Adjuncts.OfType<CleaveReactor>()
                                         select _a).ToList())
                    {
                        _cr.Eject();
                    }
                }

                // unlink callback
                _slot.RemoveChangeMonitor(this);
            }
        }

        /// <summary>need to attach a body to monitor item slots for changes</summary>
        private void AttachBody(Body body)
        {
            foreach (var _slot in body.ItemSlots.AllSlots.OfType<HoldingSlot>())
            {
                // found a holding slot
                _slot.AddChangeMonitor(this);

                if (_slot.SlottedItem is IMeleeWeapon _wpn)
                {
                    foreach (var _head in _wpn.AllHeads)
                    {
                        _head.AddAdjunct(new CleaveReactor(this));
                    }
                }
            }
        }
        #endregion

        #region IMonitorChange<Body> Members
        void IMonitorChange<Body>.PreValueChanged(object sender, ChangeValueEventArgs<Body> args)
        {
        }

        /// <summary>when a creature's body changes, re-do the event hooks and look for items in slots</summary>
        void IMonitorChange<Body>.ValueChanged(object sender, ChangeValueEventArgs<Body> args)
        {
            // body change (with size change and extra-carry change) effects load limits
            DetachBody(args.OldValue);
            AttachBody(args.NewValue);
        }

        void IMonitorChange<Body>.PreTestChange(object sender, AbortableChangeEventArgs<Body> args)
        {
        }
        #endregion

        #region IMonitorChange<ISlottedItem> Members
        public void ValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args)
        {
            // if anything (armor or shields) changes
            if (args.OldValue is IMeleeWeapon _oldWeapon)
            {
                // old item being removed, de-power item
                foreach (var _cr in (from _h in _oldWeapon.AllHeads
                                     from _a in _h.Adjuncts.OfType<CleaveReactor>()
                                     select _a).ToList())
                {
                    _cr.Eject();
                }
            }
            if (args.NewValue is IMeleeWeapon _newWeapon)
            {
                // item being removed, power-up item
                foreach (var _head in _newWeapon.AllHeads)
                {
                    _head.AddAdjunct(new CleaveReactor(this));
                }
            }
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args)
        {
        }

        public void PreTestChange(object sender, AbortableChangeEventArgs<ISlottedItem> args)
        {
        }
        #endregion
    }
}
