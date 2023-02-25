using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class SummoningWindowLink : GroupMemberAdjunct
    {
        public SummoningWindowLink(object source, SummoningGroup group) 
            : base(source, group)
        {

        }

        public override object Clone()
            => new SummoningWindowLink(Source, SummoningGroup);

        public SummoningWindow SummoningWindow => Anchor as SummoningWindow;
        public SummoningGroup SummoningGroup => Group as SummoningGroup;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            var _map = SummoningWindow.Setting as LocalMap;
            var _location = SummoningWindow?.GetLocated()?.Locator.Location;
            new ObjectPresenter(SummoningWindow.SummonedTarget, _map.MapContext, SummoningWindow.GeometricSize,
                new Cubic(_location, SummoningWindow.GeometricSize), _map.Resources);

            // can only control while active
            SummoningWindow.SummonedTarget.AddAdjunct(new Summoned(this, SummoningGroup));
        }

        protected override void OnDeactivate(object source)
        {
            // can only control while active
            SummoningWindow.SummonedTarget.Adjuncts
                .OfType<Summoned>()
                .FirstOrDefault(_s => _s.Source == this && _s.ControlGroup == SummoningGroup)?
                .Eject();
            SummoningWindow.SummonedTarget?.GetLocated()?.Eject();
            base.OnDeactivate(source);
        }
    }
}
