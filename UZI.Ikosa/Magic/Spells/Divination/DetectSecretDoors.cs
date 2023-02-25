using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class DetectSecretDoors : DetectBase<DetectSecretDoorsAdjunct>
    {
        public override string DisplayName => @"Detect Secret Doors";
        public override string Description => @"Sense secret doors designed to be unnoticed.";
        public override DurationRule DurationRule(int subMode) => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));
        protected override DetectSecretDoorsAdjunct NewEffect(MagicPowerEffect source, IAdjunctable target)
        {
            foreach (var _range in source.CapabilityRoot.GetCapability<IRegionCapable>().Dimensions(null, source.CasterLevel))
                return new DetectSecretDoorsAdjunct(source.MagicPowerActionSource, _range);
            return new DetectSecretDoorsAdjunct(source.MagicPowerActionSource, 60);
        }
    }

    [Serializable]
    public class DetectSecretDoorsAdjunct : DetectAdjunctBase, IDetectExtraInfo
    {
        public DetectSecretDoorsAdjunct(INamedActionSource source, double range)
            : base(source, range)
        {
        }

        #region public override bool DoesLocatorMatch(Locator testLocator)
        public override bool DoesLocatorMatch(Locator testLocator)
        {
            foreach (var _obj in testLocator.AllConnectedOf<CoreObject>())
            {
                // TODO: look for blocker
                if (_obj is IOpenable _openable)
                {
                    return _obj.HasActiveAdjunct<Searchable>();
                }
            }
            return false;
        }
        #endregion

        #region Actions
        protected override IEnumerable<ActionBase> PrimeActions(LocalActionBudget budget)
        {
            if (budget.CanPerformRegular)
                yield return new DetectPresenceAction<DetectSecretDoorsAdjunct>(@"Detect.SecretDoors.1", 
                    @"Detect Presence of Secret Doors", this, @"101");
            yield break;
        }

        protected override IEnumerable<ActionBase> SecondActions(LocalActionBudget budget)
        {
            if (budget.CanPerformRegular)
                yield return new DetectDivinationAction<DetectSecretDoorsAdjunct>(@"Detect.SecretDoors.2", 
                    @"Location/Direction of secret doors", this, @"102");
            yield break;
        }

        protected override IEnumerable<ActionBase> ThirdActions(LocalActionBudget budget)
        {
            if (budget.CanPerformRegular)
                yield return new FindOpenerAction(@"Detect.SecretDoors.3", 
                    @"Mechanism to open a secret door", this, @"103");
            yield break;
        }
        #endregion

        #region IDetectExtraInfo Members
        public IEnumerable<ExtraInfo> GetExtraInformation(CoreActivity activity, Locator testLocator)
        {
            // prepare for opener location
            var _findAct = activity.Action as FindOpenerAction;

            var _critter = Anchor as Creature;
            foreach (var _obj in testLocator.AllConnectedOf<CoreObject>())
            {
                if (_findAct == null)
                {
                    if (_obj is IOpenable _openable)
                    {
                        // TODO: look for blocker
                        foreach (var _adj in _obj.Adjuncts.Where(_a => _a.IsActive && typeof(Searchable).IsAssignableFrom(_a.GetType())))
                        {
                            if (_adj is Searchable _srch)
                            {
                                // basic description
                                var _descr = new Description(@"Secret Door", @"Secret Door");
                                var _info = new Informable(_descr);

                                // try to find the secret door with a decent search check
                                var _sData = new SearchData(_critter, new Deltable(40), false); // NOTE: should be high enough for non-legendary secret doors
                                var _srchSet = new Interaction(activity.Actor, _critter, _obj, _sData);
                                _obj.HandleInteraction(_srchSet);

                                // link it to the environment (direction-only if we couldn't find it with senses)
                                var _marker = new ExtraInfoMarker(GetExtraInfoSource(), _info,
                                    testLocator.GeometricRegion, _critter.Awarenesses.GetAwarenessLevel(_obj.ID) < AwarenessLevel.Aware,
                                    AuraStrength.Moderate, null);
                                yield return _marker;

                                // only mark one per object
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // looking for a mechanism for the door
                    if ((_obj is OpenerCloser _oc) && (_oc.Openable == _findAct.Openable))
                    {
                        // TODO: look for blocker
                        // basic description
                        var _descr = new Description(@"Mechanism", @"Opening Mechanism");
                        var _info = new Informable(_descr);

                        // blast a search check to the object (just in case it requires searching
                        var _sData = new SearchData(_critter, new Deltable(40), false); // NOTE: should be high enough for non-legendary hidden mechanisms
                        var _srchSet = new Interaction(activity.Actor, _critter, _obj, _sData);
                        _obj.HandleInteraction(_srchSet);

                        // link it to the environment (direction-only if we couldn't find it with senses)
                        var _marker = new ExtraInfoMarker(GetExtraInfoSource(), _info, testLocator.GeometricRegion,
                            _critter.Awarenesses.GetAwarenessLevel(_obj.ID) < AwarenessLevel.Aware,
                            AuraStrength.Moderate, null);
                        yield return _marker;
                    }
                }
            }
            yield break;
        }
        #endregion

        public override Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Detect Secret Doors", ID);

        public override object Clone()
            => new DetectSecretDoorsAdjunct(Source as INamedActionSource, Range);
    }

    [Serializable]
    public class FindOpenerAction : DetectDivinationAction<DetectSecretDoorsAdjunct>
    {
        public FindOpenerAction(string key, string displayName, DetectSecretDoorsAdjunct detector, string orderKey)
            : base(key, displayName, detector, orderKey)
        {
            _Openable = null;
        }

        private IOpenable _Openable;
        public IOpenable Openable => _Openable;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            AimTarget _target = activity.Targets.Where(_t => _t.Key.Equals(@"Secret Door")).FirstOrDefault();
            if (_target != null)
            {
                _Openable = _target.Target as IOpenable;
                if (_Openable != null)
                {
                    // could make sure we only check on searchable doors, but this allows us to find hidden mechanisms for non-secret doors...
                    return base.OnPerformActivity(activity);
                }
            }
            return activity.GetActivityResultNotifyStep(@"Target must be openable");
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new AwarenessAim(@"Secret Door", @"Secret Door", FixedRange.One, FixedRange.One, new FixedRange(Detector.Range), new ObjectTargetType());
            yield break;
        }
    }
}
