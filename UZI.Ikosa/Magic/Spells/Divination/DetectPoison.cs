using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class DetectPoison : SpellDef, ISpellMode
    {
        public override string DisplayName => @"Detect Poison";
        public override string Description => @"Determine whether creature, object or area is poisoned or poisonous";
        public override MagicStyle MagicStyle => new Divination(Divination.SubDivination.Detection);

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<ISpellMode> SpellModes { get; }
        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                yield return this;
                yield return new DetectPoisonEnvironmentMode();
                yield break;
            }
        }
        #endregion

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Target", @"Target", FixedRange.One, FixedRange.One, new NearRange(), new TargetType[] { new ObjectTargetType(), new CreatureTargetType() });
            yield return new RollAim(@"Alchemy", @"Craft Alchemy Check Roll", new DieRoller(20));
            yield return new RollAim(@"Wisdom", @"Wisdom Check Roll", new DieRoller(20));
            yield break;
        }

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            // TODO: requires line of detect...
            activation.AppendFollowing(new PowerApplyStep<SpellSource>(activation, activation.PowerUse, activation.Actor,
                null, InteractPowerTransit(activation, 0, activation.TargetingProcess.Targets[0]), false, false));
        }

        internal static void AddCheck(Creature critter, Collection<Info> descriptions, Poison poison, int wisdom, int alchemy)
        {
            // auto-check
            if (critter.Skills.Skill<CraftSkill<CraftAlchemy>>().SkillCheck(20, poison, alchemy)
                || critter.Abilities.Wisdom.AbilityCheck(critter, 20, poison, wisdom, Deltable.GetDeltaCalcNotify(critter.ID, @"Poison Check (Wisdom)").DeltaCalc))
            {
                descriptions.Add(
                    new Description(poison.OriginalName, new string[]
                    {
                        $@"Primary: {poison.PrimaryDamage.Name}",
                        $@"Secondary: {poison.SecondaryDamage.Name}",
                        $@"Difficulty: {poison.Difficulty.EffectiveValue}",
                        $@"Activation: {poison.Activation.ToString()}"
                    }));
            }
        }

        #region public void ApplySpell(PowerApplyStep<SpellSource> apply)
        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // identification checks
            int _checkVal(string key)
                => apply.TargetingProcess.GetFirstTarget<ValueTarget<int>>(key)?.Value ?? 10;

            var _wisCheck = _checkVal(@"Wisdom");
            var _alcCheck = _checkVal(@"Alchemy");

            if ((apply.DeliveryInteraction.Target is CoreObject _obj)
                && (apply.Actor is Creature _critter))
            {
                var _descriptions = new Collection<Info>();

                // is poison?
                var _poison = _obj as Poison;
                if (_poison != null)
                {
                    AddCheck(_critter, _descriptions, _poison, _wisCheck, _alcCheck);
                }

                // main target poisoned or poisonous?
                foreach (var _effect in from _eff in _obj.Adjuncts
                                        let _et = _eff.GetType()
                                        where typeof(Poisoned).IsAssignableFrom(_et) || typeof(Poisonous).IsAssignableFrom(_et)
                                        select _eff)
                {
                    if (_effect is Poisoned)
                        _poison = (_effect as Poisoned).Poison;
                    else
                        _poison = (_effect as Poisonous).Poison;
                    AddCheck(_critter, _descriptions, _poison, _wisCheck, _alcCheck);
                }

                // any parts of the target poison, poisoned or poisonous?
                foreach (var _part in from _sub in _obj.AllConnected(null)
                                      where typeof(CoreObject).IsAssignableFrom(_sub.GetType())
                                      select _sub as CoreObject)
                {
                    // poison?
                    _poison = _poison as Poison;
                    if (_poison != null)
                    {
                        AddCheck(_critter, _descriptions, _poison, _wisCheck, _alcCheck);
                    }

                    // poisoned or poisonous?
                    foreach (var _effect in from _eff in _part.Adjuncts
                                            let _et = _eff.GetType()
                                            where typeof(Poisoned).IsAssignableFrom(_et) || typeof(Poisonous).IsAssignableFrom(_et)
                                            select _eff)
                    {
                        if (_effect is Poisoned)
                            _poison = (_effect as Poisoned).Poison;
                        else
                            _poison = (_effect as Poisonous).Poison;
                        AddCheck(_critter, _descriptions, _poison, _wisCheck, _alcCheck);
                    }
                }

                if (apply.TargetingProcess is CoreActivity _activity)
                {
                    apply.AppendFollowing(_activity.GetActivityResultNotifyStep(_descriptions.ToArray()));
                }
            }
        }
        #endregion
        #endregion
    }

    [Serializable]
    public class DetectPoisonEnvironmentMode : ISpellMode
    {
        public string DisplayName => @"Detect Poison (environment)";
        public string Description => @"Determine whether area is poisonous";

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new EnvironmentAim(@"Cube", @"Cube", FixedRange.One, FixedRange.One, new NearRange(), FixedRange.One);
            yield return new RollAim(@"Alchemy", @"Craft Alchemy Check Roll", new DieRoller(20));
            yield return new RollAim(@"Wisdom", @"Wisdom Check Roll", new DieRoller(20));
            yield break;
        }

        #region private static IEnumerable<OptionAimOption> Take10Options()
        private static IEnumerable<OptionAimOption> Take10Options()
        {
            yield return new OptionAimOption() { Key = @"Yes", Name = @"Take 10", Description = @"Take 10 for this check" };
            yield return new OptionAimOption() { Key = @"No", Name = @"Roll 1d20", Description = @"Roll 1d20 for this check" };
            yield break;
        }
        #endregion

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        #region public void Deliver(PowerDeliveryStep<SpellSource> deliver)
        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            if (deliver.TargetingProcess.Targets[0] is GeometricRegionTarget _cubicTarget)
            {
                // TODO: requires line of detect...
                var _delivery = SpellDef.InteractSpellTransitToRegion(deliver, _cubicTarget);
                deliver.AppendFollowing(new PowerApplyStep<SpellSource>(deliver, deliver.PowerUse, deliver.Actor,
                    null, _delivery, false, false));
            }
        }
        #endregion

        #region public void ApplySpell(PowerApplyStep<SpellSource> apply)
        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // identification checks
            int _checkVal(string key)
            {
                var _check = apply.TargetingProcess.GetFirstTarget<ValueTarget<int>>(key);
                if (_check != null)
                    return _check.Value;
                return 10;
            }
            var _wisCheck = _checkVal(@"Wisdom");
            var _alcCheck = _checkVal(@"Alchemy");

            if (apply.Actor is Creature _critter)
            {
                // get ready to build results
                var _planar = _critter?.GetPlanarPresence() ?? Tactical.PlanarPresence.None;
                var _descriptions = new Collection<Info>();

                // get volume picked to examine
                if (apply.TargetingProcess.Targets.Where(_t => _t.Key == @"Cube").First() is GeometricRegionTarget _cubic)
                {
                    // find all poisonous locator captures containing the cube
                    foreach (var _catch in from _zone in _cubic.MapContext.LocatorZones.AllCaptures()
                                           where _zone.ContainsCell(_cubic.Region.AllCellLocations().FirstOrDefault(), null, _planar)
                                           select _zone)
                    {
                        if (_catch.Source is CoreObject _src)
                        {
                            if (Poisonous.IsPoisonous(_src))
                            {
                                DetectPoison.AddCheck(_critter, _descriptions, Poisonous.GetPoison(_src), _wisCheck, _alcCheck);
                            }
                        }
                    }
                }


                if (apply.TargetingProcess is CoreActivity _activity)
                {
                    apply.AppendFollowing(_activity.GetActivityResultNotifyStep(_descriptions.ToArray()));
                }
            }
        }
        #endregion

        public virtual IMode GetCapability<IMode>() where IMode : class, ICapability
            => this as IMode;
    }
}
