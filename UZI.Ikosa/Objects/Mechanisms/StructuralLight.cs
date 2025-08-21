using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Time;
using Uzi.Core.Dice;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    /// <summary>Light that is part of the environmental structure.</summary>
    [Serializable]
    public class StructuralLight : LocatableObject, IActivatable, IDisableable
    {
        #region construction
        public StructuralLight(string name, bool isVisible, Illumination light, bool active, int seedDifficulty)
            : base(name, isVisible)
        {
            _ACtrl = new ChangeController<Activation>(this, new Activation(this, active));
            _Difficulty = new Deltable(seedDifficulty);
            _Disablers = [];
            _DFCtrl = new ChangeController<DisableFail>(this, new DisableFail());

            // adjunct
            AddAdjunct(light);
            light.IsActive = active;
        }
        #endregion

        #region data
        private ChangeController<Activation> _ACtrl;
        private Deltable _Difficulty;
        private Collection<Guid> _Disablers;
        private ChangeController<DisableFail> _DFCtrl;
        #endregion

        /// <summary>Either has a DisabledObject adjunct, or has less than half structure points remaining</summary>
        public bool IsDisabled
            => this.HasAdjunct<DisabledObject>();

        #region IActivatable Members

        public Activation Activation
        {
            get => _ACtrl.LastValue;
            set
            {
                if ((_ACtrl.LastValue.Source != value.Source) && (_ACtrl.LastValue.IsActive != value.IsActive))
                {
                    // Ensure it is allowed (something may prevent a mechanism from activating/de-activating)
                    if (_ACtrl.WillAbortChange(value))
                    {
                        return;
                    }

                    // do pre-value changed
                    _ACtrl.DoPreValueChanged(value);

                    // do value changed
                    _ACtrl.DoValueChanged(value);
                    DoPropertyChanged(nameof(Activation));
                }
            }
        }

        #endregion

        #region IControlChange<Activation> Members

        public void AddChangeMonitor(IMonitorChange<Activation> monitor)
            => _ACtrl.AddChangeMonitor(monitor);

        public void RemoveChangeMonitor(IMonitorChange<Activation> monitor)
            => _ACtrl.RemoveChangeMonitor(monitor);

        #endregion

        #region IDisableable Members

        public ActionTime ActionTime
            => (DisableDifficulty.EffectiveValue < 15)
            ? new ActionTime(Round.UnitFactor)
            : (DisableDifficulty.EffectiveValue < 20)
            ? new ActionTime(Round.UnitFactor * DieRoller.RollDie(Guid.Empty, 4, @"Time", @"Rounds"))
            : new ActionTime(Round.UnitFactor * DiceRoller.RollDice(Guid.Empty, 2, 4, @"Time", @"Rounds"));

        /// <summary>Reported for ActionInfo (as max of possible rolls)</summary>
        public string TimeCostString
            => (DisableDifficulty.EffectiveValue < 15)
            ? @"1 round"
            : (DisableDifficulty.EffectiveValue < 20)
            ? @"4 rounds"
            : @"8 rounds";

        public Deltable DisableDifficulty => _Difficulty;
        public Collection<Guid> ConfusedDisablers => _Disablers;

        public void FailedDisable(CoreActivity activity)
        {
            ConfusedDisablers.Add(activity.Actor.ID);
            _DFCtrl.DoValueChanged(new DisableFail());
        }

        #endregion

        public IEnumerable<IActivatable> Dependents
        {
            get
            {
                yield break;
            }
        }

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

        public Illumination Illumination
            => Adjuncts.OfType<Illumination>().FirstOrDefault();

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => null;

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        // IActionSource Members
        public IVolatileValue ActionClassLevel
            => new Deltable(1);

        public override IGeometricSize GeometricSize
            => new GeometricSize(1, 1, 1);

        public override Sizer Sizer
            => new ObjectSizer(Size.Medium, this);

        protected override string ClassIconKey
            => nameof(StructuralLight);

        public override bool IsTargetable => true;
    }
}