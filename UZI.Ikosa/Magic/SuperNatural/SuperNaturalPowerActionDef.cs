using System;
using System.Linq;
using Uzi.Ikosa.Actions;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public abstract class SuperNaturalPowerActionDef : PowerActionDef<SuperNaturalPowerActionSource>, ISuperNaturalPowerActionDef
    {
        protected SuperNaturalPowerActionDef(IPowerBattery battery)
        {
            _Battery = battery;
        }

        private IPowerBattery _Battery;

        public IPowerBattery MagicBattery => _Battery;
        public int PowerCost => 1;
        public abstract int PowerLevel { get; }
        public abstract MagicStyle MagicStyle { get; }
        public abstract SuperNaturalPowerActionDef SeedPowerDef { get; }

        #region IExtraModeRoot Members
        public virtual IMode GetCapability<IMode>() 
            where IMode : class, ICapability
            => this as IMode; 
        #endregion

        #region public static void ApplySuperNaturalEffect(PowerApplyStep<SuperNaturalPowerSource> apply)
        /// <summary>Apply a super natural effect, unless an expected save roll succeeded, otherwise do nothing.</summary>
        public static void ApplySuperNaturalEffect(PowerApplyStep<SuperNaturalPowerActionSource> apply)
        {
            if (apply.DeliveryInteraction.Target is IAdjunctable _target)
            {
                if (apply.DeliveryInteraction.InteractData is MagicPowerEffectTransit<SuperNaturalPowerActionSource> _magicPowerEffectTransit)
                {
                    foreach (var _effect in _magicPowerEffectTransit.MagicPowerEffects)
                    {
                        // assume the effect will be added
                        var _add = true;
                        if (apply.PowerUse.CapabilityRoot.GetCapability<IDurableCapable>() is IDurableCapable _durable)
                        {
                            // see if there is an expected save
                            var _saveKey = _durable.DurableSaveKey(apply.TargetingProcess.Targets, apply.DeliveryInteraction, _effect.SubMode);
                            if (!_saveKey.Equals(string.Empty))
                            {
                                // get the prerequisite for the expected save, 
                                var _savePre = apply.AllPrerequisites<SavePrerequisite>(_saveKey).FirstOrDefault();

                                // if the save does not negate the effect, or the save is not successful, we will continue to add
                                _add = ((_savePre.SaveMode.SaveEffect != SaveEffect.Negates) || !_savePre.Success);
                            }
                        }

                        // if we are still adding, add the effect
                        if (_add)
                        {
                            _target.AddAdjunct(_effect);
                        }
                    }
                }
            }
        }
        #endregion

        public virtual MagicPowerDefInfo ToMagicPowerDefInfo()
            => this.GetMagicPowerDefInfo();

        public override PowerDefInfo ToPowerDefInfo()
            => ToMagicPowerDefInfo();
    }
}
