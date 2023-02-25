using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Materials;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    /// <summary>
    /// Blocks self-sourced Open/Close actions from an IOpenable.
    /// Supplies its own open/close action, which can suppress one lock during the opening.
    /// </summary>
    [Serializable]
    public class RelockingOpener : OpenerCloser, ILockMechanism
    {
        #region Construction
        public RelockingOpener(string name, Material material, int disableDifficulty)
            : base(name, material, disableDifficulty)
        {
        }
        #endregion

        #region public LockGroup LockGroup { get; }
        public LockGroup LockGroup
        {
            get => (from _lm in Adjuncts.OfType<LockMechanism>()
                    select _lm.LockGroup).FirstOrDefault();
            set
            {
                // remove old lock
                var _lockMech = Adjuncts.OfType<LockMechanism>().FirstOrDefault();
                _lockMech?.Eject();

                // add new lock
                if (value != null)
                {
                    _lockMech = new LockMechanism(value);
                    AddAdjunct(_lockMech);
                }
            }
        }
        #endregion

        #region public override IOpenable Openable { get; set; }
        public override IOpenable Openable
        {
            get => base.Openable;
            set
            {
                if (value != Openable)
                {
                    // clear old lock group
                    LockGroup = null;
                    if (value != null)
                    {
                        // make a new lock group
                        var _group = new LockGroup(@"Lock", false, true);
                        value.AddAdjunct(new LockTarget(_group));
                        LockGroup = _group;
                    }

                    // and base openable...
                    base.Openable = value;
                }
            }
        }
        #endregion

        #region IActionProvider Members
        public override IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
            if (LockGroup != null)
            {
                var _budget = budget as LocalActionBudget;

                if (Activation.IsActive && _budget.CanPerformBrief)
                {
                    yield return new OpenCloseLockedAction(this, Openable, LockGroup, @"101");
                }
            }
            foreach (var _act in BaseMechanismActions(budget))
                yield return _act;
            yield break;
        }
        #endregion
    }
}
