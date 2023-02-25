using System;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Items
{
    [
    Serializable,
    ItemInfo(@"Chest", @"Holds items", @"chest")
    ]
    public class Chest : ContainerItemBase, IAudibleOpenable
    {
        public Chest()
            : this(@"Chest", true)
        {
        }
        public Chest(string name, bool opaque)
            : base(name, new ContainerObject(@"storage", WoodMaterial.Static, true, false), true)
        {
            ItemSizer.NaturalSize = Size.Small;
            BaseWeight = 25;
            Price.CorePrice = 2;
            MaxStructurePoints.BaseValue = 15;
            Container.MaximumLoadWeight = 250;
            Container.MaxStructurePoints = 1;
            TareWeight = 25;
            ItemMaterial = WoodMaterial.Static;
        }

        protected override string ClassIconKey => @"chest";

        #region IAudibleOpenable Members
        private string GetMaterialString()
            => $@"{ItemMaterial.SoundQuality}";

        public SoundRef GetOpeningSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"opening",
                (0, @"whining"),
                (5, $@"{GetMaterialString()} whining"),
                (10, $@"{GetMaterialString()} chest opening")),
                8, 90, serialState);

        public SoundRef GetOpenedSound(Func<Guid> idFactory, object source, ulong serialState)
            => null;

        public SoundRef GetClosingSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"closing",
                (0, @"whining"),
                (5, $@"{GetMaterialString()} whining"),
                (10, $@"{GetMaterialString()} chest closing")),
                8, 90, serialState);

        public SoundRef GetClosedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"closed",
                (0, @"thump"),
                (5, $@"{GetMaterialString()} thump"),
                (10, $@"{GetMaterialString()} chest closed")),
                4, 105, serialState);

        public SoundRef GetBlockedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"blocked",
                (0, @"rattling"),
                (10, $@"{GetMaterialString()} rattling")),
                8, 90, serialState);
        #endregion
    }
}
