using System;
using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    public static class IkosaServiceInfos
    {
        public static IEnumerable<Type> InfoTypes()
        {
            // from CORE
            yield return typeof(DeltableInfo);
            yield return typeof(Description);
            yield return typeof(Info);
            yield return typeof(CoreInfo);

            // from IKOSA
            yield return typeof(AdjunctInfo);
            yield return typeof(AmmoInfo);
            yield return typeof(ArmorInfo);
            yield return typeof(BodyInfo);
            yield return typeof(CasterClassInfo);
            yield return typeof(CreatureObjectInfo);
            yield return typeof(DoubleMeleeWeaponInfo);
            yield return typeof(FeatInfo);
            yield return typeof(FlightMovementInfo);
            yield return typeof(ItemSlotInfo);
            yield return typeof(LoadableProjectileWeaponInfo);
            yield return typeof(MountSlotInfo);
            yield return typeof(MagicPowerDefInfo);
            yield return typeof(MagicPowerSourceInfo);
            yield return typeof(MeleeWeaponInfo);
            yield return typeof(MetaMagicInfo);
            yield return typeof(MovementInfo);
            yield return typeof(NaturalWeaponInfo);
            yield return typeof(ObjectInfo);
            yield return typeof(PossessionInfo);
            yield return typeof(PowerClassInfo);
            yield return typeof(PowerDefInfo);
            yield return typeof(ProjectileWeaponInfo);
            yield return typeof(ProtectorInfo);
            yield return typeof(ShieldInfo);
            yield return typeof(SkillInfo);
            yield return typeof(SpellDefInfo);
            yield return typeof(SpellSourceInfo);
            yield return typeof(SpellTriggerInfo);
            yield return typeof(TradeOfferInfo);
            yield return typeof(TimelineActionProviderInfo);
            yield return typeof(WeaponHeadInfo);

            yield break;
        }
    }
}
