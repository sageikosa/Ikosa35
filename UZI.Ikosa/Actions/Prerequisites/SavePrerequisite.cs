using System;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Target always have to make the save</summary>
    [Serializable]
    public class SavePrerequisite : StepPrerequisite, ISourcedObject
    {
        #region Construction
        /// <summary>Target always have to make the save</summary>
        public SavePrerequisite(object source, Qualifier workSet, string key, string name, SaveMode saveMode, bool isSecret = false) :
            base(source, workSet, key, name)
        {
            _IsSecret = isSecret;
            _Mode = saveMode;

            // always fail
            if ((workSet?.Target is IProvideSaves _prov)
                && _prov.AlwaysFailsSave)
            {
                _Roll = new ConstDeltable(1);
            }
        }
        #endregion

        #region data
        private bool _IsSecret;
        private SaveMode _Mode;
        private Deltable _Roll = null;
        #endregion

        public bool IsSecret => _IsSecret;
        public SaveMode SaveMode => _Mode;

        public override CoreActor Fulfiller
            => (Qualification?.Target is Creature _critter) && !IsSecret
            ? _critter
            : null;

        public Deltable SaveRoll
        {
            get => _Roll;
            set
            {
                // terminate old qualify delta subscriptions
                foreach (var _q in SaveMode.QualifiedDeltas)
                {
                    _q.DoTerminate();
                }

                // track roll and add qualify deltas
                _Roll = value;
                foreach (var _q in SaveMode.QualifiedDeltas)
                {
                    _Roll.Deltas.Add(_q);
                }
            }
        }

        public override bool IsReady => SaveRoll != null;
        public override bool IsSerial => true;
        public override bool UniqueKey => true;

        /// <summary>IStep member</summary>
        public override bool FailsProcess => false;

        public bool Success
        {
            get
            {
                if (IsReady)
                {
                    var _save = new SavingThrowData(Qualification.Actor as Creature, SaveMode, _Roll);

                    // let target's handlers alter the roll if possible
                    Qualification.Target.HandleInteraction(
                        new Interaction(Qualification.Actor, Qualification.Source, Qualification.Target, _save));

                    return _save.Success(Qualification);
                }
                return false;
            }
        }

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
        {
            var _info = ToInfo<SavePrerequisiteInfo>(step);
            _info.SaveType = SaveMode.SaveType.ToString();
            if (IsReady)
                _info.Value = SaveRoll.EffectiveValue;
            return _info;
        }

        public override void MergeFrom(PrerequisiteInfo info)
        {
            if (info is SavePrerequisiteInfo _savePre)
            {
                if (_savePre.Value.HasValue)
                    SaveRoll = new Deltable(_savePre.Value.Value);
            }
        }
    }
}
