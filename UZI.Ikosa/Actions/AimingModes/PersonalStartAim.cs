using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Senses;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Indicates an intersection of the personal locator</summary>
    [Serializable]
    public class PersonalStartAim : PersonalAim
    {
        /// <summary>Indicates an intersection of the personal locator</summary>
        public PersonalStartAim(string key, string displayName, CoreActor actor)
            : base(key, displayName, actor)
        {
        }

        protected List<Locator> GetLocatorList()
        {
            return (from _vt in ValidTargets
                    let _l = _vt.GetLocated()
                    select _l.Locator).ToList();
        }

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, AimTargetInfo[] infos, IInteractProvider provider)
        {
            // start with valid selected target infos
            return from _st in SelectedTargets<AimTargetInfo>(actor, action, infos)
                   join _vt in ValidTargets
                   on _st.TargetID equals _vt.ID
                   // that have sensors to select aim points
                   let _sensor = _vt as ISensorHost
                   where _sensor != null
                   // find their map presence (for map context)
                   let _loc = _vt.GetLocated()
                   where _loc != null
                   // and return the target
                   select new LocationTarget(this.Key, LocationAimMode.Intersection, new Intersection(_sensor.AimPoint), _loc.Locator.MapContext);
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            return ToPersonalAimInfo<PersonalStartAimInfo>(action, actor);
        }
    }
}
