using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class SmiteTrait : TraitEffect, IActionProvider, IMonitorChange<CoreActivity>
    {
        public SmiteTrait(ITraitSource traitSource, Alignment alignment)
            : base(traitSource)
        {
            _Alignment = alignment;
            _Battery = null;
            _IsSmiting = false;
            _MeleeOnly = true;
            _NaturalOnly = true;
            _Delta = new SmiteDelta(this);
        }

        #region data
        private Alignment _Alignment;
        private IPowerBattery _Battery;
        private bool _IsSmiting;
        private bool _MeleeOnly;
        private bool _NaturalOnly;
        private SmiteDelta _Delta;
        #endregion

        public Alignment Alignment => _Alignment;
        public IPowerBattery PowerBattery => _Battery;
        public SmiteDelta SmiteDelta => _Delta;
        public bool IsMeleeOnly { get => _MeleeOnly; set => _MeleeOnly = value; }
        public bool IsNaturalOnly { get => _NaturalOnly; set => _NaturalOnly = value; }

        public int SmiteDamage
            => Math.Min(((TraitSource as ITraitPowerClassSource)?.PowerClass?.ClassPowerLevel
                ?? Creature.ActionClassLevel).QualifiedValue(null), 20);

        #region public bool IsSmiting { get; set; }
        public bool IsSmiting
        {
            get => _IsSmiting;
            set
            {
                if (value != _IsSmiting)
                {
                    if (!value)
                    {
                        Creature.ExtraWeaponDamage.Deltas.Add(SmiteDelta);
                        Creature.RemoveChangeMonitor(this);
                    }
                    else
                    {
                        // must be able to use charges to turn ON
                        if (!PowerBattery.CanUseCharges(1))
                            return;

                        SmiteDelta?.DoTerminate();
                        Creature.AddChangeMonitor(this);
                    }
                    _IsSmiting = value;
                }
            }
        }
        #endregion

        public override TraitEffect Clone(ITraitSource traitSource)
            => new SmiteTrait(traitSource, Alignment);

        public override object Clone()
            => new SmiteTrait(TraitSource, Alignment);

        #region OnActivate
        protected override void OnActivate(object source)
        {
            Creature.Actions.Providers.Add(this, this);
            _Battery ??= new FullResetBattery(TraitSource, 1, Day.UnitFactor,
                (Creature?.Setting as ITacticalMap)?.EndOfDay ?? 0);
            Creature?.AddAdjunct(_Battery as Adjunct);
            base.OnActivate(source);
        }
        #endregion

        #region OnDeactivate
        protected override void OnDeactivate(object source)
        {
            (_Battery as Adjunct)?.Eject();
            SmiteDelta?.DoTerminate();
            Creature.RemoveChangeMonitor(this);
            if (Creature?.Actions.Providers.ContainsKey(this) ?? false)
                Creature.Actions.Providers.Remove(this);
            base.OnDeactivate(source);
        }
        #endregion

        #region IMonitorChange<CoreActivity> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<CoreActivity> args) { }

        public void PreValueChanged(object sender, ChangeValueEventArgs<CoreActivity> args)
        {
            if (args.Action.Equals(@"Stop", StringComparison.OrdinalIgnoreCase))
            {
                if (args.NewValue.Action is ISupplyAttackAction _atkAct)
                {
                    if (IsMeleeOnly && !(_atkAct.Attack.Weapon is IMeleeWeapon))
                        return;
                    if (IsNaturalOnly && !(_atkAct.Attack.Weapon is NaturalWeapon))
                        return;

                    // consume use
                    PowerBattery.UseCharges(1);
                    IsSmiting = false;
                }
            }
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<CoreActivity> args) { }

        #endregion

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (PowerBattery.CanUseCharges(1))
                yield return new SmiteChoice(this);
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo($@"Smite {Alignment.NoNeutralString()}", ID);
    }

    [Serializable]
    public class SmiteDelta : IQualifyDelta
    {
        public SmiteDelta(SmiteTrait smiteTrait)
        {
            _SmiteTrait = smiteTrait;
            _Terminator = new TerminateController(this);
        }

        #region data
        private SmiteTrait _SmiteTrait;
        private TerminateController _Terminator;
        #endregion

        public SmiteTrait SmiteTrait => _SmiteTrait;

        #region public IEnumerable<IDelta> QualifiedDelta(Qualifier qualify)
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // make sure it is an attack
            if (qualify is Interaction _workset)
            {
                if (_workset.InteractData is AttackData)
                {
                    // see if attack applies (with a qualifying weapon against an appropriately aligned foe)
                    if (_workset.Source is IWeaponHead _head)
                    {
                        if (SmiteTrait.IsMeleeOnly && !(_head.ContainingWeapon is IMeleeWeapon))
                            yield break;
                        if (SmiteTrait.IsNaturalOnly && !(_head.ContainingWeapon is NaturalWeapon))
                            yield break;

                        if (_workset.Target is Creature _critter
                            && _critter.Alignment.IsMatchingAxial(SmiteTrait.Alignment))
                        {
                            yield return new QualifyingDelta(
                                  Math.Min(((SmiteTrait.TraitSource as ITraitPowerClassSource)?.PowerClass?.ClassPowerLevel
                                  ?? _critter.ActionClassLevel).QualifiedValue(qualify), 20), this, @"Smite");
                        }
                    }
                }
            }

            // default is damage does not apply
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
}
