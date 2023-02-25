using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Feats
{
    [Serializable]
    public class CleaveAttackRoll : PreReqListStepBase
    {
        public CleaveAttackRoll(CoreStep predecessor, CleaveReactor reactor, CoreActor target)
            : base(predecessor)
        {
            _Reactor = reactor;
            _Target = target;

            var _critter = _Reactor.CleaveFeat.Creature;
            _PendingPreRequisites.Enqueue(new RollPrerequisite(this, null, _critter, @"Cleave.AttackRoll", @"Cleaving Attack",
                new DieRoller(20), false));
            _PendingPreRequisites.Enqueue(new RollPrerequisite(this, null, _critter, @"Cleave.CriticalRoll", @"Cleaving Critical Confirm",
                new DieRoller(20), false));
        }

        #region data
        private CleaveReactor _Reactor;
        private CoreActor _Target;
        #endregion

        public RollPrerequisite AttackRoll
            => AllPrerequisites<RollPrerequisite>(@"Cleave.AttackRoll").FirstOrDefault();

        public RollPrerequisite CriticalRoll
            => AllPrerequisites<RollPrerequisite>(@"Cleave.CriticalRoll").FirstOrDefault();

        protected override bool OnDoStep()
        {
            // creature
            var _critter = _Reactor.CleaveFeat.Creature;
            var _aLoc = Locator.FindFirstLocator(_critter);
            if (_aLoc != null)
            {
                // source cell
                var _sensors = _critter as ISensorHost;
                var _srcCell = _sensors.GetAimCell(_aLoc?.GeometricRegion);
                if (_srcCell != null)
                {
                    // target
                    // get target cell
                    var _trgCell = _sensors.GetTargetCell(_aLoc, _Target);

                    // get impact from "original" impact
                    var _impact = (Process as CoreActivity)?.Targets.OfType<AttackTarget>().FirstOrDefault()?
                        .Attack.Impact ?? Contracts.AttackImpact.Penetrating;

                    // strike based on reactor (weapon that just struck)
                    var _strike = new MeleeStrike(_Reactor.WeaponHead, _impact, @"101");
                    _strike.CanPerformNow(_critter.GetLocalActionBudget());
                    var _score = new Deltable(AttackRoll.RollValue);
                    var _critical = new Deltable(CriticalRoll.RollValue);

                    // build attack target (with melee attack data for the melee strike)
                    var _atkTarget = new AttackTarget(@"Cleave.Target", _Target,
                        new MeleeAttackData(_critter, _strike, _aLoc, _impact, _score, _critical, false, _srcCell, _trgCell, 1, 1))
                        .ToEnumerable()
                        .Select(_t => _t as AimTarget)
                        .ToList();
                    var _activity = new CoreActivity(_critter, _strike, _atkTarget);

                    // register use and attack
                    _Reactor.GetCleaveBudget().RegisterUse();
                    Process.ProcessManager.StartProcess(_activity);
                }
            }
            return true;
        }
    }
}
