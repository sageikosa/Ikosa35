using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Creatures.Templates;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Fidelity;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public abstract class SummonMonsterBase : SpellDef, ISpellMode, IDurableCapable, IDurableAnchorCapable
    {
        public override MagicStyle MagicStyle => new Conjuration(Conjuration.SubConjure.Summoning);

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, true, false);

        public override IEnumerable<SpellComponent> DivineComponents
            => YieldComponents(true, true, false, false, true);

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        public override IEnumerable<Descriptor> Descriptors => _Descriptors;

        /// <summary>GetCreature will set this...</summary>
        protected IList<Descriptor> _Descriptors = [];

        protected abstract IPowerDef NewForPowerSource();

        public override IPowerDef ForPowerSource()
            => NewForPowerSource();

        public abstract IEnumerable<OptionAimOption> GetCreatures(CoreActor actor, SummonMonsterMode monsterMode);
        public abstract Creature GetCreature(CoreActor actor, OptionAimOption option, string prefix, int index);

        protected string GetCreatureName(string name, string prefix, int index)
            => $@"{prefix}{(!string.IsNullOrEmpty(prefix) ? "-" : string.Empty)}{name} {index}";

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            if (mode is SummonMonsterMode _smm)
            {
                yield return new OptionAim(@"Creature.Type", @"Creature type to summon", true, FixedRange.One, FixedRange.One,
                    GetCreatures(actor, _smm));
                yield return new CharacterStringAim(@"Name Prefix", @"Name to help differentiate creatures",
                    FixedRange.One, new FixedRange(16));
                switch (_smm.SubMode)
                {
                    case SummonSubMode.Regular:
                        yield return new LocationAim(@"Location", @"Starting Location", LocationAimMode.Cell,
                            FixedRange.One, FixedRange.One, new NearRange());
                        break;

                    case SummonSubMode.FewLesser:
                        yield return new LocationAim(@"Location", @"Starting Location", LocationAimMode.Cell,
                            FixedRange.One, new FixedRange(_smm.Number), new NearRange());
                        break;

                    case SummonSubMode.SeveralLeast:
                        yield return new LocationAim(@"Location", @"Starting Location", LocationAimMode.Cell,
                            FixedRange.One, new FixedRange(_smm.Number), new NearRange());
                        break;
                }
            }
            yield break;
        }

        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            var _idx = 1;
            foreach (var _location in activation.TargetingProcess.Targets.OfType<LocationTarget>())
            {
                var _geoInteract = new GeometryInteract
                {
                    Index = _idx,
                    ID = Guid.NewGuid(),
                    Point3D = _location.SupplyPoint3D(),
                    Position = _location.Location.ToCellPosition(),
                    AimMode = _location.LocationAimMode
                };
                SpellDef.CarryDurableEffectsToCell(activation, _geoInteract, 0);
                _idx++;
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // favor material plane
            var _tProc = apply.TargetingProcess;
            var _ethereal = !(apply.Actor?.GetLocated()?.Locator.PlanarPresence.HasMaterialPresence() ?? true);
            var _mapContext = (apply.Actor.Setting as LocalMap)?.MapContext;
            var _geomInteract = apply.DeliveryInteraction.Target as GeometryInteract;

            // set up virtual object(s)
            var _prefix = _tProc.Targets.OfType<CharacterStringTarget>()?.FirstOrDefault()?.CharacterString;
            var _critter = GetCreature(apply.Actor, _tProc.Targets.OfType<OptionTarget>()?.FirstOrDefault()?.Option,
                _prefix, _geomInteract.Index);
            var _window = new SummoningWindow(@"summoning_window", _critter);
            var _size = new GeometricSize(_window.GeometricSize);
            var _region = new CubicBuilder(_size, _size.CenterCell(_geomInteract.AimMode == LocationAimMode.Intersection))
                .BuildGeometry(_geomInteract.AimMode, _geomInteract.Position);
            // TODO: larger creature size location positioning, offset/caster clockwise offset
            new Locator(_window, _mapContext, _window.GeometricSize, _region);

            // get delivered effect
            var _effect = apply.DurableMagicEffects.FirstOrDefault();

            // setup summoningGroup
            var _powerSource = apply.PowerUse.PowerActionSource;
            var _summonGroup = new SummoningGroup(_powerSource);

            // track the group in the effect
            _effect.AllTargets.Add(new ValueTarget<SummoningGroup>(nameof(SummoningGroup), _summonGroup));

            // connect summoning group to actor
            apply.Actor?.AddAdjunct(new Summoner(_powerSource, _summonGroup));

            // add magic power effect to summoning window
            _window.AddAdjunct(_effect);
        }

        // IDurableCapable
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();
        public bool IsDismissable(int subMode) => true;

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => string.Empty;

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // enable if possible
            (source.AnchoredAdjunctObject as SummoningWindowLink)?.ActivateAdjunct();
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            // disable if possible
            (source.AnchoredAdjunctObject as SummoningWindowLink)?.DeActivateAdjunct();
        }

        // creature may wink in and out of existence if durable magic effect is suppressed by anti-magic
        public object OnAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            // enable if possible
            if (source is MagicPowerEffect _spellEffect)
            {
                var _summonGroup = _spellEffect.GetTargetValue<SummoningGroup>(nameof(SummoningGroup));
                if (_summonGroup != null)
                {
                    // create effect
                    var _window = new SummoningWindowLink(_spellEffect, _summonGroup) { InitialActive = false };
                    target.AddAdjunct(_window);
                    return _window;
                }
            }
            return null;
        }

        public void OnEndAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            // remove the adjunct group and all members from their anchors
            (source as MagicPowerEffect)?
                .GetTargetValue<SummoningGroup>(nameof(SummoningGroup))?
                .EjectMembers();

            // when the durable spell effect is removed from the virtual object, the object is removed from context
            var _window = target as SummoningWindow;
            (_window.SummonedTarget as Creature)?.UnPath();
            (_window.SummonedTarget as Creature)?.UnGroup();
            _window?.Abandon();
        }

        protected Creature GenerateCreature<SpecType>(CoreActor actor, string name, string prefix, int index, string devotion,
            LawChaosAxis lawChaos, GoodEvilAxis goodEvil, bool celestial, bool fiendish)
            where SpecType : Species, new()
        {
            var _species = new SpecType();
            var _abilities = _species.DefaultAbilities();
            var _critter = new Creature(GetCreatureName(name, prefix, index), _abilities);
            _species.BindTo(_critter);
            _critter.Devotion = new Devotion(devotion);
            _critter.AddAdjunct(new AlignedCreature(new Alignment(lawChaos, goodEvil)));

            // just ensure no conflicts
            if (celestial && goodEvil != GoodEvilAxis.Evil)
            {
                _critter.AddAdjunct(new Celestial());
            }

            if (fiendish && goodEvil != GoodEvilAxis.Good)
            {
                _critter.AddAdjunct(new Fiendish());
            }

            // summoned monster is in the same teams as the caster
            foreach (var _t in actor.Adjuncts.OfType<TeamMember>())
            {
                TeamMember.SetTeamMember(_critter, _t.TeamGroup.Name);
            }

            return _critter;
        }
    }

    [Serializable]
    public class SummonMonsterMode : ISpellMode
    {
        private readonly SummonMonsterBase _Summon;
        private readonly SummonSubMode _SubMode;
        private readonly int _Number;

        public SummonMonsterMode(SummonMonsterBase summonMonster, SummonSubMode subMode)
        {
            _Summon = summonMonster;
            _SubMode = subMode;
            _Number = subMode switch
            {
                SummonSubMode.FewLesser => Math.Max(2, DiceRoller.RollDie(null, 3, @"Summon Monster (Few Lesser)", @"Number")),
                SummonSubMode.SeveralLeast => Math.Max(3, new ComplexDiceRoller(@"1d4+1").RollValue(null, @"Summon Monster (Several Least)", @"Number")),
                _ => 1,
            };
        }

        public string DisplayName => $@"{_Summon.DisplayName} ({SubMode})";
        public string Description => $@"{_Summon.Description} ({SubMode})";
        public bool AllowsSpellResistance => _Summon.AllowsSpellResistance;
        public bool IsHarmless => _Summon.IsHarmless;

        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
            => _Summon.ActivateSpell(activation);

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => _Summon.AimingMode(actor, mode);

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
            => _Summon.ApplySpell(apply);

        Capability ICapabilityRoot.GetCapability<Capability>()
            => _Summon.GetCapability<Capability>();

        // specific information about SummonMonster
        public SummonSubMode SubMode => _SubMode;
        public int Number => _Number;
    }

    [Serializable]
    public enum SummonSubMode : byte
    {
        Regular,
        FewLesser,
        SeveralLeast
    }
}
