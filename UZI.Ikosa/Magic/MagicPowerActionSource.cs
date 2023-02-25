using System;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Magic
{
    /// <summary>Used as a source for interactions [InteractData]</summary>
    [Serializable]
    public abstract class MagicPowerActionSource : PowerActionSource, IMagicPowerActionSource
    {
        protected MagicPowerActionSource(IPowerClass powerClass, int powerLevel, IMagicPowerActionDef magicDef)
            : base(powerClass, powerLevel, magicDef)
        {
        }

        public IMagicPowerActionDef MagicPowerActionDef
            => PowerActionDef as IMagicPowerActionDef;

        #region IMagicAura Members
        public MagicStyle MagicStyle
            => MagicPowerActionDef.MagicStyle;

        /// <summary>Caster Level for the CasterClass qualified for this spell source</summary>
        public int CasterLevel
        {
            get
            {
                // if power class is direct get creature for class
                Creature _critter = null;
                if (PowerClass is AdvancementClass _trueClass)
                {
                    _critter = _trueClass.Creature;
                }

                return PowerClass.ClassPowerLevel.QualifiedValue(new Qualifier(_critter, this, null));
            }
        }
        #endregion

        #region IAura Members
        public AuraStrength MagicStrength
        {
            get
            {
                if (PowerLevel < 4)
                    return AuraStrength.Faint;
                if (PowerLevel < 7)
                    return AuraStrength.Moderate;
                if (PowerLevel < 10)
                    return AuraStrength.Strong;
                return AuraStrength.Overwhelming;
            }
        }

        public AuraStrength AuraStrength => MagicStrength;
        #endregion

        protected MPSInfo ToInfo<MPSInfo>()
            where MPSInfo : MagicPowerSourceInfo, new()
        {
            return new MPSInfo
            {
                ID = ID,
                Message = MagicPowerActionDef.DisplayName,
                Description = MagicPowerActionDef.Description,
                PowerLevel = PowerLevel,
                CasterLevel = CasterLevel,
                AuraStrength = AuraStrength,
                PowerClass = PowerClass.ToPowerClassInfo(),
                MagicPowerDef = MagicPowerActionDef.ToMagicPowerDefInfo()
            };
        }

        public MagicPowerSourceInfo ToMagicPowerSource()
            => ToInfo<MagicPowerSourceInfo>();

        public IMagicPowerDef MagicPowerDef
            => PowerDef as IMagicPowerDef;
    }
}