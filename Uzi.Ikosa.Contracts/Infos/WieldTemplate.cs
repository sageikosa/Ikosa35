using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    /// <summary>
    /// Unarmed, Light, OneHanded, TwoHanded, Double
    /// </summary>
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public enum WieldTemplate
    {
        [EnumMember]
        TooSmall = -1,
        [EnumMember]
        /// <summary>Light</summary>
        Unarmed = 0,
        [EnumMember]
        /// <summary>Main hand gives STR to damage, off hand gives 1/2 STR to damage</summary>
        Light,
        [EnumMember]
        /// <summary>Main hand gives STR to damage, off hand gives 1/2 STR to damage.  Can wield 2-handed</summary>
        OneHanded,
        [EnumMember]
        /// <summary>Must wield two-handed, gives 1.5 STR to damage.</summary>
        TwoHanded,
        [EnumMember]
        /// <summary>Can wield double (both ends), two-handed (one end) or one-handed (one end)</summary>
        Double,
        /// <summary>Cannot be wielded (due to size differences with creature)</summary>
        [EnumMember]
        TooBig
    }

    public static class WieldTemplateHelper
    {
        public static int OpposedDelta(this WieldTemplate template)
        {
            switch (template)
            {
                case WieldTemplate.TooSmall:
                case WieldTemplate.Unarmed:
                case WieldTemplate.Light:
                    return -4;

                case WieldTemplate.OneHanded:
                    return 0;

                case WieldTemplate.TwoHanded:
                case WieldTemplate.Double:
                case WieldTemplate.TooBig:
                default:
                    return 4;
            }
        }

        public static bool NotIn(this WieldTemplate self, params WieldTemplate[] others)
            => !others.Contains(self);

        public static bool In(this WieldTemplate self, params WieldTemplate[] others)
            => others.Contains(self);

        public static string GetWieldString(this WieldTemplate self)
        {
            switch (self)
            {
                case WieldTemplate.Unarmed: return @"unarmed";
                case WieldTemplate.Light: return @"light";
                case WieldTemplate.OneHanded: return @"one-handed";
                case WieldTemplate.TwoHanded: return @"two-handed";
                case WieldTemplate.Double: return @"double-headed";
                default: return @"unwieldy";
            }
        }
    }
}
