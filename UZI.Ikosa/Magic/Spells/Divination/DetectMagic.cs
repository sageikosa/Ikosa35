using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Time;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    /// <summary>Spell Definition for Detect Magic</summary>
    [Serializable]
    public class DetectMagic : DetectBase<DetectMagicEffect>
    {
        public override string DisplayName => @"Detect Magic";
        public override string Description => @"Presence, strength and location of magical auras";
        public override DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));
        protected override DetectMagicEffect NewEffect(MagicPowerEffect source, IAdjunctable target)
        {
            foreach (var _range in source.CapabilityRoot.GetCapability<IRegionCapable>().Dimensions(null, source.CasterLevel))
            {
                return new DetectMagicEffect(source.MagicPowerActionSource, _range);
            }

            return new DetectMagicEffect(source.MagicPowerActionSource, 60);
        }
    }

    /// <summary>Adjunct that grants DetectMagic actions</summary>
    [Serializable]
    public class DetectMagicEffect : DetectAuraAdjunctBase, IDetectExtraInfo
    {
        public DetectMagicEffect(INamedActionSource source, double range) :
            base(source, range)
        {
            _Checked = [];
        }

        #region data
        protected Dictionary<Guid, bool> _Checked;
        #endregion

        #region Actions to Emit
        protected override IEnumerable<ActionBase> PrimeActions(LocalActionBudget budget)
        {
            if (budget.CanPerformRegular)
            {
                yield return new DetectPresenceAction<DetectMagicEffect>(@"Detect.Magic.1", @"Detect Magic Presence", this, @"101");
            }

            yield break;
        }

        protected override IEnumerable<ActionBase> SecondActions(LocalActionBudget budget)
        {
            if (budget.CanPerformRegular)
            {
                yield return new DetectNumberAuraAction<DetectMagicEffect>(@"Detect.Magic.2", @"Detect Number of Magic Auras", this, @"102");
            }

            yield break;
        }

        protected override IEnumerable<ActionBase> ThirdActions(LocalActionBudget budget)
        {
            if (budget.CanPerformRegular)
            {
                yield return new DetectDivinationAction<DetectMagicEffect>(@"Detect.Magic.3", @"Detect Location and Strength of Magic Auras", this, @"103");
            }

            yield break;
        }
        #endregion

        #region public override void ValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args)
        public override void ValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args)
        {
            // reset counter
            base.ValueChanged(sender, args);

            // cancel divination markers (since geometry changed)
            try { (Anchor as Creature)?.ExtraInfoMarkers.RemoveSource(ID); }
            catch { }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            // cancel divination markers
            try { (Anchor as Creature)?.ExtraInfoMarkers.RemoveSource(ID); }
            catch { }

            // remove action provider and geometric change hook
            base.OnDeactivate(source);
        }
        #endregion

        private IEnumerable<IMagicAura> GetAuras(Locator testLocator)
            => (from _obj in testLocator.AllConnectedOf<CoreObject>()
                where NoBlocker(_obj)
                from _ma in _obj.Adjuncts.Where(_a => _a.IsActive).OfType<IMagicAura>()
                select _ma);

        private bool NoBlocker(CoreObject testObj)
            // TODO: consider detection transit along line of detect, and detection handler...
            => (!(from _a in testObj.Adjuncts
                  where _a.IsActive && typeof(IMagicAura).IsAssignableFrom(_a.GetType())
                  let _ma = _a as IMagicAura
                  where _ma.AuraStrength == AuraStrength.None
                  select _ma).Any());

        /// <summary>Gets the count of the auras in the locator</summary>
        public override int CountAuras(Locator testLocator)
            => GetAuras(testLocator).Count();

        public override IAura GetStrongestAura(Locator testLocator)
            => GetAuras(testLocator)
                .OrderByDescending(_ma => _ma.AuraStrength)
                .FirstOrDefault();

        public override bool DoesLocatorMatch(Locator testLocator)
            => GetAuras(testLocator).Any();

        #region IDetectExtraInfo Members
        public IEnumerable<ExtraInfo> GetExtraInformation(CoreActivity activity, Locator testLocator)
        {
            var _critter = Anchor as Creature;
            foreach (var _obj in testLocator.AllConnectedOf<CoreObject>())
            {
                // if there are no aura strength "none" auras...
                if (NoBlocker(_obj))
                {
                    foreach (var _adj in _obj.Adjuncts.Where(_a => _a.IsActive))
                    {
                        if (_adj is IMagicAura _mAura)
                        {
                            // basic description
                            var _descr = new Description(@"Strength", _mAura.MagicStrength.ToString());
                            var _info = new Informable(_descr);

                            // must be a creature with awareness 
                            IActionProvider _provider = null;
                            var _style = false;
                            if ((_critter != null) && (_critter.Awarenesses[_obj.ID] == AwarenessLevel.Aware))
                            {
                                if (_Checked.TryGetValue(_mAura.ID, out _style))
                                {
                                    // previously done
                                    _info.Infos.Add(new Description(@"Magic Style", _mAura.MagicStyle.ToString()));
                                }
                                else
                                {
                                    // must make a check to get more data
                                    _provider = new MagicAuraStyleActions(_critter, _mAura, _Checked, _info);
                                }
                            }

                            // link it to the environment
                            var _marker = new ExtraInfoMarker(GetExtraInfoSource(), _info,
                                testLocator.GeometricRegion, false, _mAura.MagicStrength, _provider);
                            yield return _marker;
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        public override Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Detect Magic", ID);

        public override object Clone()
            => new DetectMagicEffect(Source as INamedActionSource, Range);
    }

    [Serializable]
    public class MagicAuraStyleActions : IExternalActionProvider, IActionSource
    {
        public MagicAuraStyleActions(Creature critter, IMagicAura aura, Dictionary<Guid, bool> results, Informable inform)
        {
            _Aura = aura;
            _ID = Guid.NewGuid();
            _Results = results;
            _Critter = critter;
            _Inform = inform;
        }

        #region data
        private IMagicAura _Aura;
        private Guid _ID;
        private Creature _Critter;
        private Dictionary<Guid, bool> _Results;
        private Informable _Inform;
        #endregion

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // only allow check action if we haven't checked it before
            if (!_Results.ContainsKey(_Aura.ID))
            {
                yield return new MagicAuraStyleCheckAction(this, @"100");
            }

            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            return new Info { Message = @"Check for School of Magic" };
        }

        #endregion

        /// <summary>Used as both the IActionProvider and the IActionSource ID</summary>
        public Guid ID => _ID;
        public Guid PresenterID => _ID;

        public Dictionary<Guid, bool> Results => _Results;
        public Creature Creature => _Critter;
        public IMagicAura Aura => _Aura;
        public Informable Informable => _Inform;

        // IActionSource
        public IVolatileValue ActionClassLevel
            => new Deltable(1);
    }

    [Serializable]
    public class MagicAuraStyleCheckAction : ActionBase
    {
        public MagicAuraStyleCheckAction(MagicAuraStyleActions provider, string orderKey)
            : base(provider, new ActionTime(TimeType.Free), false, false, orderKey)
        {
            _Provider = provider;
        }

        private MagicAuraStyleActions _Provider;

        public override string Key => @"CheckMagicStyle";
        public override string DisplayName(CoreActor actor) => @"Check Magic Style";
        public override bool IsStackBase(CoreActivity activity) => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _result = activity.GetFirstTarget<SuccessCheckTarget>(@"Check");
            if (_result != null)
            {
                var _success = _result.Success;
                _Provider.Results[_Provider.ID] = _success; // NOTE: this will suppress further MagicAuraStyleCheckActions
                if (_success)
                {
                    _Provider.Informable.Infos.Add(new Description(@"Magic Style", _Provider.Aura.MagicStyle.ToString()));

                    // TODO: next step refresh markers...
                    var _step = activity.GetActivityResultNotifyStep($@"Magic Style: {_Provider.Aura.MagicStyle.ToString()}");
                    _step.AppendFollowing(activity.GetNotifyStep(
                        new RefreshNotify(false, true, false, false, false)));
                    return _step;
                }
                else
                {
                    var _step = activity.GetActivityResultNotifyStep(@"Could not determine magic style");
                    _step.AppendFollowing(activity.GetNotifyStep(
                        new RefreshNotify(false, true, false, false, false)));
                    return _step;
                }
            }

            return CreateActivityFeedback(activity, new RefreshNotify(false, true, false, false, false));
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            var _check = new SuccessCheck(_Provider.Creature.Skills.Skill<SpellcraftSkill>(), 15 + _Provider.Aura.PowerLevel, _Provider.Aura);
            yield return new SuccessCheckAim(@"Check", @"Spellcraft Check", _check);
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}