using System;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class ActorStateChangeStep : CoreStep
    {
        public ActorStateChangeStep(CoreProcess process, Adjunct remove, Adjunct add, Creature critter)
            : base(process)
        {
            _Remove = remove;
            _Add = add;
            _Critter = critter;
        }

        #region private data
        private Adjunct _Remove;
        private Adjunct _Add;
        private Creature _Critter;
        #endregion

        public override string Name
        {
            get
            {
                if (_Critter != null)
                    return string.Format(@"{0} changed from {1} to {2}", _Critter.Name, _Remove.SourceName(), _Add.SourceName());
                return base.Name;
            }
        }

        protected override bool OnDoStep()
        {
            // remove
            if (_Remove != null)
                _Remove.Eject();

            if (_Critter != null)
            {
                // add
                if (_Add != null)
                    _Critter.AddAdjunct(_Add);

                // special state change!
                if ((_Remove is NotHealing) && (_Add is Disabled))
                {
                    // TODO: natural healing for creature going from NotHealing to Disabled (use a step!)
                }
            }

            // done
            EnqueueNotify(new RefreshNotify(true, true, true, false, false), _Critter.ID);
            // TODO: new Info { Title = Name },
            return true;
        }

        protected override StepPrerequisite OnNextPrerequisite() { return null; }
        public override bool IsDispensingPrerequisites { get { return false; } }
    }
}
