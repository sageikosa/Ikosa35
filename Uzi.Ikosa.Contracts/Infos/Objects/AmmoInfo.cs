using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class AmmoInfo : WeaponHeadInfo
    {
        public AmmoInfo()
            : base()
        {
        }

        protected AmmoInfo(AmmoInfo copySource)
            : base(copySource)
        {
            Count = copySource.Count;
            InfoIDs = new HashSet<Guid>(copySource.InfoIDs.Select(_g => _g).Distinct());
            Key = copySource.Key;
            BundleID = copySource.BundleID;
        }

        /// <summary>Total in set</summary>
        [DataMember]
        public int Count { get; set; }

        [DataMember]
        public HashSet<Guid> InfoIDs { get; set; }

        [DataMember]
        public Guid BundleID { get; set; }

        /// <summary>
        /// Should be concatenated InfoIDs
        /// </summary>
        [DataMember]
        public string Key { get; set; }

        public override string CompareKey => Key;

        public Visibility CountVisibility
            => (Count > 1)
            ? Visibility.Visible
            : Visibility.Collapsed;

        public override object Clone()
        {
            return new AmmoInfo(this);
        }

        public static string GetKey(Guid bundleID, HashSet<Guid> infoIDs)
            => string.Concat(bundleID.ToString(), @"|",
                string.Join(@"|", infoIDs.OrderBy(_g => _g).Select(_g => _g.ToString())));
    }
}
