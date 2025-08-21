using System;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public abstract class MagicPowerSource : PowerSource, IMagicPowerSource
    {
        protected MagicPowerSource(IPowerClass powerClass, int powerLevel, IMagicPowerDef magicDef)
            : base(powerClass, powerLevel, magicDef)
        {
            _PowerLevel = powerLevel;
        }

        private int _PowerLevel;

        public IMagicPowerDef MagicPowerDef => PowerDef as IMagicPowerDef;

        #region IMagicAura Members
        public MagicStyle MagicStyle => MagicPowerDef.MagicStyle;

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
                {
                    return AuraStrength.Faint;
                }

                if (PowerLevel < 7)
                {
                    return AuraStrength.Moderate;
                }

                if (PowerLevel < 10)
                {
                    return AuraStrength.Strong;
                }

                return AuraStrength.Overwhelming;
            }
        }

        public AuraStrength AuraStrength { get { return MagicStrength; } }
        #endregion

        protected MPSInfo ToInfo<MPSInfo>()
            where MPSInfo : MagicPowerSourceInfo, new()
            => new MPSInfo
            {
                ID = ID,
                Message = MagicPowerDef.DisplayName,
                PowerLevel = PowerLevel,
                CasterLevel = CasterLevel,
                AuraStrength = AuraStrength,
                PowerClass = PowerClass.ToPowerClassInfo(),
                MagicPowerDef = MagicPowerDef.ToMagicPowerDefInfo()
            };

        public MagicPowerSourceInfo ToMagicPowerSource()
            => ToInfo<MagicPowerSourceInfo>();
    }
}
