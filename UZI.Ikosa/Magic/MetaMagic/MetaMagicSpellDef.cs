using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic
{
    /// <summary>Base class for metamagic spell def effects</summary>
    [Serializable]
    public abstract class MetaMagicSpellDef : ISpellDef
    {
        protected MetaMagicSpellDef(ISpellDef wrapped, Guid metaID, bool isSpontaneous)
        {
            _Wrapped = wrapped.ForPowerSource() as ISpellDef;
            _MetaID = metaID;
            _IsSpontaneous = isSpontaneous;
        }

        #region data
        private Guid _MetaID;
        private bool _IsSpontaneous;
        private ISpellDef _Wrapped;
        #endregion

        public ISpellDef Wrapped => _Wrapped;
        public bool IsSpontaneous => _IsSpontaneous;

        public MagicPowerDefInfo ToMagicPowerDefInfo()
            => this.GetSpellDefInfo();

        public PowerDefInfo ToPowerDefInfo()
            => ToMagicPowerDefInfo();

        /// <summary>True if any wrapper from this to the underlying ISpellMode matches the supplied metamagicwrapper</summary>
        public bool HasWrapper<MetaWrap>() where MetaWrap : MetaMagicSpellDef
        {
            // if this wrapper is the correct type, return true
            if (GetType() == typeof(MetaWrap))
            {
                return true;
            }

            // if this is wrapping a metamagic wrapper itself, dig further in
            return (_Wrapped as MetaMagicSpellDef)?.HasWrapper<MetaWrap>() ?? false;
        }

        public abstract string MetaTag { get; }
        public abstract int SlotAdjustment { get; }
        public abstract string Benefit { get; }

        /// <summary>Typically the unique guid for the feat providing access to this meta-magic</summary>
        public Guid PresenterID => _MetaID;

        // ISpellDef Members
        // fixed parts
        public string Key => $@"{_Wrapped.Key}[{MetaTag}]";
        public string DisplayName => $@"{_Wrapped.DisplayName} ({MetaTag})";
        public string Description => _Wrapped.Description;
        public IPowerDef ForPowerSource() => this;
        public MagicStyle MagicStyle => _Wrapped.MagicStyle;
        public SpellDef SeedSpellDef => _Wrapped.SeedSpellDef;

        // overridable parts
        public virtual IEnumerable<Descriptor> Descriptors => _Wrapped.Descriptors;

        public virtual bool HasPlanarCompatibility(PlanarPresence source, PlanarPresence target)
            => _Wrapped.HasPlanarCompatibility(source, target);

        #region public virtual ActionTime CastingTime { get; }
        public virtual ActionTime ActionTime
        {
            get
            {
                if (_IsSpontaneous)
                {
                    if (SeedSpellDef.ActionTime.ActionTimeType != TimeType.Span)
                    {
                        return new ActionTime(Round.UnitFactor);
                    }
                    else
                    {
                        return new ActionTime(SeedSpellDef.SeedSpellDef.ActionTime.SpanLength + 1);
                    }
                }
                return _Wrapped.ActionTime;
            }
        }
        #endregion

        public virtual IEnumerable<SpellComponent> DivineComponents => _Wrapped.DivineComponents;
        public virtual IEnumerable<SpellComponent> ArcaneComponents => _Wrapped.ArcaneComponents;
        public virtual IEnumerable<ISpellMode> SpellModes => _Wrapped.SpellModes;
        public virtual bool ArcaneCharisma => _Wrapped.ArcaneCharisma;
        public virtual IEnumerable<Type> SimilarSpells => _Wrapped.SimilarSpells;
    }
}
