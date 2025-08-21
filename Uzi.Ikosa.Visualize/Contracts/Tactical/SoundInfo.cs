using System;
using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class SoundInfo
    {
        [DataMember]
        public double Strength { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string ResourceKey { get; set; }

        public bool IsMatch(SoundInfo tester)
            => Description == tester.Description;   // TODO: match by resourceKey

        public static string GetStrengthDescription(double presence, double strength, int exceed)
        {
            if (exceed < 8)
            {
                if (strength >= 0.8)
                {
                    return @"strong ";
                }

                if (strength <= 0.2)
                {
                    return @"faint ";
                }
            }
            else if (exceed < 12)
            {
                if (strength >= 0.7)
                {
                    return (presence >= 90 ? "loud " : "close ");
                }

                if (strength <= 0.3)
                {
                    return (presence <= 75 ? "soft " : "far ");
                }
            }
            else
            {
                if (strength >= 0.8)
                {
                    return $@"very {(presence >= 90 ? "loud " : "close ")}";
                }

                if (strength >= 0.6)
                {
                    return (presence >= 90 ? "loud " : "close ");
                }

                if (strength <= 0.2)
                {
                    return (presence <= 75 ? "soft " : "far ");
                }

                if (strength <= 0.4)
                {
                    return $@"very {(presence <= 75 ? "soft " : "far ")} ";
                }
            }
            return string.Empty;
        }
    }
}
