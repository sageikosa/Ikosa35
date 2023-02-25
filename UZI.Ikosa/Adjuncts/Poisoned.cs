using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>indicates a creature has been poisoned</summary>
    [Serializable]
    public class Poisoned : Adjunct, ITrackTime
    {
        public Poisoned(Poison source)
            : base(source)
        {
            PrimaryDone = false;
        }

        public bool PrimaryDone { get; internal set; }
        public double SecondaryTime { get; set; }

        public Poison Poison
            => Source as Poison;

        public override bool CanAnchor(IAdjunctable newAnchor)
        {
            // can only poison a thing with a constitution score
            if ((newAnchor is Creature _critter) && !_critter.Abilities.Constitution.IsNonAbility)
            {
                // if poison has a specific source, no multiple affects
                if ((Poison.SourceID == null)
                    || !_critter.Adjuncts.OfType<Poisoned>().Any(_p => (_p.Poison.SourceID == Poison.SourceID) && (_p != this)))
                {
                    return base.CanAnchor(newAnchor);
                }
            }
            return false;
        }

        public override object Clone()
            => new Poisoned(Poison);

        // ITrackTime Members
        /// <summary>Informs the action manager that the poisoning requires some step-based activity</summary>
        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (!PrimaryDone || ((timeVal >= SecondaryTime) && (direction == TimeValTransition.Leaving)))
            {
                Anchor?.StartNewProcess(new PoisonSaveStep(this), @"Poisoned");
                if (!PrimaryDone)
                {
                    // setup for secondary affect
                    SecondaryTime = (Anchor?.GetCurrentTime() ?? 0d) + Minute.UnitFactor;

                    // and done
                    PrimaryDone = true;
                }
            }
        }

        public double Resolution
            => Minute.UnitFactor;
    }

    [Serializable]
    public class PoisonSaveStep : PreReqListStepBase
    {
        public PoisonSaveStep(Poisoned poisoned)
            : base((CoreProcess)null)
        {
            _Poisoned = poisoned;
            _IsPrimary = !_Poisoned.PrimaryDone;
            var _qualifier = new Qualifier(null, Poisoned.Poison, Poisoned.Anchor as IInteract);
            var _difficulty = Poisoned.Poison.Difficulty.GetDeltaCalcInfo(_qualifier, $@"{Poisoned.Poison.Name} Difficulty");
            _PendingPreRequisites.Enqueue(new SavePrerequisite(Poisoned.Poison, _qualifier,
                @"Poison", @"Save versus Poison", new SaveMode(SaveType.Fortitude, SaveEffect.Negates, _difficulty)));
        }

        #region data
        private Poisoned _Poisoned;
        private bool _IsPrimary;
        #endregion

        public SavePrerequisite Save
            => GetPrerequisite<SavePrerequisite>();

        public Poisoned Poisoned => _Poisoned;
        public bool IsPrimary => _IsPrimary;

        protected override bool OnDoStep()
        {
            if (Save.Success)
            {
                // clear prereqs
                EnqueueNotify(new CheckResultNotify(Poisoned.Anchor.ID, @"Save", true, new Info { Message = @"versus Poison" }), Poisoned.Anchor.ID);
                if ((Poisoned.Poison.SourceID != null) && (Poisoned.Poison.SaveImmuneSpan != null))
                {
                    // if saving grants temporary immunity, grant it
                    var _endTime = (Poisoned.Anchor?.GetCurrentTime() ?? 0d) + (Poisoned.Poison.SaveImmuneSpan ?? 0d);
                    Poisoned.Anchor.AddAdjunct(
                        new TemporaryPoisonImmunity(Poisoned.Poison, Poisoned.Poison.SourceID, _endTime));
                }

                // successful secondary simply ejects the poison
                if (!IsPrimary)
                    Poisoned.Eject();
            }
            else
            {
                EnqueueNotify(new CheckResultNotify(Poisoned.Anchor.ID, @"Save", false, new Info { Message = @"versus Poison" }), Poisoned.Anchor.ID);
                new PoisonedStep(this);
            }
            return true;
        }
    }

    [Serializable]
    public class PoisonedStep : PreReqListStepBase
    {
        #region ctor(...)
        public PoisonedStep(PoisonSaveStep saveStep) :
            base(saveStep)
        {
            _SaveStep = saveStep;

            IEnumerable<RollPrerequisite> _rollPrereq = null;
            if (PoisonSaveStep.IsPrimary)
            {
                _rollPrereq = (from _roll in Poisoned.Poison.GetPrimaryRollers()
                               select new RollPrerequisite(Poisoned.Poison, new Interaction(null, Poisoned.Anchor, null, null),
                                   null, _roll.Key, _roll.Name, _roll.Roller, false)).ToList();
            }
            else
            {
                _rollPrereq = (from _roll in Poisoned.Poison.GetSecondaryRollers()
                               select new RollPrerequisite(Poisoned.Poison, new Interaction(null, Poisoned.Anchor, null, null),
                                   null, _roll.Key, _roll.Name, _roll.Roller, false)).ToList();
            }
            if (_rollPrereq != null)
                foreach (var _rPrereq in _rollPrereq)
                {
                    _PendingPreRequisites.Enqueue(_rPrereq);
                }
        }
        #endregion

        #region data
        private PoisonSaveStep _SaveStep;
        #endregion

        public PoisonSaveStep PoisonSaveStep => _SaveStep;
        public Poisoned Poisoned => _SaveStep.Poisoned;

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            if (IsComplete)
                return true;

            var _id = Poisoned.Anchor.ID;
            if (PoisonSaveStep.IsPrimary)
            {
                var _primaries = (from _roll in Poisoned.Poison.GetPrimaryRollers()
                                  let _pre = AllPrerequisites<RollPrerequisite>(_roll.Key).FirstOrDefault()
                                  where _pre != null
                                  select _pre.RollValue).ToArray();

                // perform primary adjunct
                var _dmg = Poisoned.Poison.ApplyPrimary(this, Poisoned.Anchor as Creature, _primaries).ToList();

                // and done
                if (_dmg.Any())
                {
                    EnqueueNotify(new DealtDamageNotify(Poisoned.Anchor.ID, @"Damage Taken", _dmg, new Info { Message = @"Poison" }), Poisoned.Anchor.ID);
                    EnqueueNotify(new RefreshNotify(true, true, true, false, false), Poisoned.Anchor.ID);
                }
            }
            else
            {
                var _secondaries = (from _roll in Poisoned.Poison.GetSecondaryRollers()
                                    let _pre = AllPrerequisites<RollPrerequisite>(_roll.Key).FirstOrDefault()
                                    where _pre != null
                                    select _pre.RollValue).ToArray();

                // perform secondary adjunct
                var _dmg = Poisoned.Poison.ApplySecondary(this, Poisoned.Anchor as Creature, _secondaries).ToList();

                // and done
                if (_dmg.Any())
                {

                    EnqueueNotify(new DealtDamageNotify(Poisoned.Anchor.ID, @"Damage Taken", _dmg, new Info { Message = @"Poison" }), Poisoned.Anchor.ID);
                    EnqueueNotify(new RefreshNotify(true, true, true, false, false), Poisoned.Anchor.ID);
                }

                // secondary complete ejects the poison
                Poisoned.Eject();
            }

            // clear prereqs
            //EnqueueNotify(new SysNotify(@"Poison", new Info { Message = @"Poison Done" }), _id);
            return true;
        }
        #endregion
    }
}
