using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class AdvancementCapacity : GroupParticipantAdjunct
    {
        public AdvancementCapacity(AdvancementCapacityGroup group)
            : base(typeof(AdvancementCapacity), group)
        {
        }

        #region data
        private int _OriginalLevel;
        #endregion

        public AdvancementCapacityGroup AdvancementCapacityGroup => Group as AdvancementCapacityGroup;
        public int OriginalLevel => _OriginalLevel;
        public int CurrentLevel => Creature?.Classes.Sum(_c => _c.CurrentLevel) ?? 0;
        public Creature Creature => Anchor as Creature;

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is Creature)
            && !(newAnchor?.HasActiveAdjunct<AdvancementCapacity>() ?? true);

        protected override void OnActivate(object source)
        {
            _OriginalLevel = Creature.Classes.Sum(_c => _c.CurrentLevel);
            base.OnActivate(source);
        }

        public override object Clone()
            => new AdvancementCapacity(AdvancementCapacityGroup);
    }
}
