using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using Uzi.Core.Contracts;
using System.Windows;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    [KnownType(typeof(Description))]
    public class WeaponHeadInfo : CoreInfo
    {
        #region construction
        public WeaponHeadInfo()
        {
        }

        protected WeaponHeadInfo(WeaponHeadInfo copySource)
            : base(copySource)
        {
            Material = copySource.Material.Clone() as MaterialInfo;
            DamageTypes = new Collection<DamageType>(copySource.DamageTypes.ToList());
            CriticalRange = copySource.CriticalRange;
            CriticalLow = copySource.CriticalLow;
            CriticalRangeFactor = copySource.CriticalRangeFactor.Clone() as DeltableInfo;
            CriticalDamageFactor = copySource.CriticalDamageFactor.Clone() as DeltableInfo;
            AdjunctInfos = copySource.AdjunctInfos.Select(_a => _a.Clone() as Info).ToArray();
            IsMainHead = copySource.IsMainHead;
        }
        #endregion

        [DataMember]
        public MaterialInfo Material { get; set; }
        [DataMember]
        public string DamageRollString { get; set; }
        [DataMember]
        public Collection<DamageType> DamageTypes { get; set; }
        [DataMember]
        public int CriticalRange { get; set; }
        [DataMember]
        public int CriticalLow { get; set; }
        [DataMember]
        public DeltableInfo CriticalRangeFactor { get; set; }
        [DataMember]
        public DeltableInfo CriticalDamageFactor { get; set; }
        [DataMember]
        public Info[] AdjunctInfos { get; set; }
        [DataMember]
        public bool IsMainHead { get; set; }

        public Visibility AdjunctVisibility
            => (AdjunctInfos?.Any() ?? false) ? Visibility.Visible : Visibility.Collapsed;

        public override object Clone()
        {
            return new WeaponHeadInfo(this);
        }

        /// <summary>
        /// Compact combat summary string
        /// </summary>
        public string CombatSummary
        {
            get
            {
                string _critical()
                {
                    var _low = (CriticalLow < 20);
                    var _mult = (CriticalDamageFactor.BaseValue > 2);
                    if (_low)
                    {
                        if (_mult)
                            return $@" ({CriticalLow}-20/X{CriticalDamageFactor.BaseValue})";
                        return $@" ({CriticalLow}-20)";
                    }
                    else if (_mult)
                    {
                        return $@" (X{CriticalDamageFactor.BaseValue})";
                    }
                    return string.Empty;
                }

                string _damageTypes()
                {
                    if (DamageTypes.Any())
                    {
                        if (DamageTypes.Count == 1)
                            return DamageTypes.First().ToString();
                        return string.Join(" | ", DamageTypes.Select(_d => _d.ToString()));
                    }
                    return string.Empty;
                }

                return $@"{DamageRollString}{_critical()} {_damageTypes()}";
            }
        }
    }
}