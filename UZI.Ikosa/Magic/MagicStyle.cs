using System;
using Uzi.Core;

namespace Uzi.Ikosa.Magic
{
    /// <summary>Abstract base for Evocation, Conjuration, Enchantment, General, Illusion, Necromancy, Divination, Transformation and Abjuration</summary>
    /// <remarks>NOTE: these are defined as ICore (null CurrentSetting) so that they can be used with QualifiedDeltas</remarks>
    [Serializable]
    public abstract class MagicStyle : ICore
    {
        public virtual string StyleName => GetType().Name;
        public abstract string SpecialistName { get; }

        #region ICore Members
        public abstract Guid ID { get; }
        public CoreSetting CurrentSetting { get { return null; } set {/* NOP */ } }
        #endregion
    }

    [Serializable]
    public class Abjuration : MagicStyle
    {
        public override Guid ID => new Guid(@"{CB243080-39D5-482d-80AA-8E2C33E361FC}");
        public override string SpecialistName => @"Abjurationist";
    }

    [Serializable]
    public class Conjuration : MagicStyle
    {
        public enum SubConjure
        {
            Calling,
            Creation,
            Healing,
            Summoning,
            Teleportation
        }
        public override Guid ID => new Guid(@"{F62992DA-ABB4-428b-83E5-93970EFE318F}");
        public Conjuration(SubConjure subStyle) { SubStyle = subStyle; }
        public SubConjure SubStyle { get; private set; }
        public override string StyleName => $@"{base.StyleName} ({SubStyle})";
        public override string SpecialistName => @"Conjurer";
    }

    /// <summary>Foretelling, extra-sensory action</summary>
    [Serializable]
    public class Divination : MagicStyle
    {
        public enum SubDivination
        {
            /// <summary>Makes things clearly understood</summary>
            Illumination,
            /// <summary>Predicts future situations</summary>
            Precognition,
            /// <summary>Sees things from far away (includes scrying)</summary>
            RemoteSensing,
            /// <summary>See auras and essences of objects and creatures</summary>
            Detection,
            /// <summary>Search the past for hidden knowledge</summary>
            Lore
        }
        public Divination(SubDivination subStyle) { SubStyle = subStyle; }
        public override Guid ID => new Guid(@"{D716182A-5B9A-405b-ADF2-5A771ECB746D}");
        public SubDivination SubStyle { get; private set; }
        public override string StyleName => $@"{base.StyleName} ({SubStyle})";
        public override string SpecialistName => @"Diviner";
    }

    [Serializable]
    public class Enchantment : MagicStyle
    {
        public enum SubEnchantment
        {
            Charm,
            Compulsion
        }
        public Enchantment(SubEnchantment subStyle) { SubStyle = subStyle; }
        public SubEnchantment SubStyle { get; private set; }
        public override Guid ID => new Guid(@"{F6A0F551-ADBE-425f-B232-63543EE458D5}");
        public override string StyleName => $@"{base.StyleName} ({SubStyle})";
        public override string SpecialistName => @"Enchanter";
    }

    /// <summary>Conducts energy from magical sources</summary>
    [Serializable]
    public class Evocation : MagicStyle
    {
        public override Guid ID => new Guid(@"{A35A7928-415D-4c50-8ADF-42E684E806C5}");
        public override string SpecialistName => @"Evoker";
    }

    [Serializable]
    public class Illusion : MagicStyle
    {
        public enum SubIllusion
        {
            Figment,
            Glamer,
            Pattern,
            Phantasm,
            Shadow
        }
        public Illusion(SubIllusion subStyle) { SubStyle = subStyle; }
        public SubIllusion SubStyle { get; private set; }
        public override Guid ID => new Guid(@"{0B3200CF-3DD5-48f7-92BD-BD20B4B96420}");
        public override string StyleName => $@"{base.StyleName} ({SubStyle})";
        public override string SpecialistName => @"Illusionist";
    }

    [Serializable]
    public class Necromancy : MagicStyle
    {
        public override Guid ID => new Guid(@"{9887A751-CE00-4a1b-B36F-4917365E6FDF}");
        public override string SpecialistName => @"Necromancer";
    }

    [Serializable]
    public class Transformation : MagicStyle
    {
        public override Guid ID => new Guid(@"{070262AF-ED18-481b-8EAA-B5CA6D34ECDE}");
        public override string SpecialistName => @"Transformagrafist";
    }

    [Serializable]
    public class General : MagicStyle
    {
        public override Guid ID => new Guid(@"{9FC1508A-79D5-45d8-BFFF-5FB6ED564D12}");
        public override string SpecialistName => @"Universalist";
    }
}