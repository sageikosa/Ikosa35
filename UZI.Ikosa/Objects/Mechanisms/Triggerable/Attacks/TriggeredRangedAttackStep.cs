using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class TriggeredRangedAttackStep : PreReqListStepBase
    {
        #region ctor()
        public TriggeredRangedAttackStep(RangedTriggerable triggerable, int attackIndex, int attackCount)
            : base((CoreProcess)null)
        {
            _Triggerable = triggerable;
            _AtkIdx = attackIndex;
            _AtkCount = attackCount;
            _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Trap.Attack", $@"{triggerable.Name} Trap Attack", new DieRoller(20), false));
            _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Trap.Critical", $@"{triggerable.Name} Trap Critical", new DieRoller(20), false));
        }
        #endregion

        #region data
        private RangedTriggerable _Triggerable;
        private int _AtkIdx;
        private int _AtkCount;
        #endregion

        public override string Name
            => _AtkCount > 1
            ? $@"Ranged Trap Attack ({_AtkIdx}/{_AtkCount})"
            : @"Ranged Trap Attack";

        public CoreTargetingProcess TargetingProcess
            => Process as CoreTargetingProcess;

        public RangedTriggerable RangedTriggerable => _Triggerable;

        public RollPrerequisite AttackRoll
            => AllPrerequisites<RollPrerequisite>(@"Trap.Attack").FirstOrDefault();

        public RollPrerequisite CriticalRoll
            => AllPrerequisites<RollPrerequisite>(@"Trap.Critical").FirstOrDefault();

        protected override bool OnDoStep()
        {
            if (TargetingProcess is CoreTargetingProcess _proc)
            {
                // must have a loaded item for this attack
                if (RangedTriggerable.Loaded > 0)
                {
                    // decrease loaded items...
                    RangedTriggerable.Loaded--;

                    // rolls
                    var _attackRoll = AttackRoll.RollValue;
                    var _score = new Deltable(_attackRoll);
                    var _critScore = new Deltable(CriticalRoll.RollValue);
                    _score.Deltas.Add(new SoftQualifiedDelta(RangedTriggerable.AttackBonus));
                    _critScore.Deltas.Add(new SoftQualifiedDelta(RangedTriggerable.AttackBonus));


                    // location of attack and target (exclude thing to which mechanism is bound)
                    var _locator = RangedTriggerable.GetLocated()?.Locator;
                    var _bound = _locator.ICore;
                    var _srcCell = RangedTriggerable.Location;
                    var _trgCell = _srcCell;
                    var _target = (from _l in _locator.Map.MapContext.LocatorsInCell(_srcCell, _locator.PlanarPresence)
                                   from _o in _l.GetCapturable<IInteract>()
                                   where (_o != _bound)
                                   select new
                                   {
                                       Target = _o,
                                       Precedence = (_o is Creature) ? 0 : 1
                                   })
                                   .OrderBy(_x => _x.Precedence)
                                   .FirstOrDefault()?.Target;

                    // direct uses the locator that triggered the attack
                    if (RangedTriggerable.IsDirect)
                    {
                        // from the direct target list
                        var _direct = TargetingProcess
                            .GetFirstTarget<ValueTarget<List<Locator>>>(@"Direct")?
                            .Value.FirstOrDefault();
                        if ((_direct != null)
                            && (_direct.ICoreAs<IInteract>().FirstOrDefault() is IInteract _alt))
                        {
                            // the main ICore and the nearest cell to the source-cell
                            _target = _alt;
                            _trgCell = _direct.GeometricRegion
                                .AllCellLocations()
                                .OrderBy(_cl => IGeometricHelper.Distance(_cl, _srcCell))
                                .FirstOrDefault();
                        }
                    }

                    // setup attack target
                    TargetingProcess.Targets
                        .Add(_attackRoll >= RangedTriggerable.CriticalLow
                            ? new AttackTarget(@"Ranged.Target", _target,
                                new RangedAttackData(null, RangedTriggerable, null, _locator, Contracts.AttackImpact.Penetrating,
                                    _score, _critScore, false, _srcCell.GetPoint(), _srcCell, _trgCell, 1, 1))
                            : new AttackTarget(@"Ranged.Target", _target,
                                new RangedAttackData(null, RangedTriggerable, null, _locator, Contracts.AttackImpact.Penetrating,
                                    _score, false, _srcCell.GetPoint(), _srcCell, _trgCell, 1, 1)));

                    // do attack next...
                    var _attack = new AttackStep(_proc, RangedTriggerable);
                    AppendFollowing(_attack);
                }
            }
            return true;
        }
    }
}
