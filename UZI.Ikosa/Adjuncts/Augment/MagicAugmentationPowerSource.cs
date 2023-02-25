using System;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Power Source for a magical augmentation
    /// </summary>
    [Serializable]
    public class MagicAugmentationPowerSource : MagicPowerActionSource
    {
        /// <summary>
        /// Power Source for a magical augmentation
        /// </summary>
        public MagicAugmentationPowerSource(IPowerClass powerClass, int powerLevel, IMagicPowerActionDef powerDef)
            : base(powerClass, powerLevel, powerDef)
        {
        }

        public override void UsePower() { }

        public override string DisplayName
            => PowerActionDef.DisplayName;

        public static MagicAugmentationPowerSource CreateItemPowerSource(MagicType magicType, Alignment alignment, 
            int casterLevel, Type casterClassType, int powerLevel, MagicStyle magicStyle, 
            string displayName, string description, string key, params Descriptor[] descriptors)
        {
            // create an item caster
            return CreateItemPowerSource(
                new ItemCaster(magicType, casterLevel, alignment, 10 + casterLevel + (casterLevel / 2),
                Guid.Empty, casterClassType), powerLevel, magicStyle, displayName, description, key, descriptors);
        }

        public static MagicAugmentationPowerSource CreateItemPowerSource(IPowerClass powerClass, int powerLevel, 
            MagicStyle magicStyle, string displayName, string description, string key, params Descriptor[] descriptors)
        {
            // the power creating the augmentation
            var _powerDef = new MagicAugmentPowerDef(magicStyle, displayName, description, key, descriptors);

            // the source for this particular augmentation instance (based on the power)
            return new MagicAugmentationPowerSource(powerClass, powerLevel, _powerDef);
        }
    }
}
