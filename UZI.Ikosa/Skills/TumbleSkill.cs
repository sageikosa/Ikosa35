using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using System.Linq;
using Uzi.Ikosa.Movement;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Skills
{
    // TODO: +3 fighting defensively if >=5 ranks
    // TODO: +6 total defense if >=5 ranks
    [Serializable, SkillInfo("Tumble", MnemonicCode.Dex, false, 1d)]
    public class TumbleSkill : SkillBase, IModifier, IActionProvider
    {
        public TumbleSkill(Creature skillUser)
            :
            base(skillUser)
        {
            _Terminator = new TerminateController(this);
        }

        /// <summary>True if trained and either not encumbered or speed is not affected by encumberance</summary>
        public bool IsUsable
        {
            get
            {
                var _land = Creature.Movements.AllMovements.OfType<LandMovement>().FirstOrDefault();
                return IsTrained
                    && (Creature.EncumberanceCheck.Unencumbered || (_land?.NoEncumberancePenalty ?? false));
            }
        }

        public int? CurrentTumble
        {
            // TODO: does this even make sense to have this, tumble is increasing and reactive...
            get
            {
                return Creature.Adjuncts.OfType<Tumbling>()
                    .Where(_t => _t.IsActive && !_t.IsCheckExpired)
                    .Max(_t => (int?)_t.SuccessCheckTarget.Result);
            }
        }

        // IDelta Members
        public int Value
            => (BaseValue >= 5) ? 2 : 0;

        public object Source
            => GetType();

        public string Name
            => @">=5 ranks in Tumble";

        public bool Enabled
        {
            get { return true; }
            set { /* ignore */ }
        }

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
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

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsTrained)
            {
                // TODO: entertainment...like perform
                // TODO: set/clear tumble opportunity avoidance (checks made when opportunity would apply)
                //       ... set/clear additional movement cost factors
                // TODO: set/clear tumble space invader opportunity avoidance (checks made when opportunity would apply)
                //       ... set/clear additional movement cost factors
            }
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
