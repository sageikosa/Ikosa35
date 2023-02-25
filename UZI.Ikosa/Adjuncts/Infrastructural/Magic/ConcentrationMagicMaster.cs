using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class ConcentrationMagicMaster : GroupMasterAdjunct, IActionProvider, ITrackTime, IDistractable
    {
        public ConcentrationMagicMaster(MagicPowerEffect effect, ConcentrationMagicControl group)
            : base(effect, group)
        {
            _ConBase = new Deltable(effect.PowerLevel);
        }

        #region state
        private double _StartTime;
        private double _ExpireTime;
        private Deltable _ConBase;
        #endregion

        public double StartTime => _StartTime;
        public double ExpirationTime => _ExpireTime;

        public MagicPowerEffect MagicPowerEffect => Source as MagicPowerEffect;
        public ConcentrationMagicControl Control => Group as ConcentrationMagicControl;

        public void DoDisable()
        {
            if (MagicPowerEffect is FragileMagicEffect _fragile
                && _fragile.IsActive)
            {
                _fragile.Activation = new Activation(this, false);
            }
        }

        public void DoConcentrate()
        {
            _ExpireTime += Round.UnitFactor;
        }

        public override object Clone()
            => new ConcentrationMagicMaster(MagicPowerEffect, Control);

        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                _critter.Actions.Providers.Add(this, this);
                _StartTime = ((_critter.Setting as LocalMap)?.CurrentTime ?? 0d);
                _ExpireTime = _StartTime + Round.UnitFactor;
            }

            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as CoreActor)?.Actions.Providers.Remove(this);
            DoDisable();
            base.OnDeactivate(source);
        }

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (budget is LocalActionBudget _budget)
            {
                if (_budget.CanPerformRegular)
                {
                    yield return new ConcentrateOnSpell(this, budget.Actor, @"200");
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            var _tInfo = GetInfoData.GetInfoFeedback(Control.Target.Anchor as ICoreObject, Anchor as CoreActor);
            //var _magicInfo = Control.Target?.ConcentrationMagicEffect.MagicPowerActionSource.ToMagicPowerSource();
            // TODO: _magicInfo on _tInfo in a better structured info...
            //return new AdjunctInfo($@"Concentrate to maintain {_magicInfo.Message} on {_tInfo.Message}", ID);
            return new AdjunctInfo($@"Concentrate to maintain effect on {_tInfo.Message}", ID);
        }

        public double Resolution => Round.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            // need to perform concentration action to push this forward, or else it times out
            if (_ExpireTime <= timeVal)
            {
                DoDisable();
            }
        }

        public Deltable ConcentrationBase => _ConBase;

        public void Interrupted()
        {
            Eject();
        }
    }
}
