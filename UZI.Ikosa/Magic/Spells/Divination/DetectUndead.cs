using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Time;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class DetectUndead : DetectBase<DetectUndeadEffect>
    {
        protected override DetectUndeadEffect NewEffect(MagicPowerEffect source, IAdjunctable target)
        {
            foreach (var _range in source.CapabilityRoot.GetCapability<IRegionCapable>().Dimensions(null, source.CasterLevel))
                return new DetectUndeadEffect(source.MagicPowerActionSource, _range);
            return new DetectUndeadEffect(source.MagicPowerActionSource, 60);
        }

        public override DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));

        public override string DisplayName => @"Detect Undead";
        public override string Description => @"Presence, strength and location of undead auras";
    }

    [Serializable]
    public class DetectUndeadEffect : DetectAuraAdjunctBase, IDetectExtraInfo
    {
        public DetectUndeadEffect(INamedActionSource source, double range)
            : base(source, range)
        {
        }

        #region public override bool DoesLocatorMatch(Locator testLocator)
        public override bool DoesLocatorMatch(Locator testLocator)
            => testLocator.AllConnectedOf<CoreObject>()
                .Any(_obj => _obj.Adjuncts.Where(_a => _a.IsActive).OfType<IUndeadAura>().Any());
        #endregion

        #region Actions to Emit
        protected override IEnumerable<ActionBase> PrimeActions(LocalActionBudget budget)
        {
            if (budget.CanPerformRegular)
                yield return new DetectPresenceAction<DetectUndeadEffect>(@"Detect.Undead.1", @"Detect Undead Presence", this, @"101");
            yield break;
        }

        protected override IEnumerable<ActionBase> SecondActions(LocalActionBudget budget)
        {
            if (budget.CanPerformRegular)
                yield return new DetectNumberAuraAction<DetectUndeadEffect>(@"Detect.Undead.2", @"Detect Number of Undead Auras", this, @"102");
            yield break;
        }

        protected override IEnumerable<ActionBase> ThirdActions(LocalActionBudget budget)
        {
            if (budget.CanPerformRegular)
                yield return new DetectDivinationAction<DetectUndeadEffect>(@"Detect.Undead.3", @"Detect Location and Strength of Undead Auras", this, @"103");
            yield break;
        }
        #endregion

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

        public override Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Detect Undead", ID);

        #region public override int CountAuras(Locator testLocator)
        /// <summary>
        /// Count the auras in the geometry
        /// </summary>
        /// <param name="testLocator"></param>
        /// <returns></returns>
        public override int CountAuras(Locator testLocator)
            => (from _obj in testLocator.AllConnectedOf<CoreObject>()
                from _adj in _obj.Adjuncts.Where(_a => _a.IsActive).OfType<IUndeadAura>()
                select _adj).Count();
        #endregion

        #region public override IAura GetStrongestAura(Locator testLocator)
        public override IAura GetStrongestAura(Locator testLocator)
        {
            AuraStrength _strong = AuraStrength.None;
            IUndeadAura _aura = null;
            foreach (var _obj in testLocator.AllConnectedOf<CoreObject>())
            {
                // TODO: look for blocker
                foreach (Adjunct _adj in _obj.Adjuncts.Where(_a => _a.IsActive))
                {
                    if ((_adj is IUndeadAura _mAura)
                        && (_mAura.Strength > _strong))
                    {
                        // capture new strongest
                        _strong = _mAura.Strength;
                        _aura = _mAura;

                        // far enough
                        if (_aura.Strength == AuraStrength.Overwhelming)
                            return _aura;
                    }
                }
            }
            return _aura;
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
                    if (_adj is IUndeadAura _uAura)
                    {
                        // basic description
                        var _descr = new Description(@"Strength", _uAura.Strength.ToString());
                        var _info = new Informable(_descr);

                        // link it to the environment
                        var _marker = new ExtraInfoMarker(GetExtraInfoSource(), _info,
                            testLocator.GeometricRegion, false, _uAura.AuraStrength, null);
                        yield return _marker;
                    }
                }
            }
            yield break;
        }
        #endregion

        public override object Clone()
        {
            return new DetectUndeadEffect(Source as INamedActionSource, Range);
        }
    }
}
