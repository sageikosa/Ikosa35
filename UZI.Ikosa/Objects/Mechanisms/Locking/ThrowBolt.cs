using System;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Objects
{
    /// <summary>
    /// Throw bolt, that can fasten an openable shut together and is anchored in place.  Also, a base for Hasp.  
    /// Supports spring loaded throw bolts by optionally allowing its blocker to allow closure when it itself is closed.
    /// </summary>
    [Serializable]
    public class ThrowBolt : FastenerObject, IAudibleOpenable
    {
        public ThrowBolt(string name, Material material, int disableDifficulty, IOpenable target, IAnchorage anchorage, bool allowClose)
            : base(name, material, disableDifficulty, allowClose)
        {
            // set target
            _Target = target;

            // bind the throw bolt to the anchorage
            this.BindToObject(anchorage);
        }

        // TODO: enforce throwbolt is object bound to fasten target...?
        private IOpenable _Target;

        protected override IOpenable FastenTarget
            => _Target;

        protected override string ClassIconKey
            => nameof(ThrowBolt);

        #region IAudibleOpenable Members
        protected string GetMaterialString()
            => $@"{ObjectMaterial.SoundQuality}";

        public virtual SoundRef GetOpeningSound(Func<Guid> idFactory, object source, ulong serialState)
            => new(new Audible(idFactory(), ID, @"opening", 
                (0, @"sliding"),
                (5, $@"{GetMaterialString()} sliding"),
                (10, $@"{GetMaterialString()} bolt sliding")),
                12, 90, serialState);

        public virtual SoundRef GetOpenedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new(new Audible(idFactory(), ID, @"opened", 
                (0, @"thump"),
                (5, $@"{GetMaterialString()} thump"),
                (10, $@"{GetMaterialString()} bolt stopping")),
                10, 90, serialState);

        public virtual SoundRef GetClosingSound(Func<Guid> idFactory, object source, ulong serialState)
            => new(new Audible(idFactory(), ID, @"closing", 
                (0, @"sliding"),
                (5, $@"{GetMaterialString()} sliding"),
                (10, $@"{GetMaterialString()} bolt sliding")),
                12, 90, serialState);

        public virtual SoundRef GetClosedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new(new Audible(idFactory(), ID, @"closed", 
                (0, @"thump"),
                (5, $@"{GetMaterialString()} thump"),
                (10, $@"{GetMaterialString()} bolt stopping")),
                10, 90, serialState);

        public virtual SoundRef GetBlockedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new(new Audible(idFactory(), ID, @"blocked", 
                (0, @"rattling"),
                (10, $@"{GetMaterialString()}rattling")),
                10, 90, serialState);
        #endregion
    }
}
