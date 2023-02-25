using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class FreshnessTime
    {
        [DataMember]
        public double LastTime { get; set; }
        [DataMember]
        public int Generation { get; set; }

        public void UpdateFreshness(double currentTime)
        {
            if (currentTime > LastTime)
            {
                LastTime = currentTime;
                Generation = 0;
            }
            else
            {
                Generation++;
            }
        }
        public static bool operator >(FreshnessTime a, FreshnessTime b)
        {
            return (a.LastTime > b.LastTime)
                || ((a.LastTime == b.LastTime) && (a.Generation > b.Generation));
        }
        public static bool operator <(FreshnessTime a, FreshnessTime b)
        {
            return (a.LastTime < b.LastTime)
                || ((a.LastTime == b.LastTime) && (a.Generation < b.Generation));
        }
        public static bool operator ==(FreshnessTime a, FreshnessTime b)
        {
            return (a.LastTime == b.LastTime) && (a.Generation == b.Generation);
        }
        public static bool operator !=(FreshnessTime a, FreshnessTime b)
        {
            return (a.LastTime != b.LastTime) || (a.Generation != b.Generation);
        }
    }
}
