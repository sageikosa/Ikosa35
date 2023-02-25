using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Senses;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class PersonalConicAim : PersonalStartAim
    {
        public PersonalConicAim(string key, string description, Range distance, CoreActor actor)
            : base(key, description, actor)
        {
            _Dist = distance;
        }

        private Range _Dist;
        public Range Distance { get { return _Dist; } }

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
                   let _locator = _loc.Locator
                   let _cone = new ConeBuilder(
                       (int)(Distance.EffectiveRange(actor, action.CoreActionClassLevel(actor, this)) / 5d),
                       AnchorFaceHelper.MovementFaces(_locator.GetGravityFace(), _sensor.Heading, _sensor.Incline).ToArray()
                       )
                   // and return the target
                   select new GeometricTarget(this.Key,
                       new ConicBuilderInfo(_cone),
                       _sensor.GetAimCell(_locator.GeometricRegion),
                       new Intersection(_sensor.AimPoint),
                       _locator.MapContext);
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToPersonalAimInfo<PersonalConicAimInfo>(action, actor);
            _info.Distance = this.Distance.EffectiveRange(actor, _info.ClassLevel);
            return _info;
        }
    }
}
