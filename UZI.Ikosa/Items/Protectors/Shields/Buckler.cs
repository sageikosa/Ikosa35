using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Items.Shields
{
    [ItemInfo(@"Buckler", @"AR:+1 Check:-1 SpellFail:5%", @"buckler"), Serializable]
    /// <summary>
    /// Shield that allows a weapon to be wielded also.  
    /// When equipped, a virtual "_FreeHand" is added to the item slots for the creature.
    /// </summary>
    public class Buckler : ShieldBase
    {
        public Buckler()
            : base(@"Buckler", false, 1, -1, 5, true)
        {
            Init(15m, 5d, Materials.SteelMaterial.Static, 5);
            Sizer.NaturalSize = Size.SmallerSize(Sizer.NaturalSize);
            _AtkDelta = new BucklerAttackPenalty(this);
            _Disabler = new BucklerDisabler(this);
        }

        #region data
        private ItemSlot _LastMain;
        private HoldingSlot _FreeHand;
        private BucklerAttackPenalty _AtkDelta;
        private BucklerDisabler _Disabler;
        #endregion

        public override int OpposedDelta => -4;
        public HoldingSlot FreeHand => _FreeHand;

        #region protected override void OnSetItemSlot()
        protected override void OnSetItemSlot()
        {
            base.OnSetItemSlot();

            // add item slot to creature
            if (MainSlot != null)
            {
                if (_LastMain != MainSlot)
                {
                    ReleaseFreeHand();
                }
                if (_FreeHand == null)
                {
                    _LastMain = MainSlot;
                    _FreeHand = new HoldingSlot(this, MainSlot.SubType, true);
                    CreaturePossessor.Body.ItemSlots.Add(_FreeHand);
                    CreaturePossessor.MeleeDeltable.Deltas.Add(_AtkDelta);
                    CreaturePossessor.RangedDeltable.Deltas.Add(_AtkDelta);
                    CreaturePossessor.OpposedDeltable.Deltas.Add(_AtkDelta);
                    CreaturePossessor.AddAdjunct(_Disabler);
                }
            }
        }
        #endregion

        #region protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        {
            var _reslot = GetReslotInfo(_FreeHand, slotA);
            base.OnClearSlots(slotA, slotB);
            ReslotItem(_reslot);
            ReleaseFreeHand();
        }
        #endregion

        private void ReleaseFreeHand()
        {
            // remove freehand item slot
            if (_FreeHand != null)
            {
                CreaturePossessor.Body.ItemSlots.Remove(_FreeHand);
            }
            _AtkDelta.DoTerminate();
            _FreeHand = null;
            _Disabler.Eject();
        }

        protected override string ClassIconKey => @"buckler";
    }

    [Serializable]
    public class BucklerAttackPenalty : IQualifyDelta
    {
        public BucklerAttackPenalty(Buckler buckler)
        {
            _Buckler = buckler;
            _Terminator = new TerminateController(this);
            _Delta = new QualifyingDelta(-1, typeof(BucklerAttackPenalty), @"Attack from Buckler Arm");
        }

        #region data
        private Buckler _Buckler;
        private IDelta _Delta;
        #endregion

        public Buckler Buckler => _Buckler;

        #region public IDelta QualifiedDelta(Qualifier qualify)
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // make sure it is an attack
            if (qualify is Interaction _workset)
            {
                if (_workset.InteractData is AttackData)
                {
                    // see if attack is sourced from a weapon in a buckler holding slot
                    if (_workset.Source is IWeaponHead _head)
                    {
                        if (_head.ContainingWeapon == Buckler.FreeHand.SlottedItem)
                        {
                            yield return _Delta;
                        }
                        else
                        {
                            // projectile, needing two hands
                            if ((_head.ContainingWeapon is IProjectileWeapon _projectile)
                                && _projectile.UsesTwoHands
                                && (_projectile is ISlottedItem _slotted))
                            {
                                // one slot is not filled
                                if ((_slotted.MainSlot == null) || (_slotted.SecondarySlot == null))
                                {
                                    // and there are no other empty slots besides this hand
                                    if (Buckler.CreaturePossessor.Body.ItemSlots.GetFreeHand(Buckler.FreeHand) == null)
                                    {
                                        yield return _Delta;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // default is penalty does not apply
            yield break;
        }
        #endregion

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  
        /// Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Terminator.DoTerminate();
        }

        #region IControlTerminate Members
        private readonly TerminateController _Terminator;
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
        #endregion
        #endregion
    }

    [Serializable]
    public class BucklerDisabler : Adjunct, ICanReactBySideEffect
    {
        public BucklerDisabler(Buckler buckler)
            : base(buckler)
        {
        }

        public Buckler Buckler => Source as Buckler;

        public bool IsFunctional => true;

        public override object Clone()
            => new BucklerDisabler(Buckler);

        public void ReactToProcessBySideEffect(CoreProcess process)
        {
            // attack activity
            if ((process is CoreActivity _activity)
                && (_activity.Action is ActionBase _action)
                && (_action is ISupplyAttackAction _atk))
            {
                // for either a weapon in buckler hand
                if ((_atk.Attack.Weapon == Buckler.FreeHand.SlottedItem)
                    // or a projectile weapon needing two hands
                    || ((_atk.Attack.Weapon is IProjectileWeapon _projectile)
                        && _projectile.UsesTwoHands
                        && (_projectile is ISlottedItem _slotted)
                        // and one of the projectile slots is empty
                        && ((_slotted.MainSlot == null) || (_slotted.SecondarySlot == null))
                        // and there are no other free slots except the buckler hand
                        && (Buckler.CreaturePossessor.Body.ItemSlots.GetFreeHand(Buckler.FreeHand) == null)))
                {
                    if (!Buckler.CreaturePossessor.Feats.Contains(typeof(ImprovedShieldBash)))
                    {
                        var _time = ((Buckler.Setting as ITacticalMap)?.CurrentTime ?? 0) + Round.UnitFactor;
                        Buckler.AddAdjunct(new CancelShield(Buckler, _time));
                    }
                }
            }
        }
    }
}
