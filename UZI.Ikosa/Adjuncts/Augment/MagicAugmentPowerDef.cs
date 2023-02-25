using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class MagicAugmentPowerDef : IMagicPowerActionDef
    {
        public MagicAugmentPowerDef(MagicStyle style,
            string displayName, string description, string key,
            params Descriptor[] descriptors)
        {
            _Style = style;
            _Name = displayName;
            _Description = description;
            _Key = key;
            _Descriptors = descriptors.ToList();
        }

        #region data
        private MagicStyle _Style;
        private string _Name;
        private string _Description;
        private string _Key;
        private List<Descriptor> _Descriptors;
        #endregion

        #region IMagicPowerDef Members

        public MagicStyle MagicStyle => _Style;

        public MagicPowerDefInfo ToMagicPowerDefInfo()
            => this.GetMagicPowerDefInfo();

        #endregion

        // IPowerDef Members
        public string DisplayName => _Name;
        public string Description => _Description;
        public string Key => _Key;
        public IPowerDef ForPowerSource() => this;
        public IEnumerable<Descriptor> Descriptors => _Descriptors.Select(_d => _d);
        public ActionTime ActionTime => new ActionTime(Contracts.TimeType.Free);

        public PowerDefInfo ToPowerDefInfo()
            => ToMagicPowerDefInfo();

        /// <summary>Default: true if overlapping, or source-material to target-ethereal with force descriptor</summary>
        public virtual bool HasPlanarCompatibility(PlanarPresence source, PlanarPresence target)
        {
            if (source.HasOverlappingPresence(target))
                return true;
            if (source.HasMaterialPresence() && target.HasEtherealPresence() && Descriptors.OfType<Force>().Any())
                return true;
            return false;
        }
    }
}