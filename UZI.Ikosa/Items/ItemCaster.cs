using System;
using System.Collections.Generic;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Contracts;
using Newtonsoft.Json;
using System.Linq;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// Snapshot of properties needed for magic stored in items
    /// </summary>
    [Serializable]
    public class ItemCaster : ICasterClass
    {
        #region Construction
        public ItemCaster(MagicType magicType, int casterLevel, Alignment alignment, int difficulty, Guid casterID, Type casterClassType)
        {
            _MType = magicType;
            _PowerLevel = new DeltableQualifiedDelta(casterLevel, @"Item Caster Level", this);
            _Align = alignment;
            _Difficulty = new Deltable(difficulty);
            _Guid = casterID;
            _Type = casterClassType;
        }

        /// <summary>Constructor for staff replacement difficulty</summary>
        public ItemCaster(MagicType magicType, IVolatileValue casterLevel, Alignment alignment, IDeltable difficulty, Guid casterID, Type casterClassType)
        {
            _MType = magicType;
            _PowerLevel = casterLevel;
            _Align = alignment;
            _Difficulty = difficulty;
            _Guid = casterID;
            _Type = casterClassType;
        }
        #endregion

        #region Private Data
        private MagicType _MType;
        private IVolatileValue _PowerLevel;
        private Alignment _Align;
        private IDeltable _Difficulty;
        private Guid _Guid;
        private Type _Type;
        #endregion

        public string ClassName => nameof(ItemCaster);
        public string Key => typeof(ItemCaster).FullName;
        public Guid OwnerID => _Guid;
        public MagicType MagicType => _MType;
        public Alignment Alignment => _Align;
        public Type CasterClassType => _Type;
        public IVolatileValue EffectiveLevel => _PowerLevel;
        public IDeltable SpellDifficultyBase => _Difficulty;
        public CastingAbilityBase BonusSpellAbility => null;
        public IVolatileValue ClassPowerLevel => _PowerLevel;
        public CastingAbilityBase SpellDifficultyAbility => null;

        public string ClassIconKey => nameof(ItemCaster);
        public bool IsPowerClassActive { get => true; set { } }
        public IEnumerable<ClassSpell> UsableSpells => Enumerable.Empty<ClassSpell>();
        public IEnumerable<ClassSpell> CastableSpells => Enumerable.Empty<ClassSpell>();

        #region IControlChange<Activation> Members

        public void AddChangeMonitor(IMonitorChange<Activation> monitor)
        {
            // NOTHING
        }

        public void RemoveChangeMonitor(IMonitorChange<Activation> monitor)
        {
            // NOTHING
        }

        #endregion

        #region INotifyPropertyChanged Members
        [field: NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion

        public PowerClassInfo ToPowerClassInfo()
            => ToCasterClassInfo();

        #region public CasterClassInfo ToCasterClassInfo()

        public CasterClassInfo ToCasterClassInfo()
            => new CasterClassInfo
            {
                ClassPowerLevel = _PowerLevel.ToVolatileValueInfo(),
                OwnerID = OwnerID.ToString(),
                ID = OwnerID,
                Key = Key,
                IsPowerClassActive = IsPowerClassActive,
                MagicType = MagicType,
                Alignment = Alignment.ToString(),
                EffectiveLevel = EffectiveLevel.ToVolatileValueInfo(),
                SpellDifficultyBase = SpellDifficultyBase.ToDeltableInfo(),
                Icon = new ImageryInfo { Keys = ClassIconKey.ToEnumerable().ToArray() }
            };

        #endregion

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => (new QualifyingDelta(_PowerLevel.QualifiedValue(qualify), GetType(), GetType().Name)).ToEnumerable();

        private TerminateController _TCtrl;
        private TerminateController Term
            => _TCtrl ??= new TerminateController(this);

        public void DoTerminate()
            => Term.DoTerminate();

        public void AddTerminateDependent(IDependOnTerminate subscriber)
            => Term.AddTerminateDependent(subscriber);

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            => Term.RemoveTerminateDependent(subscriber);

        public int TerminateSubscriberCount => Term.TerminateSubscriberCount;

        public bool CanUseDescriptor(Descriptor descriptor) => true;
    }
}
