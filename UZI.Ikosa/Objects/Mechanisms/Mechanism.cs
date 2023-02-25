using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Actions;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core.Dice;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public abstract class Mechanism : ObjectBase, IDisableable, ITacticalActionProvider
    {
        #region Construction
        protected Mechanism(string name, Material material, int seedDifficulty)
            : base(name, material)
        {
            _ACtrl = new ChangeController<Activation>(this, new Activation(this, false));
            _Difficulty = new Deltable(seedDifficulty);
            _Disablers = new Collection<Guid>();
            _DFCtrl = new ChangeController<DisableFail>(this, new DisableFail());
        }
        #endregion

        #region data
        private Deltable _Difficulty;
        private Collection<Guid> _Disablers;
        private ChangeController<DisableFail> _DFCtrl;
        private ChangeController<Activation> _ACtrl;
        #endregion

        #region public Activation Activation { get; set; }
        public Activation Activation
        {
            get => _ACtrl.LastValue;
            set
            {
                if ((_ACtrl.LastValue.Source != value.Source) || (_ACtrl.LastValue.IsActive != value.IsActive))
                {
                    // Ensure it is allowed (something may prevent a mechanism from activating/de-activating)
                    if (_ACtrl.WillAbortChange(value))
                    {
                        return;
                    }
                    if (value.IsActive)
                    {
                        if (!OnPreActivate())
                            return;
                    }
                    else
                    {
                        if (!OnPreDeactivate())
                            return;
                    }

                    // do pre-value changed
                    _ACtrl.DoPreValueChanged(value);

                    // do value changed
                    _ACtrl.DoValueChanged(value);
                    if (value.IsActive)
                    {
                        OnActivate();
                    }
                    else
                    {
                        OnDeactivate();
                    }

                    DoPropertyChanged(nameof(Activation));
                }
            }
        }
        #endregion

        /// <summary>Either has a DisabledObject adjunct, or has less than half structure points remaining</summary>
        public bool IsDisabled
            => this.HasAdjunct<DisabledObject>()
            || (StructurePoints < (MaxStructurePoints / 2));

        /// <summary>override and return false for the mechanism itself to veto the activation</summary>
        protected virtual bool OnPreActivate()
            => true;

        /// <summary>override and return false for the mechanism itself to veto the deactivation</summary>
        protected virtual bool OnPreDeactivate()
            => true;

        protected virtual void OnActivate() { }
        protected virtual void OnDeactivate() { }

        #region IDisableable Members

        #region IControlChange<Activation> Members
        public void AddChangeMonitor(IMonitorChange<Activation> monitor)
        {
            _ACtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<Activation> monitor)
        {
            _ACtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        public ActionTime ActionTime
            => (DisableDifficulty.EffectiveValue < 15)
            ? new ActionTime(Round.UnitFactor)
            : (DisableDifficulty.EffectiveValue < 20)
            ? new ActionTime(new Round(), new DieRoller(4), TimeType.Span)
            : new ActionTime(new Round(), new DiceRoller(2,4), TimeType.Span);

        /// <summary>Reported for ActionInfo (as max of possible rolls)</summary>
        public string TimeCostString
            => (DisableDifficulty.EffectiveValue < 15)
            ? @"1 round"
            : (DisableDifficulty.EffectiveValue < 20)
            ? @"4 rounds"
            : @"8 rounds";

        public Deltable DisableDifficulty => _Difficulty;
        public Collection<Guid> ConfusedDisablers => _Disablers;

        public virtual void FailedDisable(CoreActivity activity)
        {
            ConfusedDisablers.Add(activity.Actor.ID);
            _DFCtrl.DoValueChanged(new DisableFail());
        }

        #endregion

        public abstract IEnumerable<IActivatable> Dependents { get; }

        #region IActionProvider Members
        /// <summary>Equivalent to base.GetActions(), but safe for inheritance in generic enumerators</summary>
        protected IEnumerable<CoreAction> BaseMechanismActions(CoreActionBudget budget)
        {
            if (budget.Actor is Creature _critter)
            {
                if (_critter.Skills.Skill<DisableMechanismSkill>().IsTrained)
                {
                    var _budget = budget as LocalActionBudget;
                    var _canDo = (ActionTime.ActionTimeType == TimeType.Span || ActionTime.ActionTimeType == TimeType.Total) && _budget.CanPerformTotal;
                    if (!_canDo)
                        _canDo = _budget.CanPerformRegular;
                    if (!_canDo)
                        _canDo = (ActionTime.ActionTimeType == TimeType.Brief) && _budget.CanPerformBrief;
                    if (_canDo)
                        yield return new DisableMechanismAction(this, @"401");
                    if (_canDo)
                        yield return new EnableMechanismAction(this, @"402");
                }
            }
            yield break;
        }

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
            => GetTacticalActions(budget);
        #endregion

        #region IControlChange<DisableFail> Members

        public void AddChangeMonitor(IMonitorChange<DisableFail> monitor)
        {
            _DFCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<DisableFail> monitor)
        {
            _DFCtrl.RemoveChangeMonitor(monitor);
        }

        #endregion

        // IActionSource Members
        public IVolatileValue ActionClassLevel
            => new Deltable(1);

        // ITacticalActionProvider Members
        public virtual IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
            => BaseMechanismActions(budget);

        public bool IsContextMenuOnly => this.GetObjectBindings()
            .Any(_b => !(_b is ITacticalActionProvider)
            || ((_b as ITacticalActionProvider)?.IsContextMenuOnly ?? false)); // depends on what it's attached to

        public override IGeometricSize GeometricSize => Sizer.Size.CubeSize();

        public override bool IsTargetable => true;

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;
    }
}