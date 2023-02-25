using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class ConcentrateOnSpell : ActionBase
    {
        #region ctor()
        public ConcentrateOnSpell(ConcentrationMagicMaster concentration, CoreActor actor, string orderKey)
            : this(concentration, actor, new ActionTime(TimeType.Regular), orderKey)
        {
        }

        public ConcentrateOnSpell(ConcentrationMagicMaster concentration, CoreActor actor, ActionTime timeCost, string orderKey)
            : base(concentration.MagicPowerEffect.MagicPowerActionSource, timeCost, false, false, orderKey)
        {
            _Concentration = concentration;
            _Actor = actor;
        }
        #endregion

        #region state
        private ConcentrationMagicMaster _Concentration;
        private CoreActor _Actor;
        #endregion

        public override string Key => @"Spell.Concentrate";

        public override bool IsMental => true;

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
            => Enumerable.Empty<AimingMode>();

        private Info GetAnchorInfo()
            => GetInfoData.GetInfoFeedback(_Concentration.Anchor as ICoreObject, _Actor);

        public override string DisplayName(CoreActor actor)
            => $@"Concentrate on {_Concentration.MagicPowerEffect.MagicPowerActionSource.DisplayName} through {GetAnchorInfo()?.Message}";

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _info = ObservedActivityInfoFactory.CreateInfo(DisplayName(observer), activity.Actor, observer);
            if ((observer == activity?.Actor) && (activity?.Actor != null))
            {
                //_info.Details = new Info { Message = SpellMode.Description };
            }
            return _info;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // use budget
            activity.EnqueueRegisterPreEmptively(Budget);

            // concentrate
            _Concentration.DoConcentrate();

            // unable to start using the spell (but made the effort and used the components)
            return activity.GetActivityResultNotifyStep(@"Extending concentration time");
        }
    }
}
