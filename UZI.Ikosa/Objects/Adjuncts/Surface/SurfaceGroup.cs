using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class SurfaceGroup : AdjunctGroup
    {
        // TODO: define some basic containment properties and operations
        public SurfaceGroup()
            : base(typeof(SurfaceGroup))
        {
        }

        public SurfaceContainer Container { get { return Members.OfType<SurfaceContainer>().FirstOrDefault(); } }
        public IEnumerable<OnSurface> Contained { get { return Members.OfType<OnSurface>(); } }

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();

        protected override void OnMemberRemoved(GroupMemberAdjunct member)
        {
            // not removing the container, no contained, but still have a container
            if ((member != Container) && !Contained.Any() && (Container != null))
            {
                Container.Eject();
            }

            base.OnMemberRemoved(member);
        }

        public static SurfaceContainer GetSurfaceContainer(IAdjunctable master)
        {
            var _master = master.Adjuncts.OfType<SurfaceContainer>().FirstOrDefault();
            if (_master != null)
            {
                // if already a master of a surface group, return it
                return _master;
            }
            else
            {
                // otherwise, add a new one
                var _group = new SurfaceGroup();
                _master = new SurfaceContainer(_group);
                master.AddAdjunct(_master);
                return _master;
            }
        }
    }
}
