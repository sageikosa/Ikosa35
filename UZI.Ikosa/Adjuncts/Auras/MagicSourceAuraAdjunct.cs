using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Actions;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Used by magic items that need to express an aura
    /// </summary>
    [Serializable]
    public class MagicSourceAuraAdjunct : Adjunct, IMagicAura, IAlignmentAura
    {
        public MagicSourceAuraAdjunct(MagicPowerActionSource source)
            : base(source)
        {
        }

        public MagicPowerActionSource MagicPowerActionSource => (MagicPowerActionSource)Source;

        // IMagicAura Members
        public virtual AuraStrength MagicStrength
        {
            get
            {
                if (!IsActive)
                {
                    return AuraStrength.None;
                }

                if (MagicPowerActionSource.PowerLevel < 4)
                {
                    return AuraStrength.Faint;
                }

                if (MagicPowerActionSource.PowerLevel < 7)
                {
                    return AuraStrength.Moderate;
                }

                if (MagicPowerActionSource.PowerLevel < 10)
                {
                    return AuraStrength.Strong;
                }

                return AuraStrength.Overwhelming;
            }
        }

        public virtual MagicStyle MagicStyle => MagicPowerActionSource.MagicStyle;
        public int PowerLevel => MagicPowerActionSource.PowerLevel;
        public int CasterLevel => MagicPowerActionSource.CasterLevel;

        // IAlignmentAura Members
        Alignment IAlignmentAura.Alignment
        {
            get
            {
                if (MagicPowerActionSource.MagicPowerActionDef.Descriptors.OfType<Good>().Any())
                {
                    return Alignment.NeutralGood;
                }
                if (MagicPowerActionSource.MagicPowerActionDef.Descriptors.OfType<Evil>().Any())
                {
                    return Alignment.NeutralEvil;
                }
                if (MagicPowerActionSource.MagicPowerActionDef.Descriptors.OfType<Chaotic>().Any())
                {
                    return Alignment.ChaoticNeutral;
                }
                if (MagicPowerActionSource.MagicPowerActionDef.Descriptors.OfType<Lawful>().Any())
                {
                    return Alignment.LawfulNeutral;
                }
                return Alignment.TrueNeutral;
            }
        }

        AuraStrength IAlignmentAura.AlignmentStrength
        {
            get
            {
                var _casterLevel = MagicPowerActionSource.CasterLevel;
                if (_casterLevel <= 2)
                {
                    return AuraStrength.Faint;
                }

                if (_casterLevel <= 8)
                {
                    return AuraStrength.Moderate;
                }

                if (_casterLevel <= 20)
                {
                    return AuraStrength.Strong;
                }

                return AuraStrength.Overwhelming;
            }
        }

        int IAlignmentAura.PowerLevel
            => MagicPowerActionSource.CasterLevel;

        public AuraStrength AuraStrength
            => MagicStrength;

        public override object Clone()
            => new MagicSourceAuraAdjunct(MagicPowerActionSource);
    }
}
