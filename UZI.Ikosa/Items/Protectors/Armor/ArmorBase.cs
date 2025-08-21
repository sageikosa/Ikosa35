using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Abilities;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Armor
{
    [Serializable]
    public abstract class ArmorBase : ProtectorItemBase, IArmor, IMonitorChange<Size>
    {
        #region Construction
        protected ArmorBase(string name, ArmorProficiencyType proficiency, int bonus, int maxDex,
            int checkPenalty, int spellFailure)
            : base(name, ItemSlot.ArmorRobeSlot, bonus, checkPenalty, spellFailure)
        {
            _BaseProtect = bonus;
            Sizer.NaturalSize = Size.Medium;
            _ProficiencyType = proficiency;
            _MaxDex = new ConstDeltable(maxDex);
            _BodyType = typeof(HumanoidBody);
        }

        protected ArmorBase(string name, ArmorProficiencyType proficiency, int bonus, int maxDex,
            int checkPenalty, int spellFailure, Type bodyType)
            : base(name, ItemSlot.ArmorRobeSlot, bonus, checkPenalty, spellFailure)
        {
            if (bodyType.IsSubclassOf(typeof(Body)))
            {
                _BaseProtect = bonus;
                Sizer.NaturalSize = Size.Medium;
                _ProficiencyType = proficiency;
                _MaxDex = new ConstDeltable(maxDex);
                _BodyType = bodyType;
            }
            else
            {
                throw new ArgumentOutOfRangeException("bodyType", "Only body types allowed for armor");
            }
        }
        #endregion

        #region INIT
        /// <summary>Set up other armor qualities not accounted for in the constructor base</summary>
        protected virtual void Init(decimal basePrice, double weight, Materials.Material material)
        {
            var _costFactor = 1m;
            if (!typeof(HumanoidBody).IsAssignableFrom(BodyType))
            {
                _costFactor = 2m;
            }

            ItemMaterial = material;
            Price.CorePrice = basePrice * _costFactor;
            BaseWeight = weight;
            MaxStructurePoints.BaseValue = 1;
            _ArmorStruc = new Delta(_BaseProtect * 5, this);
            MaxStructurePoints.Deltas.Add(_ArmorStruc);
            ItemSizer.AddChangeMonitor(this);
        }
        #endregion

        #region private data
        protected int _BaseProtect;
        protected ArmorProficiencyType _ProficiencyType;
        protected ConstDeltable _MaxDex;
        private Type _BodyType;
        protected Delta _ArmorStruc;
        #endregion

        // IArmor Members
        public virtual ArmorProficiencyType ProficiencyType => _ProficiencyType;
        public ConstDeltable MaxDexterityBonus => _MaxDex;
        public override object Source => typeof(IArmor);

        /// <summary>Type for the body</summary>
        public Type BodyType => _BodyType;

        protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        {
            base.OnClearSlots(slotA, slotB);
            if (slotA != null)
            {
                CreaturePossessor.MaxDexterityToARBonus.ClearValue(typeof(ArmorBase));
            }
        }

        protected override void OnSetItemSlot()
        {
            base.OnSetItemSlot();
            if (MainSlot != null)
            {
                // modify the creature's AR
                Creature _creature = CreaturePossessor;
                if (ProficiencyType != ArmorProficiencyType.None)
                {
                    _creature.MaxDexterityToARBonus.SetValue(typeof(ArmorBase), MaxDexterityBonus.EffectiveValue);
                    if (!_creature.Proficiencies.IsProficientWith(this, _creature.AdvancementLog.NumberPowerDice))
                    {
                        // DEX/STR skills not normally affected by armor check
                        _Penalty = new Delta(CheckPenalty.EffectiveValue, typeof(ArmorBase), @"Armor non-proficiency");
                        foreach (var _skill in from _s in _creature.Skills
                                               where (new string[] { MnemonicCode.Str, MnemonicCode.Dex }).Contains(_s.KeyAbilityMnemonic)
                                               && (_s.CheckFactor == 0)
                                               select _s)
                        {
                            _skill.Deltas.Add(_Penalty);
                        }
                    }
                }
            }
        }

        #region IMonitorChange<Size> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<Size> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<Size> args) { }
        public void ValueChanged(object sender, ChangeValueEventArgs<Size> args)
        {
            // adjust protection factor when size changes
            if (args.OldValue.ArmorBonusFactor != args.NewValue.ArmorBonusFactor)
            {

                ProtectionBonus.BaseValue = (int)(_BaseProtect * args.NewValue.ArmorBonusFactor);
                if (ProtectionBonus.BaseValue < 1)
                {
                    ProtectionBonus.BaseValue = 1;
                }
            }

            // adjust max structure points (based upon armor bonus)
            _ArmorStruc.Value = (int)(ProtectionBonus.BaseDoubleValue * 5d * args.NewValue.ItemWeightFactor);
            if (_ArmorStruc.Value < 1)
            {
                _ArmorStruc.Value = 1;
            }
        }
        #endregion

        public override Info GetInfo(CoreActor actor, bool baseValues)
        {
            var _info = ToProtectorInfo<ArmorInfo>(actor, baseValues);
            _info.ProficiencyType = ProficiencyType;
            _info.BodyType = BodyType.SourceName();
            if (baseValues)
            {
                _info.MaxDexterityBonus = new DeltableInfo(MaxDexterityBonus.BaseValue);
            }
            else
            {
                _info.MaxDexterityBonus = MaxDexterityBonus.ToDeltableInfo();
            }
            return _info;
        }

        public override bool SlottingProvokes
            => true;

        public override bool UnslottingProvokes
            => true;
    }
}
