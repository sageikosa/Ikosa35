using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class LiquidFallingStopStep : PreReqListStepBase
    {
        #region construction
        public LiquidFallingStopStep(CoreStep predecessor, Locator locator, LiquidFallMovement movement)
            : base(predecessor)
        {
            _Locator = locator;
            _LiquidFall = movement;
            if (movement.FallTrack == 1)
            {
                // non lethal damage roll (by game master)
                LiquidFallMovement.FallMovement.MaxNonLethal += 1;
                var _nonLethal = LiquidFallMovement.FallMovement.NonLethalRoller;
                if (_nonLethal != null)
                {
                    _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Fall.Damage.Soft", @"Non-Lethal Falling Damage",
                        _nonLethal, false));
                }

                // damage roll (by game master)
                var _lethal = LiquidFallMovement.FallMovement.DamageRoller;
                if (_lethal != null)
                {
                    _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Fall.Damage.Hard", @"Falling Damage",
                        _lethal, false));
                }
            }
        }
        #endregion

        #region state
        private Locator _Locator;
        private LiquidFallMovement _LiquidFall;
        #endregion

        public override string Name => @"Stopped falling through liquid";
        public Locator Locator => _Locator;
        public LiquidFallMovement LiquidFallMovement { get => _LiquidFall; set => _LiquidFall = value; }

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            // get prerequisites
            var _dmgPre = AllPrerequisites<RollPrerequisite>(@"Fall.Damage.Hard").FirstOrDefault();
            var _nonPre = AllPrerequisites<RollPrerequisite>(@"Fall.Damage.Soft").FirstOrDefault();

            // get status messages
            var _msg = new List<string>();
            if (_dmgPre != null)
            {
                _msg.Add(string.Format(@"Damage: {0}", _dmgPre.RollValue));
            }
            if (_nonPre != null)
            {
                _msg.Add(string.Format(@"Non-Lethal: {0}", _nonPre.RollValue));
            }

            // must have accumulated fall dice, and not reducing, no need to 
            foreach (var _interact in Locator.ICoreAs<IInteract>())
            {
                // nonlethal damage
                if (_nonPre != null)
                {
                    var _non = new DamageData(_nonPre.RollValue, true, @"Fall", 0);
                    var _deliver = new DeliverDamageData(null, _non.ToEnumerable(), false, false);
                    _interact.HandleInteraction(new Interaction(null, this, _interact, _deliver));
                }

                // damage
                if (_dmgPre != null)
                {
                    var _dmg = new DamageData(_dmgPre.RollValue, false, @"Fall", 0);
                    var _deliver = new DeliverDamageData(null, _dmg.ToEnumerable(), false, false);
                    _interact.HandleInteraction(new Interaction(null, this, _interact, _deliver));
                }

                var _fallStop = new FallStop(Locator, _msg);
                _interact.HandleInteraction(new StepInteraction(this, null, this, _interact, _fallStop));

                // generate sound...
                LiquidFallMovement.GenerateImpactSound(false);
            }

            // remove falling adjunct
            LiquidFallMovement.RemoveLiquidFalling();

            // done
            return true;
        }
        #endregion
    }
}
