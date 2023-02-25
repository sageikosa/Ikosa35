using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Affects self and/or self-equivalent (eg, familiar spirit)
    /// </summary>
    [Serializable]
    public class PersonalAim : AimingMode
    {
        #region construction
        public PersonalAim(string key, CoreActor actor)
            : base(key, @"Self", new FixedRange(1), new FixedRange(1))
        {
            _Actor = actor;
        }

        public PersonalAim(string key, string description, CoreActor actor)
            : base(key, description, new FixedRange(1), new FixedRange(1))
        {
            _Actor = actor;
        }
        #endregion

        private CoreActor _Actor;
        public CoreActor Actor { get { return _Actor; } }

        #region public IEnumerable<CoreActor> ValidTargets { get; }
        public IEnumerable<CoreActor> ValidTargets
        {
            get
            {
                return _Actor.ToEnumerable<CoreActor>()
                    .Union(from _ipf in _Actor.Adjuncts
                               .Where(_a => _a.IsActive)
                               .OfType<IProvidePersonalAim>()
                           from _p in _ipf.PersonalAim
                           select _p);
            }
        }
        #endregion

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action,
            AimTargetInfo[] infos, IInteractProvider provider)
        {
            return SelectedTargets<AimTargetInfo>(actor, action, infos)
                .Select(_i => new AimTarget(Key, provider.GetIInteract(_i.TargetID ?? Guid.Empty)));
        }

        protected PAInfo ToPersonalAimInfo<PAInfo>(CoreAction action, CoreActor actor)
            where PAInfo : PersonalAimInfo, new()
        {
            var _info = ToInfo<PAInfo>(action, actor);
            _info.ValidTargets = (from _vt in ValidTargets
                                  let _i = GetInfoData.GetInfoFeedback(_vt, actor)
                                  where _i != null
                                  select new AwarenessInfo
                                  {
                                      ID = _vt.ID,
                                      Info = _i
                                  }).ToList();
            return _info;
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            return ToPersonalAimInfo<PersonalAimInfo>(action, actor);
        }
    }

    /// <summary>Used by adjuncts to indicate they provide additional personal aim targets</summary>
    public interface IProvidePersonalAim
    {
        IEnumerable<CoreActor> PersonalAim { get; }
    }
}
