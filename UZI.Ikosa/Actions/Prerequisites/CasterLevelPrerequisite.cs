using System;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Caster Checks made by the Caster Class automatically if not the actor.
    /// </summary>
    [Serializable]
    public class CasterLevelPrerequisite : StepPrerequisite
    {
        #region Construction
        public CasterLevelPrerequisite(SpellSource source, Interaction workSet, CoreActor fulfiller,
            string key, string name, Guid targetID, PowerAffectTracker tracker, Deltable difficulty, bool canFail)
            : base(source, workSet, key, name)
        {
            _Difficulty = difficulty;
            _CanFail = canFail;
            _Fulfiller = fulfiller;
            _TargetID = targetID;
            _Tracker = tracker;
        }
        #endregion

        #region state
        private Deltable _Difficulty;
        private CoreActor _Fulfiller;
        private bool _CanFail;
        private Guid _TargetID;
        private PowerAffectTracker _Tracker;
        #endregion

        public Deltable Difficulty => _Difficulty;
        public SpellSource SpellSource => Source as SpellSource;
        public PowerAffectTracker PowerTracker => _Tracker;
        public Guid TargetID => _TargetID;

        /// <summary>If the actor is not the caster, then this needs to be automatically provided?</summary>
        public override CoreActor Fulfiller => _Fulfiller;

        public Deltable CasterCheck { get; set; }

        public override bool IsReady => CasterCheck != null;
        public override bool IsSerial => true;

        /// <summary>IStep member</summary>
        public override bool FailsProcess => !Success && _CanFail;

        public bool Success
        {
            get
            {
                if (IsReady)
                {
                    // TODO: spell penetration...make sure it has hooked onto the caster level for each caster past, present, and future
                    //if (ICore != null)
                    //    return (CasterCheck.QualifiedValue(ICore) >= Difficulty.QualifiedValue(ICore));
                    //else
                    var _check = Deltable.GetCheckNotify(Fulfiller?.ID, @"Caster Check", SpellSource.CasterClass?.OwnerID, @"Spell Difficulty");
                    var _result = CasterCheck.QualifiedValue(Qualification, _check.CheckInfo)
                        >= Difficulty.QualifiedValue(Qualification, _check.OpposedInfo);
                    PowerTracker.SetAffect(TargetID, _result);
                    return _result;
                }
                return false;
            }
        }

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
        {
            // TODO:
            return null;
        }

        public override void MergeFrom(PrerequisiteInfo info)
        {
            // TODO:
        }
    }
}
