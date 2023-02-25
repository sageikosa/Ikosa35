using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public static class AimTargetingFactory
    {
        public static AimTargetingBase GetAimTargeting(AimingModeInfo aimingMode, ActivityInfoBuilder builder)
        {
            if (aimingMode is OptionAimInfo _optInfo)
            {
                return new OptionTargeting(builder, _optInfo);
            }
            else if (aimingMode is AwarenessAimInfo _awareInfo)
            {
                return new AwarenessTargeting(builder, _awareInfo);
            }
            else if (aimingMode is AttackAimInfo _atkInfo)
            {
                return new AttackTargeting(builder, _atkInfo);
            }
            else if (aimingMode is LocationAimInfo _locInfo)
            {
                // TODO: needs some more stuff also?
                return new LocationTargeting(builder, _locInfo);
            }
            else if (aimingMode is VolumeAimInfo)
            {
                // TODO: new volume targetting...should provide CubicTargetInfo...
            }
            else if (aimingMode is QuantitySelectAimInfo _quantInfo)
            {
                return new QuantitySelectTargeting(builder, _quantInfo);
            }
            else if (aimingMode is CoreListAimInfo _coreListInfo)
            {
                return new CoreInfoListTargeting(builder, _coreListInfo);
            }
            else if (aimingMode is ObjectListAimInfo _objListInfo)
            {
                return new ObjectListTargeting(builder, _objListInfo);
            }
            else if (aimingMode is CharacterStringAimInfo _charInfo)
            {
                return new CharacterStringTargeting(builder, _charInfo);
            }
            else if (aimingMode is WordAimInfo _wordInfo)
            {
                return new WordTargeting(builder, _wordInfo);
            }
            else if (aimingMode is RollAimInfo _rollInfo)
            {
                return new RollTargeting(builder, _rollInfo);
            }
            else if (aimingMode is SuccessCheckAimInfo _successInfo)
            {
                return new SuccessCheckTargeting(builder, _successInfo);
            }
            else if (aimingMode is PersonalAimInfo _persInfo)
            {
                // all personal aim derivatives use the same targeting system ...
                // ... remaining geometric information derived from service using SensorHost/Locator information
                return new PersonalAimTargeting(builder, _persInfo);
            }
            else if (aimingMode is PrepareSpellSlotsAimInfo _prepSpellInfo)
            {
                return new PrepareSpellSlotsTargeting(builder, _prepSpellInfo);
            }
            return null;
        }
    }
}
