using System;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Fidelity;
using System.CodeDom;

namespace Uzi.Ikosa.Universal
{
    public static class DefaultSpellListInitializer
    {
        public static ClassSpellList SorcererSpells()
        {
            var _classSpellList = new ClassSpellList
            {
                // NEEDS: Prestidigitation, Message, Mending, Ghost Sound, Dancing Lights
                {
                    0,
                    GetClassSpellLevel(0, typeof(AcidSplash), typeof(Resistance), typeof(DetectPoison),
                    typeof(DetectMagic), typeof(ReadMagic), typeof(Daze), typeof(Flare), typeof(Light),
                    typeof(RayOfFrost), typeof(DisruptUndead), typeof(TouchOfFatigue), typeof(OpenClose),
                    typeof(ArcaneMark))
                },
                // NEEDS: Endure Elements, Mount, Obscuring Mist, Summon Monster, Unseen Servant, Floating Disc
                //        Disguise Self, Silent Image, Ventriliquism, Animate Rope, Erase
                {
                    1,
                    GetClassSpellLevel(1, typeof(ZoneOfAlarm), typeof(HoldShut), typeof(ProtectionAgainstChaos),
                    typeof(ProtectionAgainstEvil), typeof(ProtectionAgainstGood), typeof(ProtectionAgainstLaw),
                    typeof(Shield), typeof(SlipperySurface), typeof(MageArmor), typeof(ComprehendLanguages),
                    typeof(DetectSecretDoors), typeof(DetectUndead), typeof(TrueStrike), typeof(CharmPerson),
                    typeof(Hypnotism), typeof(Sleep), typeof(BurningHands), typeof(MagicForceMissile),
                    typeof(ShockingTouch), typeof(ColorSpray), typeof(MagicalAura), typeof(CauseFear),
                    typeof(ChillTouch), typeof(RayOfEnfeeblement), typeof(EnlargePerson),
                    typeof(ExpeditiousRetreat), typeof(Jump), typeof(MagicWeapon), typeof(ReducePerson),
                    typeof(Identification), typeof(FeatherFall))
                },
                // NEEDS: *many*!
                {
                    2,
                    GetClassSpellLevel(2, typeof(PermanentTorch), typeof(ResistEnergy), typeof(Darkness),
                    typeof(BullStrength), typeof(BearEndurance), typeof(InvisibilitySpell),
                    typeof(CatGrace), typeof(EagleSplendor), typeof(FoxCunning), typeof(OwlWisdom),
                    typeof(DazeMonster), typeof(DarkvisionSpell), typeof(ScorchingRay), typeof(FalseLife),
                    typeof(BlindnessDeafness), typeof(Scare), typeof(Blur), typeof(ProtectionFromArrows),
                    typeof(AcidArrow), typeof(ArcaneLock), typeof(Knock), typeof(SpiderClimb),
                    typeof(TouchOfIdiocy), typeof(SeeInvisibility), typeof(FlamingSphere), typeof(HypnoticPattern),
                    typeof(HideousLaughter))
                },
                {
                    3,
                    GetClassSpellLevel(3, typeof(Daylight), typeof(DeepSlumber), typeof(Displacement),
                    typeof(FlameArrow), typeof(Fly), typeof(Haste), typeof(Heroism), typeof(HoldPerson),
                    typeof(KeenEdge), typeof(LightningBolt), typeof(MagicWeaponGreater),
                    typeof(ProtectionFromEnergy), typeof(Rage), typeof(RayOfExhaustion), 
                    typeof(RemoteSensing), typeof(Slow)
                    )
                },
                {
                    4,
                    GetClassSpellLevel(4, typeof(BestowCurse), typeof(RemoveCurse))
                }
            };
            return _classSpellList;
        }

        public static ClassSpellList WizardSpells()
        {
            var _classSpellList = new ClassSpellList
            {
                // NEEDS: Prestidigitation, Message, Mending, Ghost Sound, Dancing Lights
                {
                    0,
                    GetClassSpellLevel(0, typeof(AcidSplash), typeof(Resistance), typeof(DetectPoison),
                    typeof(DetectMagic), typeof(ReadMagic), typeof(Daze), typeof(Flare), typeof(Light),
                    typeof(RayOfFrost), typeof(DisruptUndead), typeof(TouchOfFatigue), typeof(OpenClose),
                    typeof(ArcaneMark))
                },
                // NEEDS: Endure Elements, Mount, Obscuring Mist, Summon Monster, Unseen Servant, Floating Disc
                //        Disguise Self, Silent Image, Ventriliquism, Animate Rope, Erase
                {
                    1,
                    GetClassSpellLevel(1, typeof(ZoneOfAlarm), typeof(HoldShut), typeof(ProtectionAgainstChaos),
                    typeof(ProtectionAgainstEvil), typeof(ProtectionAgainstGood), typeof(ProtectionAgainstLaw),
                    typeof(Shield), typeof(SlipperySurface), typeof(MageArmor), typeof(ComprehendLanguages),
                    typeof(DetectSecretDoors), typeof(DetectUndead), typeof(TrueStrike), typeof(CharmPerson),
                    typeof(Hypnotism), typeof(Sleep), typeof(BurningHands), typeof(MagicForceMissile),
                    typeof(ShockingTouch), typeof(ColorSpray), typeof(MagicalAura), typeof(CauseFear),
                    typeof(ChillTouch), typeof(RayOfEnfeeblement), typeof(EnlargePerson),
                    typeof(ExpeditiousRetreat), typeof(Jump), typeof(MagicWeapon), typeof(ReducePerson),
                    typeof(Identification), typeof(FeatherFall))
                },
                // NEEDS: *many*!
                {
                    2,
                    GetClassSpellLevel(2, typeof(PermanentTorch), typeof(ResistEnergy), typeof(Darkness),
                    typeof(BullStrength), typeof(BearEndurance), typeof(InvisibilitySpell),
                    typeof(CatGrace), typeof(EagleSplendor), typeof(FoxCunning), typeof(OwlWisdom),
                    typeof(DazeMonster), typeof(DarkvisionSpell), typeof(ScorchingRay), typeof(FalseLife),
                    typeof(BlindnessDeafness), typeof(Scare), typeof(Blur), typeof(ProtectionFromArrows),
                    typeof(AcidArrow), typeof(ArcaneLock), typeof(Knock), typeof(SpiderClimb),
                    typeof(TouchOfIdiocy), typeof(SeeInvisibility), typeof(FlamingSphere), typeof(HypnoticPattern),
                    typeof(HideousLaughter))
                },
                {
                    3,
                    GetClassSpellLevel(3, typeof(Daylight), typeof(DeepSlumber), typeof(Displacement),
                    typeof(FlameArrow), typeof(Fly), typeof(Haste), typeof(Heroism), typeof(HoldPerson),
                    typeof(KeenEdge), typeof(LightningBolt), typeof(MagicWeaponGreater),
                    typeof(ProtectionFromEnergy), typeof(Rage), typeof(RayOfExhaustion),
                    typeof(RemoteSensing), typeof(Slow)
                    )
                },
                {
                    4,
                    GetClassSpellLevel(4, typeof(BestowCurse), typeof(RemoveCurse))
                }
            };
            return _classSpellList;
        }

        public static ClassSpellList ClericSpells()
        {
            var _classSpellList = new ClassSpellList
            {
                // NEEDS: Create Water, Guidance, Mending, Purify Food and Drink
                {
                    0,
                    GetClassSpellLevel(0, typeof(CureMinorWounds), typeof(DetectMagic),
                    typeof(DetectPoison), typeof(InflictMinorWounds), typeof(Light),
                    typeof(ReadMagic), typeof(Resistance), typeof(Virtue))
                },
                // NEEDS: Endure Elements, Magic Stone, Obscuring Mist, Summon Monster
                {
                    1,
                    GetClassSpellLevel(1, typeof(AgitateWater), typeof(Bane), typeof(Bless), 
                    typeof(CauseFear), typeof(Command), typeof(ComprehendLanguages), typeof(CureLightWounds), 
                    typeof(SanctifyWater), typeof(DescrateWater), typeof(OrchestrateWater), 
                    typeof(DetectDying), typeof(DetectChaos),
                    typeof(DetectEvil), typeof(DetectGood), typeof(DetectLaw),
                    typeof(DetectUndead), typeof(DivineLuck), typeof(Doom),
                    typeof(EntropicShield), typeof(UnknownToUndead), typeof(InflictLightWounds),
                    typeof(MagicWeapon), typeof(ProtectionAgainstChaos), typeof(RemoveFear),
                    typeof(ProtectionAgainstEvil), typeof(ProtectionAgainstGood),
                    typeof(ProtectionAgainstLaw), typeof(TouchOfSanctuary), typeof(ShieldOfGrace))
                },
                // NEEDS: *many*!
                {
                    2,
                    GetClassSpellLevel(2, typeof(AidSpell), typeof(CureModerateWounds),
                    typeof(InflictModerateWounds), typeof(ResistEnergy), typeof(BullStrength),
                    typeof(BearEndurance), typeof(EagleSplendor), typeof(OwlWisdom),
                    typeof(Darkness), typeof(AlignWeapon), typeof(DelayPoison),
                    typeof(RemoveParalysis), typeof(RestorationLesser), typeof(HoldPerson),
                    typeof(Silence), typeof(SpiritualWeapon))
                },
                // NEEDS: *many*!
                {
                    3,
                    GetClassSpellLevel(3, typeof(BestowCurse), typeof(BlindnessDeafness),
                    typeof(CureSeriousWounds), typeof(Daylight), typeof(DeeperDarkness), 
                    typeof(MagicVestment), typeof(PermanentTorch), typeof(Prayer), 
                    typeof(ProtectionFromEnergy), typeof(RemoveCurse), typeof(SearingLight))
                },
                // NEEDS: *many*!
                {
                    4,
                    GetClassSpellLevel(4, typeof(CureCriticalWounds), typeof(MagicWeaponGreater))
                }
            };
            return _classSpellList;
        }

        public static ClassSpellList AdeptSpells()
        {
            var _classSpellList = new ClassSpellList
            {
                // NEEDS: Create Water, GhostSound, Guidance, Mending, Purify Food and Drink
                {
                    0,
                    GetClassSpellLevel(0, typeof(CureMinorWounds), typeof(DetectMagic), typeof(Light),
                    typeof(ReadMagic), typeof(TouchOfFatigue))
                },
                // NEEDS: Endure Elements, Obscuring Mist
                {
                    1,
                    GetClassSpellLevel(1, typeof(Bless), typeof(BurningHands), typeof(CauseFear),
                    typeof(Command), typeof(ComprehendLanguages), typeof(CureLightWounds), typeof(DetectChaos),
                    typeof(DetectEvil), typeof(DetectGood), typeof(DetectLaw), typeof(ProtectionAgainstChaos),
                    typeof(ProtectionAgainstEvil), typeof(ProtectionAgainstGood), typeof(ProtectionAgainstLaw),
                    typeof(Sleep))
                },
                // NEEDS: animal trance, mirror image, web
                {
                    2,
                    GetClassSpellLevel(2, typeof(AidSpell), typeof(BearEndurance), typeof(BullStrength),
                    typeof(CatGrace), typeof(CureModerateWounds),typeof(Darkness), typeof(DelayPoison),
                    typeof(InvisibilitySpell), typeof(ResistEnergy), typeof(ScorchingRay), typeof(SeeInvisibility))
                }
            };
            return _classSpellList;
        }

        private static ClassSpellLevel GetClassSpellLevel(int level, params Type[] spellDefTypes)
        {
            var _spellLevel = new ClassSpellLevel(level);
            foreach (var _defType in spellDefTypes)
            {
                var _def = Activator.CreateInstance(_defType) as SpellDef;
                _spellLevel.Add(new ClassSpell(level, _def));
            }
            return _spellLevel;
        }

        public static ClassSpellList Influence(Type influenceType)
        {
            if (influenceType == typeof(ChaosInfluence))
            {
                return GetInfluenceSpellList(typeof(ProtectionAgainstLaw));
            }
            //else if (influenceName.Equals(@"Death"))
            //{
            //    return GetInfluenceSpellList(typeof(CauseFear));
            //}
            //else if (influenceName.Equals(@"Destruction"))
            //{
            //    return GetInfluenceSpellList(typeof(InflictLightWounds));
            //}
            else if (influenceType == typeof(EvilInfluence))
            {
                return GetInfluenceSpellList(typeof(ProtectionAgainstGood));
            }
            else if (influenceType == typeof(FireInfluence))
            {
                return GetInfluenceSpellList(typeof(BurningHands));
            }
            else if (influenceType == typeof(GoodInfluence))
            {
                return GetInfluenceSpellList(typeof(ProtectionAgainstEvil));
            }
            else if (influenceType == typeof(HealingInfluence))
            {
                return GetInfluenceSpellList(typeof(CureLightWounds), typeof(CureModerateWounds),
                    typeof(CureSeriousWounds), typeof(CureCriticalWounds));
            }
            else if (influenceType == typeof(DivinationInfluence))
            {
                return GetInfluenceSpellList(typeof(DetectSecretDoors));
            }
            else if (influenceType == typeof(LawInfluence))
            {
                return GetInfluenceSpellList(typeof(ProtectionAgainstChaos));
            }
            //else if (influenceType==typeof(LuckInfluence))
            //{
            //    return GetInfluenceSpellList(typeof(EntropicShield));
            //}
            //else if (influenceType==typeof(NobilityInfluence))
            //{
            //    return GetInfluenceSpellList(typeof(DivineFavor), typeof(Enthrall), typeof(MagicVestment));
            //}
            //else if (influenceType==typeof(MagicInfluence))
            //{
            //    return GetInfluenceSpellList(typeof(MagicalAura));
            //}
            else if (influenceType == typeof(ProtectionInfluence))
            {
                return GetInfluenceSpellList(typeof(TouchOfSanctuary));
            }
            else if (influenceType == typeof(StrengthInfluence))
            {
                return GetInfluenceSpellList(typeof(EnlargePerson), typeof(BullStrength), typeof(MagicVestment));
            }
            else if (influenceType == typeof(WarInfluence))
            {
                return GetInfluenceSpellList(typeof(MagicWeapon), typeof(SpiritualWeapon), typeof(MagicVestment));
            }
            return GetInfluenceSpellList();
        }

        private static ClassSpellList GetInfluenceSpellList(params Type[] spellDefTypes)
        {
            var _classSpellList = new ClassSpellList();
            var _level = 0;
            foreach (var _defType in spellDefTypes)
            {
                _level++;
                if (_defType != null)
                {
                    var _spellLevel = new ClassSpellLevel(_level);
                    var _def = Activator.CreateInstance(_defType) as SpellDef;
                    _spellLevel.Add(new ClassSpell(_level, _def));
                    _classSpellList.Add(_level, _spellLevel);
                }
            }
            return _classSpellList;
        }
    }
}
