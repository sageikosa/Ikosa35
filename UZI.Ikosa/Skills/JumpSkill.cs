using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Skills
{
    /// <summary>STR; untrained; check</summary>
    [Serializable, SkillInfo("Jump", "STR", true, 1d)]
    public class JumpSkill : SkillBase, IModifier, IActionProvider
    {
        public JumpSkill(Creature skillUser)
            :
            base(skillUser)
        {
            _Terminator = new TerminateController(this);
        }

        private readonly TerminateController _Terminator;

        #region IDelta Members
        public int Value
            => (BaseValue >= 5) ? 2 : 0;

        public object Source
            => GetType();

        public string Name
            => @">=5 ranks in Jump";

        public bool Enabled
        {
            get { return true; }
            set
            {
                // ignore
            }
        }
        #endregion

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
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

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // TODO: hop-up (start and continue move)
            // TODO: jump-down (start and continue move)

            // NOTE: need to track movement for 20-ft of linearity before these actions
            // TODO: long-jump (target cell)
            // TODO: high-jump (target height/object)
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            var _info = ToInfo<SkillInfo>(null);
            _info.Message = SkillName;
            _info.IsClassSkill = IsClassSkill;
            _info.KeyAbilityMnemonic = KeyAbilityMnemonic;
            _info.UseUntrained = UseUntrained;
            _info.IsTrained = IsTrained;
            _info.CheckFactor = CheckFactor;
            return _info;
        }

        #endregion
    }

}
