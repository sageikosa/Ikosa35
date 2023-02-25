using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Added to actor that applied the durable magic effect.  
    /// Allows the actor to dismiss the durable magic effect.
    /// </summary>
    [Serializable]
    public class DismissibleMagicEffectMaster : GroupMasterAdjunct, IActionProvider
    {
        /// <summary>
        /// Added to actor that applied the durable magic effect.  
        /// Allows the actor to dismiss the durable magic effect.
        /// </summary>
        public DismissibleMagicEffectMaster(DismissibleMagicEffectControl group)
            : base(group, group)
        {
        }

        public DismissibleMagicEffectControl Control => Group as DismissibleMagicEffectControl;

        public override object Clone()
            => new DismissibleMagicEffectMaster(Control);

        protected override void OnActivate(object source)
        {
            (Anchor as CoreActor)?.Actions.Providers.Add(this, this);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);
            (Anchor as CoreActor)?.Actions.Providers.Remove(this);
        }

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (budget is LocalActionBudget _budget)
            {
                if (_budget.CanPerformRegular)
                {
                    yield return new DismissSpell(Control.Target?.DurableMagicEffect, budget.Actor, @"200");
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            var _tInfo = GetInfoData.GetInfoFeedback(Control.Target.Anchor as ICoreObject, Anchor as CoreActor);
            var _magicInfo = Control.Target?.DurableMagicEffect.MagicPowerActionSource.ToMagicPowerSource();
            // TODO: _magicInfo on _tInfo in a better structured info...
            return new AdjunctInfo($@"Controlling {_magicInfo.Message} on {_tInfo.Message}", ID);
        }
    }
}
