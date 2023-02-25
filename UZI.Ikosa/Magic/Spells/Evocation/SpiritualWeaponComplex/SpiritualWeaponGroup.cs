using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{

    [Serializable]
    public class SpiritualWeaponGroup : AdjunctGroup
    {
        public SpiritualWeaponGroup(SpellSource source)
            : base(source)
        {
        }

        public SpiritualWeaponEvocation Weapon => Members.OfType<SpiritualWeaponEvocation>().FirstOrDefault();
        public SpiritualWeaponTarget Target => Members.OfType<SpiritualWeaponTarget>().FirstOrDefault();
        public SpiritualWeaponController Controller => Members.OfType<SpiritualWeaponController>().FirstOrDefault();
        public SpellSource SpellSource => Source as SpellSource;
        public Creature ControlCreature => Controller?.Anchor as Creature;

        public override void ValidateGroup()
        {
            GroupValidations.ValidateOneToManyPlanarGroup(this);
            if (Members.Any()
                && !Members.OfType<SpiritualWeaponController>().Any())
            {
                // if the controller vanished, dispel
                EjectMembers();
            }
        }
    }
}
