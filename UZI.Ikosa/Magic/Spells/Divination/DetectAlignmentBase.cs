using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Actions;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Time;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public abstract class DetectAlignmentBase : DetectBase<DetectAlignmentAdjunct>
    {
        public override DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1));

        #region public override IEnumerable<SpellComponent> DivineComponents { get; }
        public override IEnumerable<SpellComponent> DivineComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new DivineFocusComponent();
                yield break;
            }
        }
        #endregion

        protected abstract Alignment GetFilter();

        protected override DetectAlignmentAdjunct NewEffect(MagicPowerEffect source, IAdjunctable target)
        {
            foreach (var _range in source.CapabilityRoot.GetCapability<IRegionCapable>().Dimensions(null, source.CasterLevel))
            {
                return new DetectAlignmentAdjunct(source.MagicPowerActionSource, _range, GetFilter());
            }

            return new DetectAlignmentAdjunct(source.MagicPowerActionSource, 60, GetFilter());
        }
    }

    [Serializable]
    public class DetectAlignmentAdjunct : DetectAuraAdjunctBase, IDetectExtraInfo
    {
        #region Construction
        public DetectAlignmentAdjunct(INamedActionSource source, double range, Alignment filter)
            : base(source, range)
        {
            _Filter = filter;
        }
        #endregion

        private Alignment _Filter;
        public Alignment Filter => _Filter;

        public override void ValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args)
        {
            // reset counter
            base.ValueChanged(sender, args);

            // cancel divination markers (since geometry changed)
            try { (Anchor as Creature).ExtraInfoMarkers.RemoveSource(ID); }
            catch { }
        }

        protected override void OnDeactivate(object source)
        {
            // cancel divination markers
            try { (Anchor as Creature).ExtraInfoMarkers.RemoveSource(ID); }
            catch { }

            // remove action provider and geometric change hook
            base.OnDeactivate(source);
        }

        #region Actions to Emit
        protected override IEnumerable<ActionBase> PrimeActions(LocalActionBudget budget)
        {
            if (budget.CanPerformRegular)
            {
                var _fStr = Filter.NoNeutralString();
                yield return new DetectPresenceAction<DetectAlignmentAdjunct>($@"Detect.{_fStr}.1", 
                    $@"Detect {_fStr} Presence", this, @"101");
            }
            yield break;
        }

        protected override IEnumerable<ActionBase> SecondActions(LocalActionBudget budget)
        {
            if (budget.CanPerformRegular)
            {
                var _fStr = Filter.NoNeutralString();
                yield return new DetectNumberAuraAction<DetectAlignmentAdjunct>($@"Detect.{_fStr}.2", 
                    $@"Detect Number of {_fStr} Auras", this, @"102");
            }
            yield break;
        }

        protected override IEnumerable<ActionBase> ThirdActions(LocalActionBudget budget)
        {
            if (budget.CanPerformRegular)
            {
                var _fStr = Filter.NoNeutralString();
                yield return new DetectDivinationAction<DetectAlignmentAdjunct>($@"Detect.{_fStr}.3",
                    $@"Detect Direction and Strength of {_fStr} Auras", this, @"103");
            }
            yield break;
        }
        #endregion

        #region public override int CountAuras(Locator testLocator)
        public override int CountAuras(Locator testLocator)
        {
            var _count = 0;
            foreach (var _obj in testLocator.AllConnectedOf<CoreObject>())
            {
                // TODO: look for blocker
                _count += (from _a in _obj.Adjuncts
                           where _a.IsActive
                           let _al = _a as IAlignmentAura
                           where (_al != null) && _al.Alignment.DetectsAs(_Filter)
                           && (_al.AlignmentStrength > AuraStrength.None)
                           select _al).Count();
            }
            return _count;
        }
        #endregion

        #region public override IAura GetStrongestAura(Locator testLocator)
        public override IAura GetStrongestAura(Locator testLocator)
        {
            var _myAlign = Anchor.GetAlignment();
            var _stunLevel = 0;
            var _aCritter = Anchor as Creature;
            if (_aCritter != null)
            {
                _stunLevel = _aCritter.Classes.CharacterLevel * 2;
            }

            var _strong = AuraStrength.None;
            IAlignmentAura _aura = null;
            foreach (var _obj in testLocator.AllConnectedOf<CoreObject>())
            {
                // TODO: look for blocker
                foreach (var _aligned in from _a in _obj.Adjuncts
                                         where _a.IsActive
                                         let _al = _a as IAlignmentAura
                                         where (_al != null) && _al.Alignment.DetectsAs(_Filter)
                                         select _al)
                {
                    if (_aligned.AlignmentStrength >= _strong)
                    {
                        // capture new strongest
                        if (_aligned.AlignmentStrength > _strong)
                        {
                            _strong = _aligned.AlignmentStrength;
                            _aura = _aligned;
                        }

                        // if overwhelming, see if we get stunned (look at all)
                        if (_aura.AlignmentStrength == AuraStrength.Overwhelming)
                        {
                            if (_aligned.Alignment.Opposable(_myAlign) && !_myAlign.IsNeutral)
                            {
                                if (_aligned.PowerLevel >= _stunLevel)
                                {
                                    var _stunned = new StunnedEffect(_aligned, _aCritter.GetLocated().Locator.Map.CurrentTime + Round.UnitFactor, Round.UnitFactor);
                                    Anchor.AddAdjunct(_stunned);
                                    if (Source is MagicPowerEffect _spell)
                                    {
                                        _spell.Anchor.RemoveAdjunct(_spell);
                                        if (_aura.AlignmentStrength != _aura.AuraStrength)
                                        {
                                            var _newAura = new Aura(_aura.ID, _aura.AlignmentStrength);
                                            return _newAura;
                                        }
                                        return _aura;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (_aura.AlignmentStrength != _aura.AuraStrength)
            {
                var _newAura = new Aura(_aura.ID, _aura.AlignmentStrength);
                return _newAura;
            }
            return _aura;
        }
        #endregion

        #region public override bool DoesLocatorMatch(Locator testLocator)
        public override bool DoesLocatorMatch(Locator testLocator)
        {
            foreach (var _obj in testLocator.AllConnectedOf<CoreObject>())
            {
                // TODO: look for blocker
                foreach (var _adj in _obj.Adjuncts.Where(_a => _a.IsActive))
                {
                    if ((_adj is IAlignmentAura _aligned)
                        && _aligned.Alignment.DetectsAs(_Filter)
                        && (_aligned.AlignmentStrength > AuraStrength.None))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region IDetectExtraInfo Members
        public IEnumerable<ExtraInfo> GetExtraInformation(CoreActivity activity, Locator testLocator)
        {
            var _critter = Anchor as Creature;
            foreach (var _obj in testLocator.AllConnectedOf<CoreObject>())
            {
                // TODO: look for blocker
                foreach (var _adj in _obj.Adjuncts.Where(_a => _a.IsActive))
                {
                    if ((_adj is IAlignmentAura _align) 
                        && _align.Alignment.DetectsAs(_Filter))
                    {
                        // basic description
                        var _descr = new Description(@"Strength", _align.AlignmentStrength.ToString());
                        var _info = new Informable(_descr);

                        // link it to the environment
                        var _marker = new ExtraInfoMarker(GetExtraInfoSource(), _info,
                            testLocator.GeometricRegion, true, _align.AlignmentStrength, null);
                        yield return _marker;
                    }
                }
            }
            yield break;
        }
        #endregion

        public override Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo($@"Detect {Filter.NoNeutralString()}", ID);

        public override object Clone()
            => new DetectAlignmentAdjunct(Source as INamedActionSource, Range, Filter);
    }
}
