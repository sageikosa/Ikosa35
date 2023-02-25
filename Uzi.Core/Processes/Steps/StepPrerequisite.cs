using System;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public abstract class StepPrerequisite : ISourcedObject, IStepPrerequisite
    {
        #region Construction
        /// <summary>This version of the constructor creates a prerequisite that must be dealt with by the system master.</summary>
        public StepPrerequisite(object source, string key, string name)
        {
            _Key = key;
            _Name = name;
            _Source = source;
            _Qual = null;
        }

        /// <summary>Prerequisite with a predefined workset</summary>
        public StepPrerequisite(object source, Qualifier workSet, string key, string name)
        {
            _Key = key;
            _Name = name;
            _Source = source;
            _Qual = workSet;
        }

        public StepPrerequisite(object source, CoreActor actor, object iSource, IInteract target, string key, string name)
        {
            _Key = key;
            _Name = name;
            _Source = source;
            _Qual = new Qualifier(actor, iSource, target);
        }
        #endregion

        #region Data
        private readonly string _Key;
        private readonly string _Name;
        private readonly object _Source;
        private readonly Qualifier _Qual;
        #endregion 

        /// <summary>Used to associate prerequisites with each other (such as saves and damages)</summary>
        public string BindKey => _Key;
        public string Name => _Name;
        public object Source => _Source;
        public Qualifier Qualification => _Qual;

        /// <summary>Actor who needs to meet the prerequisite</summary>
        public abstract CoreActor Fulfiller { get; }

        /// <summary>Indicates that duplicate keys should be considered duplicate prerequisites and removed from the step.</summary>
        public virtual bool UniqueKey
            => false;

        /// <summary>Indicates the prerequisite is ready</summary>
        public abstract bool IsReady { get; }

        /// <summary>
        /// Indicates the prerequisite fails the process.  
        /// Typically this is for prerequisites internal to the process, rather than in reaction
        /// </summary>
        public abstract bool FailsProcess { get; }

        /// <summary>Indicates this prerequisite must be made ready before the next can be retrieved.  Default is false.</summary>
        public virtual bool IsSerial
            => false;

        protected PInfo ToInfo<PInfo>(CoreStep step)
            where PInfo : PrerequisiteInfo, new()
        {
            var _info = new PInfo()
            {
                FulfillerID = Fulfiller?.ID ?? Guid.Empty,
                BindKey = BindKey,
                Name = Name,
                IsReady = IsReady,
                IsSerial = IsSerial,
                StepID = step.ID
            };
            return _info;
        }

        public abstract PrerequisiteInfo ToPrerequisiteInfo(CoreStep step);

        public abstract void MergeFrom(PrerequisiteInfo info);
    }
}
