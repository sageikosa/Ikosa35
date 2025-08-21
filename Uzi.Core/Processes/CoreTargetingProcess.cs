using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class CoreTargetingProcess : CoreProcess
    {
        public CoreTargetingProcess(CoreObject main, string name, List<AimTarget> targets)
            : this(null, main, name, targets)
        {
        }

        public CoreTargetingProcess(CoreStep rootStep, CoreObject main, string name, List<AimTarget> targets)
            : base(rootStep, name)
        {
            _Main = main;
            if (targets != null)
            {
                _Targets = new List<AimTarget>(targets);
                _OriginalTargets = targets;
            }
            else
            {
                _Targets = [];
                _OriginalTargets = [];
            }
        }

        #region data
        private CoreObject _Main;
        private List<AimTarget> _Targets;
        private List<AimTarget> _OriginalTargets;
        #endregion

        public CoreObject MainObject => _Main;

        public List<AimTarget> Targets => _Targets;

        public IEnumerable<AimTarget> OriginalTargets
            => _OriginalTargets.Select(_t => _t);

        /// <summary>Get first target of specified type that matches the key</summary>
        public TType GetFirstTarget<TType>(string key) where TType : AimTarget
            => _Targets.OfType<TType>().FirstOrDefault(_t => _t.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

        /// <summary>Sets target by key and type (replacing if necessary)</summary>
        public void SetFirstTarget<TType>(TType target) where TType : AimTarget
        {
            var _target = GetFirstTarget<TType>(target.Key);
            if (_target != null)
            {
                _Targets.Remove(_target);
            }
            _Targets.Add(target);
        }

        public NotifyStep GetNotifyStep(SysNotify notify, CoreActor actor)
            => new NotifyStep(this, notify)
            {
                InfoReceivers = new Guid[] { actor?.ID ?? Guid.Empty }
            };
    }
}
