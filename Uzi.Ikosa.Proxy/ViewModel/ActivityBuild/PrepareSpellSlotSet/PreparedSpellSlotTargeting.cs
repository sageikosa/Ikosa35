using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class PreparedSpellSlotTargeting : ViewModelBase
    {
        #region ctor()
        public PreparedSpellSlotTargeting(SpellSlotLevelTargeting spellSlotLevel, PreparedSpellSlotInfo preparedSpellSlot)
        {
            // unchanging
            _Level = spellSlotLevel;
            _Original = preparedSpellSlot;

            // generating
            _SpellSlot = preparedSpellSlot?.Clone() as PreparedSpellSlotInfo;
            _Selected = _Level.AvailableClassSpells
                .FirstOrDefault(_spell => _spell.SpellDef?.Key == preparedSpellSlot.SpellSource?.SpellDef?.Key)
                ?? SpellSlotLevelTargeting.Unslotted;

            // meta magics management
            _MetaMagics = new ObservableCollection<MetaMagicInfo>();
            _AvailableMetaMagics = new List<MetaMagicInfo>();
            if (preparedSpellSlot.SpellSource?.SpellDef?.MetaMagics.Any() ?? false)
            {
                // conformulate to begin
                foreach (var _meta in preparedSpellSlot.SpellSource.SpellDef.MetaMagics)
                {
                    _MetaMagics.Add(_meta);
                }
            }
            SyncMetaMagics();
            _Add = new RelayCommand<MetaMagicInfo>(
                (meta) =>
                {
                    if ((SpellSlotLevel.SpellSlotSet.PrepareSpellSlots.PrepareSpellSlotsAimInfo.AvailableMetaMagics
                        .FirstOrDefault(_mm => _mm.MetaTag == meta.MetaTag) is MetaMagicInfo _adder)
                        && !MetaMagics.Any(_mm => _mm.MetaTag == meta.MetaTag))
                    {
                        // add to observable collection and spell source being built
                        _adder = _adder.Clone() as MetaMagicInfo;
                        MetaMagics.Add(_adder);
                        PreparedSpellSlotInfo.SpellSource.SpellDef.MetaMagics.Add(_adder);
                        DoPropertyChanged(nameof(AdjustedLevel));
                        SyncMetaMagics();
                    }
                },
                (meta) => (SpellSourceInfo?.SpellDef?.Key != null)
                            && (meta != null)
                            && !MetaMagics.Any(_mm => _mm.MetaTag == meta.MetaTag)
                            && ((AdjustedLevel + meta.SlotAdjustment) <= SpellSlotLevel.SpellSlotLevelInfo.SlotLevel)
                );
            _Remove = new RelayCommand<MetaMagicInfo>(
                (meta) =>
                {
                    if (MetaMagics.FirstOrDefault(_mm => _mm.MetaTag == meta.MetaTag) is MetaMagicInfo _remover)
                    {
                        // remove from observable and spell source being built
                        MetaMagics.Remove(_remover);
                        PreparedSpellSlotInfo.SpellSource.SpellDef.MetaMagics.Remove(_remover);
                        DoPropertyChanged(nameof(AdjustedLevel));
                        SyncMetaMagics();
                    }
                },
                (meta) => (meta != null) && MetaMagics.Any(_mm => _mm.MetaTag == meta.MetaTag)
                );
        }
        #endregion

        #region data
        // unchanging
        private readonly SpellSlotLevelTargeting _Level;
        private readonly PreparedSpellSlotInfo _Original;

        // updating
        private readonly PreparedSpellSlotInfo _SpellSlot;
        private ClassSpellInfo _Selected;

        // meta-magics
        private readonly ObservableCollection<MetaMagicInfo> _MetaMagics;
        private readonly RelayCommand<MetaMagicInfo> _Add;
        private readonly RelayCommand<MetaMagicInfo> _Remove;
        private readonly List<MetaMagicInfo> _AvailableMetaMagics;
        #endregion

        public SpellSlotLevelTargeting SpellSlotLevel => _Level;
        public PreparedSpellSlotInfo OriginalPreparedSpellSlotInfo => _Original;

        public PreparedSpellSlotInfo PreparedSpellSlotInfo => _SpellSlot;
        public SpellSourceInfo SpellSourceInfo => PreparedSpellSlotInfo.SpellSource;

        #region public ClassSpellInfo ClassSpellInfo { get; set; }
        public ClassSpellInfo ClassSpellInfo
        {
            get => SpellSlotLevel.AvailableClassSpells.FirstOrDefault(_cs => _cs.SpellDef?.Key == _Selected?.SpellDef?.Key) ?? SpellSlotLevelTargeting.Unslotted;
            set
            {
                if (value?.SpellDef?.Key != _Selected?.SpellDef?.Key)
                {
                    var _caster = SpellSlotLevel.SpellSlotSet.PrepareSpellSlots.PrepareSpellSlotsAimInfo.CasterClass;
                    _Selected = value;
                    var _source = new SpellSourceInfo
                    {
                        CasterClass = _caster,
                        SpellDef = value.SpellDef.Clone() as SpellDefInfo,
                        SlotLevel = SpellSlotLevel.SpellSlotLevelInfo.SlotLevel,
                        PowerLevel = value.Level,
                        IsSpontaneous = false
                    };
                    PreparedSpellSlotInfo.SpellSource = _source;

                    // sync targets
                    if (SpellSlotLevel.SpellSlotSet.PrepareSpellSlots.Targets.FirstOrDefault()
                        ?.SlotSets.FirstOrDefault(_ss => _ss.SetIndex == SpellSlotLevel.SpellSlotSet.SpellSlotSetInfo.SetIndex)
                        ?.SlotLevels.FirstOrDefault(_sl => _sl.SlotLevel == SpellSlotLevel.SpellSlotLevelInfo.SlotLevel)
                        ?.SpellSlots.FirstOrDefault(_s => _s.SlotIndex == OriginalPreparedSpellSlotInfo.SlotIndex) is PreparedSpellSlotInfo _prep)
                    {
                        _prep.SpellSource = _source;
                    }

                    _MetaMagics.Clear();
                    SyncMetaMagics();
                }
                DoPropertyChanged(nameof(SpellSourceInfo));
                DoPropertyChanged(nameof(ClassSpellInfo));
                DoPropertyChanged(nameof(MetaMagics));
            }
        }
        #endregion

        #region private void SyncMetaMagics()
        private void SyncMetaMagics()
        {
            _AvailableMetaMagics.Clear();
            _AvailableMetaMagics.AddRange(
                from _am in SpellSlotLevel.SpellSlotSet.PrepareSpellSlots.PrepareSpellSlotsAimInfo.AvailableMetaMagics
                where !MetaMagics.Any(_m => _m.MetaTag == _am.MetaTag)
                && ((_am.SlotAdjustment + AdjustedLevel) <= SpellSlotLevel.SpellSlotLevelInfo.SlotLevel)
                orderby _am.MetaTag
                select _am);
            DoPropertyChanged(nameof(MetaMagicStringVisibility));
            DoPropertyChanged(nameof(MetaMagicString));
            DoPropertyChanged(nameof(AvailableMetaMagics));
        }
        #endregion

        public ObservableCollection<MetaMagicInfo> MetaMagics => _MetaMagics;
        public List<MetaMagicInfo> AvailableMetaMagics => _AvailableMetaMagics;
        public RelayCommand<MetaMagicInfo> AddMetaMagic => _Add;
        public RelayCommand<MetaMagicInfo> RemoveMetaMagic => _Remove;

        public string MetaMagicString => string.Join("; ", MetaMagics.Select(_m => _m.MetaTag));
        public Visibility MetaMagicStringVisibility => MetaMagics.Any() ? Visibility.Visible : Visibility.Collapsed;

        public int AdjustedLevel
            => (ClassSpellInfo?.Level ?? 0) + MetaMagics.Sum(_mm => _mm.SlotAdjustment);

        #region public bool IsChanged { get; }
        public bool IsChanged
        {
            get
            {
                // different spell sources
                if (OriginalPreparedSpellSlotInfo?.SpellSource?.SpellDef.Key != PreparedSpellSlotInfo?.SpellSource.SpellDef.Key)
                    return true;

                // unslotted, and still unslotted
                if ((OriginalPreparedSpellSlotInfo?.SpellSource?.SpellDef.Key == null) && (SpellSourceInfo?.SpellDef.Key == null))
                    return false;

                // same not-null key, check meta-magics
                if (OriginalPreparedSpellSlotInfo.SpellSource.SpellDef.MetaMagics.Any(_om => !MetaMagics.Any(_mm => _mm.MetaTag == _om.MetaTag)))
                    return true;
                if (MetaMagics.Any(_mm => !OriginalPreparedSpellSlotInfo.SpellSource.SpellDef.MetaMagics.Any(_om => _om.MetaTag == _mm.MetaTag)))
                    return true;

                // everything the same
                return false;
            }
        }
        #endregion
    }
}
