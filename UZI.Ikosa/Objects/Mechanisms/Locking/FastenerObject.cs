using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Objects
{
    /// <summary>
    /// If active, allows opening and closing.  When opened, unblocks its IOpenable target.  When closed, blocks its IOpenable target.  May be blocked itself.
    /// </summary>
    [Serializable]
    public abstract class FastenerObject : Mechanism, IOpenable, IForceOpenTarget
    {
        #region ctor()
        public FastenerObject(string name, Material material, int disableDifficulty, bool allowClose)
            : base(name, material, disableDifficulty)
        {
            // fastener aspect (can be used to fasten things)
            var _fast = new Fastener(this);
            AddAdjunct(_fast);

            _Block = new OpenBlocked(this, this, allowClose);
            _OpenState = this.GetOpenStatus(null, this, 1);
            _OCtrl = new ChangeController<OpenStatus>(this, _OpenState);
            Activation = new Activation(this, true);
        }
        #endregion

        #region Data
        protected OpenBlocked _Block;
        private OpenStatus _OpenState;
        private ChangeController<OpenStatus> _OCtrl;
        #endregion

        protected abstract IOpenable FastenTarget { get; }

        public override IEnumerable<IActivatable> Dependents { get { yield break; } }

        #region public override IEnumerable<CoreAction> GetActions(CoreActor actor)
        protected IEnumerable<CoreAction> BaseFastenerActions(LocalActionBudget budget)
        {
            if (Activation.IsActive && budget.CanPerformBrief)
                yield return new OpenCloseAction(this, this, @"101");
            yield break;
        }

        public override IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            return BaseFastenerActions(_budget).Union(BaseMechanismActions(_budget));
        }
        #endregion

        public bool AllowClose { get => _Block.CanClose; set => _Block.CanClose = value; }

        protected virtual void OnOpen()
        {
            // an open fastener no longer blocks its target
            FastenTarget?.RemoveAdjunct(_Block);
        }

        protected virtual void OnClose()
        {
            // a closed fastener no longer blocks its target
            FastenTarget?.AddAdjunct(_Block);
        }

        /// <summary>override to check for more than just the openStatus change controller</summary>
        protected virtual bool OnWillAbort(OpenStatus openState)
            => _OCtrl.WillAbortChange(openState);

        #region IOpenable Members
        public bool CanChangeOpenState(OpenStatus testValue)
            => !_OCtrl.WillAbortChange(testValue);

        public OpenStatus OpenState
        {
            get => _OpenState;
            set
            {
                var _oldVal = _OpenState;
                if (!OnWillAbort(value))
                {
                    // change
                    _OCtrl.DoPreValueChanged(value);
                    _OpenState = value;
                    _OCtrl.DoValueChanged(value);
                    DoPropertyChanged(nameof(OpenState));

                    // perform fastener behavior
                    if (_oldVal.IsClosed && !value.IsClosed)
                        OnOpen();
                    if (!_oldVal.IsClosed && value.IsClosed)
                        OnClose();
                }
            }
        }

        public double OpenWeight { get => Weight; set => DoPropertyChanged(nameof(OpenWeight)); }
        #endregion

        #region IControlChange<OpenStatus> Members

        public void AddChangeMonitor(IMonitorChange<OpenStatus> monitor)
        {
            _OCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<OpenStatus> monitor)
        {
            _OCtrl.RemoveChangeMonitor(monitor);
        }

        #endregion

        // IForceOpenTarget
        public void DoForcedOpen()
        {
            // destroy mechanism
            // TODO: damage and deactivate?
            this.DoDestruction();
        }

        public override int StructurePoints
        {
            get => base.StructurePoints;
            set
            {
                if (value <= 0)
                {
                    _Block?.Eject();
                }
                base.StructurePoints = value;
            }
        }
    }
}
