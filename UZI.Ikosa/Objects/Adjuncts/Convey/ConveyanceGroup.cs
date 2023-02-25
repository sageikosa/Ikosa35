using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class ConveyanceGroup : AdjunctGroup
    {
        public ConveyanceGroup()
            : base(typeof(ConveyanceGroup))
        {
        }

        /// <summary>Representees a conveyance</summary>
        public Conveyor Conveyor => Members.OfType<Conveyor>().FirstOrDefault();

        /// <summary>Representees passengers of a conveyance</summary>
        public IEnumerable<Conveyee> Passengers => Members.OfType<Conveyee>();

        protected override void OnMemberRemoved(GroupMemberAdjunct member)
        {
            // tear down conveyor if last passenger leaves
            if ((member != Conveyor) && !Passengers.Any() && (Conveyor != null))
                Conveyor.Eject();
            base.OnMemberRemoved(member);
        }

        public static Conveyor GetConveyor(IAdjunctable master)
        {
            var _master = master.Adjuncts.OfType<Conveyor>().FirstOrDefault();
            if (_master != null)
            {
                // if already a master of a surface group, return it
                return _master;
            }
            else
            {
                // otherwise, add a new one
                var _group = new ConveyanceGroup();
                _master = new Conveyor(_group);
                master.AddAdjunct(_master);
                return _master;
            }
        }

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();
    }
}
