using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class FallingStopStep : PreReqListStepBase
    {
        #region construction
        public FallingStopStep(CoreStep predecessor, Locator locator, BaseFallMovement movement)
            : base(predecessor)
        {
            _Locator = locator;
            _BaseFall = movement;

            if (BaseFallMovement is FallMovement _fullFall)
            {
                if (_fullFall.IsUncontrolled)
                {
                    // non lethal damage roll (by game master)
                    var _nonLethal = _fullFall.NonLethalRoller;
                    if (_nonLethal != null)
                    {
                        _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Fall.Damage.Soft", @"Non-Lethal Falling Damage",
                            _nonLethal, false));
                    }

                    // damage roll (by game master)
                    var _lethal = _fullFall.DamageRoller;
                    if (_lethal != null)
                    {
                        _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Fall.Damage.Hard", @"Falling Damage",
                            _lethal, false));
                    }
                }
            }
        }
        #endregion

        #region state
        private Locator _Locator;
        private BaseFallMovement _BaseFall;
        #endregion

        public override string Name => @"Stopped falling";
        public Locator Locator => _Locator;
        public BaseFallMovement BaseFallMovement { get => _BaseFall; set => _BaseFall = value; }

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            // get prerequisites
            var _dmgPre = AllPrerequisites<RollPrerequisite>(@"Fall.Damage.Hard").FirstOrDefault();
            var _nonPre = AllPrerequisites<RollPrerequisite>(@"Fall.Damage.Soft").FirstOrDefault();

            // get status messages 
            // TODO: ¿¿¿ eliminate this ???
            var _msg = new List<string>();
            if (_dmgPre != null)
            {
                _msg.Add(string.Format(@"Damage: {0}", _dmgPre.RollValue));
            }
            if (_nonPre != null)
            {
                _msg.Add(string.Format(@"Non-Lethal: {0}", _nonPre.RollValue));
            }

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
                var _stepInteract = new StepInteraction(this, null, this, _interact, _fallStop);
                _interact.HandleInteraction(_stepInteract);

                // TODO: ¿¿¿ sounds for slow fall stop also ???
                if (BaseFallMovement is FallMovement _fullFall)
                {
                    var _trove = _stepInteract.Feedback.OfType<TroveMergeFeedback>().FirstOrDefault()?.MergedTrove;
                    if (_trove != null)
                    {
                        _fullFall.GenerateImpactSound(_trove);
                    }
                    else
                    {
                        // NOTE: at this point, fall movement may not be connected with an object...
                        if (_fullFall.IsUncontrolled)
                        {
                            _fullFall.GenerateImpactSound(BaseFallMovement.CoreObject);
                        }
                    }
                }
            }

            // remove falling adjunct
            BaseFallMovement.RemoveFalling();

            // done
            return true;
        }
        #endregion
    }
}
