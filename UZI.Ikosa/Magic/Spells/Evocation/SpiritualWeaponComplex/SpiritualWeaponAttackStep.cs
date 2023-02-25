using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web.Security;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Magic.Spells
{

    [Serializable]
    public class SpiritualWeaponAttackStep : PreReqListStepBase
    {
        public SpiritualWeaponAttackStep(CoreStep predecessor, SpiritualWeaponGroup spiritualWeaponGroup, int stepIndex)
            : base(predecessor)
        {
            _Group = spiritualWeaponGroup;
            _Index = stepIndex;

            var _controller = _Group.ControlCreature;
            var _workSet = new Qualifier(_controller, this, _Group.Target.Anchor as IInteract);

            _PendingPreRequisites.Enqueue(
                new RollPrerequisite(this, _workSet, _controller, @"SW.Attack", @"Spiritual Weapon Attack Roll", DieRoller.CreateRoller(StandardDieType.d20), false));
            _PendingPreRequisites.Enqueue(
                new RollPrerequisite(this, _workSet, _controller, @"SW.Attack.Critical", @"Spiritual Weapon Critical Confirm Roll", DieRoller.CreateRoller(StandardDieType.d20), false));
        }

        #region state
        private SpiritualWeaponGroup _Group;
        private int _Index;
        #endregion

        public SpiritualWeaponGroup SpiritualWeaponGroup => _Group;
        public int StepIndex => _Index;

        protected override bool OnDoStep()
        {
            // spiritual weapon attack step gets attack information from spiritual weapon group
            // setup attack data
            var _weapon = SpiritualWeaponGroup.Weapon;
            var _controller = SpiritualWeaponGroup.ControlCreature;
            var _score = new Deltable(AllPrerequisites<RollPrerequisite>(@"SW.Attack").FirstOrDefault().RollValue);
            // TODO: stepIndex > 0 => base-attack penalty for multiple attacks per round
            var _critical = new Deltable(AllPrerequisites<RollPrerequisite>(@"SW.Attack.Critical").FirstOrDefault().RollValue);
            var _wLoc = _weapon.Anchor?.GetLocated()?.Locator;
            var _cell = _wLoc.GeometricRegion.AllCellLocations().FirstOrDefault();

            // critical constructor if score roll value base in range
            var _criticalLow = 21 - _weapon.CriticalRange * _weapon.CriticalRangeFactor.EffectiveValue;
            var _atk = _criticalLow <= _score.BaseValue
                ? new SpiritualWeaponAttackData(_controller, _wLoc, AttackImpact.Penetrating, _score, _critical, false, _cell, _cell, StepIndex)
                : new SpiritualWeaponAttackData(_controller, _wLoc, AttackImpact.Penetrating, _score, false, _cell, _cell, StepIndex);

            // interaction
            var _target = _Group.Target.Anchor as IInteract;
            var _atkInteract = new StepInteraction(this, _controller, _weapon, _target, _atk);
            _target.HandleInteraction(_atkInteract);

            // handle feedback
            var _atkFB = _atkInteract.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            // TODO: notifications

            AppendFollowing(new AttackResultStep(Process, null, _atkInteract, _weapon));
            return true;
        }
    }
}
