using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Time;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    /// <summary>
    /// Base for certain detection spells
    /// </summary>
    /// <typeparam name="DetectEffect"></typeparam>
    [Serializable]
    public abstract class DetectBase<DetectEffect> : SpellDef, ISpellMode, IDurableCapable, IRegionCapable
        where DetectEffect : DetectAdjunctBase
    {
        public override MagicStyle MagicStyle { get { return new Divination(Divination.SubDivination.Detection); } }

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
                yield break;
            }
        }
        #endregion

        protected abstract DetectEffect NewEffect(MagicPowerEffect source, IAdjunctable target);

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new PersonalAim(@"Self", actor);
            yield break;
        }

        public bool AllowsSpellResistance { get { return false; } }
        public bool IsHarmless { get { return true; } }

        public void ActivateSpell(PowerActivationStep<SpellSource> delivery)
        {
            SpellDef.DeliverDurable(delivery, delivery.TargetingProcess.Targets[0], 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IRegionMode Members
        public virtual IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
        {
            yield return 60;
            yield break;
        }
        #endregion

        #region IDurableSpellMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            DetectEffect _dmEffect = NewEffect(source as MagicPowerEffect, target);
            if (target is Creature _creature)
            {
                _creature.AddAdjunct(_dmEffect);
            }
            return _dmEffect;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if ((((DurableMagicEffect)source).ActiveAdjunctObject is DetectEffect _dmEffect)
                && (target is Creature _creature))
            {
                _creature.RemoveAdjunct(_dmEffect);
            }
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }

        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public virtual bool IsDismissable(int subMode)
        {
            return true;
        }

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
        {
            return string.Empty;
        }

        public abstract DurationRule DurationRule(int subMode);
        #endregion
    }

    /// <summary>
    /// Base adjunct for detection spell action providers
    /// </summary>
    [Serializable]
    public abstract class DetectAdjunctBase : Adjunct, IActionProvider, IMonitorChange<IGeometricRegion>, ICore, INamedActionSource
    {
        #region Construction
        protected DetectAdjunctBase(INamedActionSource source, double range)
            : base(source)
        {
            _Vol = null;
            _ChainRound = 0;
            _Range = range;
        }
        #endregion

        #region private data
        private double _Range;
        protected int _ChainRound;
        private GeometricTarget _Vol;
        #endregion

        public double Range => _Range;
        public GeometricTarget Volume { get => _Vol; set { _Vol = value; } }

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // hook onto creature's locator
                var _loc = Locator.FindFirstLocator(_critter);
                if (_loc != null)
                {
                    _loc.AddChangeMonitor((IMonitorChange<IGeometricRegion>)this);

                    // ...and, add action provider
                    _critter.Actions.Providers.Add(this, this);
                }
            }
            base.OnActivate(source);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // remove action provider
                _critter.Actions.Providers.Remove(this);

                // unhook creature's locator
                var _loc = Locator.FindFirstLocator(_critter);
                if (_loc != null)
                {
                    _loc.RemoveChangeMonitor((IMonitorChange<IGeometricRegion>)this);
                }
            }
            base.OnDeactivate(source);
        }
        #endregion

        #region public int ChainRound { get; }
        /// <summary>
        /// How many rounds spent (so far) locked in place
        /// </summary>
        public int ChainRound { get { return _ChainRound; } internal set { _ChainRound = value; } }
        #endregion

        public abstract bool DoesLocatorMatch(Locator testLocator);
        protected abstract IEnumerable<ActionBase> PrimeActions(LocalActionBudget budget);
        protected abstract IEnumerable<ActionBase> SecondActions(LocalActionBudget budget);
        protected abstract IEnumerable<ActionBase> ThirdActions(LocalActionBudget budget);

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;

            foreach (var _act in PrimeActions(_budget))
            {
                yield return _act;
            }

            if (_ChainRound > 0)
            {
                foreach (var _act in SecondActions(_budget))
                {
                    yield return _act;
                }

                if (_ChainRound > 1)
                {
                    foreach (var _act in ThirdActions(_budget))
                    {
                        yield return _act;
                    }
                }
            }
            yield break;
        }
        #endregion

        public abstract Info GetProviderInfo(CoreActionBudget budget);

        #region IMonitorChange<IGeometricRegion> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<IGeometricRegion> args)
        {
        }
        public void PreValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args)
        {
        }
        public virtual void ValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args)
        {
            // creature moved, so have to start detecting all over again...
            _ChainRound = 0;
        }
        #endregion

        // IActionSource
        public IVolatileValue ActionClassLevel
            => (Source as IActionSource)?.ActionClassLevel;

        // INamedActionSource 
        public string DisplayName => (Source as INamedActionSource).DisplayName;

        public ExtraInfoSource GetExtraInfoSource()
            => new ExtraInfoSource
            {
                ID = ID,
                Message = DisplayName
            };
    }

    [Serializable]
    public abstract class DetectAuraAdjunctBase : DetectAdjunctBase
    {
        protected DetectAuraAdjunctBase(INamedActionSource source, double range)
            : base(source, range)
        {
        }

        public abstract int CountAuras(Locator testLocator);
        public abstract IAura GetStrongestAura(Locator testLocator);
    }

    public interface IDetectExtraInfo
    {
        IEnumerable<ExtraInfo> GetExtraInformation(CoreActivity activity, Locator testLocator);
    }

    /// <summary>
    /// Chain round=1 detection action [ActionBase (Regular)]
    /// </summary>
    public class DetectPresenceAction<Detect> : ActionBase where Detect : DetectAdjunctBase
    {
        #region Construction
        /// <summary>
        /// Chain round=1 detection action [ActionBase (Regular)]
        /// </summary>
        public DetectPresenceAction(string key, string displayName, Detect detector, string orderKey)
            : base(detector, new ActionTime(TimeType.Regular), false, false, orderKey)
        {
            _Key = key;
            _DisplayName = displayName;
        }
        #endregion

        #region Private Data
        private string _Key;
        private string _DisplayName;
        #endregion

        public override string Key => _Key;
        public override string DisplayName(CoreActor actor) => _DisplayName;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        public Detect Detector
            => Source as Detect;

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        /// <summary>
        /// Determines whether the detection picks something up
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            Detector.Volume =
                activity.Targets.Where(_t => _t.Key.Equals(@"Cone", StringComparison.OrdinalIgnoreCase)).FirstOrDefault() as GeometricTarget;

            var _aLoc = activity.Actor.GetLocated()?.Locator;
            foreach (var _loc in Detector.Volume.MapContext.LocatorsInRegion(Detector.Volume.Geometry.Region, _aLoc.PlanarPresence))
            {
                if (Detector.DoesLocatorMatch(_loc))
                {
                    if (_loc.HasLineOfDetectFromSource(Detector.Volume.Origin.Point3D(), ITacticalInquiryHelper.EmptyArray,
                        _loc.PlanarPresence))
                    {
                        Detector.ChainRound++;
                        return activity.GetActivityResultNotifyStep(@"Auras detected");
                    }
                }
            }
            return activity.GetActivityResultNotifyStep(@"No auras");
        }
        #endregion

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new PersonalConicAim(@"Cone", @"Directional cone", new FixedRange(Detector.Range), activity.Actor);
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }

    /// <summary>
    /// Usually round=2 (number and strength or strongest) [ActionBase (Regular)]
    /// </summary>
    public class DetectNumberAuraAction<Detect> : ActionBase where Detect : DetectAuraAdjunctBase
    {
        #region Construction
        /// <summary>
        /// Usually round=2 (number and strength or strongest) [ActionBase (Regular)]
        /// </summary>
        public DetectNumberAuraAction(string key, string displayName, Detect detector, string orderKey)
            : base(detector, new ActionTime(TimeType.Regular), false, false, orderKey)
        {
            _Key = key;
            _DisplayName = displayName;
        }
        #endregion

        #region Private Data
        private string _Key;
        private string _DisplayName;
        #endregion

        public override string Key => _Key;
        public override string DisplayName(CoreActor actor) => _DisplayName;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        public Detect Detector
            => Source as Detect;

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        /// <summary>
        /// Counts locators with Auras
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _count = 0;
            Detector.ChainRound++;
            var _strong = AuraStrength.None;
            var _aLoc = activity.Actor.GetLocated()?.Locator;
            foreach (var _loc in Detector.Volume.MapContext.LocatorsInRegion(Detector.Volume.Geometry.Region, _aLoc.PlanarPresence))
            {
                // look for auras
                if (Detector.DoesLocatorMatch(_loc))
                {
                    if (_loc.HasLineOfDetectFromSource(Detector.Volume.Origin.Point3D(), ITacticalInquiryHelper.EmptyArray,
                        _loc.PlanarPresence))
                    {
                        // increase the count
                        _count += Detector.CountAuras(_loc);

                        // find strongest
                        AuraStrength _test = Detector.GetStrongestAura(_loc).AuraStrength;
                        if (_test > _strong)
                        {
                            _strong = _test;
                        }
                    }
                }
            }

            return activity.GetActivityResultNotifyStep(
                new Description(@"Aura Result",
                new string[]
                {
                    $@"{_count} Auras",
                    $@"Strongest is {_strong}"
                }));
        }
        #endregion

        /// <summary>
        /// Cannot re-aim, using geometry from the presence sweep
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }

    /// <summary>
    /// Usually round=3, populates divination info for the creature [ActionBase (Regular)]
    /// </summary>
    /// <typeparam name="Detect"></typeparam>
    public class DetectDivinationAction<Detect> : ActionBase where Detect : DetectAdjunctBase, IDetectExtraInfo
    {
        #region Construction
        /// <summary>
        /// Usually round=3, populates divination info for the creature [ActionBase (Regular)]
        /// </summary>
        public DetectDivinationAction(string key, string displayName, Detect detector, string orderKey)
            : base(detector, new ActionTime(TimeType.Regular), false, false, orderKey)
        {
            _Key = key;
            _DisplayName = displayName;
        }

        public DetectDivinationAction(string key, string displayName, ActionTime actionCost, Detect detector, string orderKey)
            : base(detector, actionCost, false, false, orderKey)
        {
        }
        #endregion

        #region Private Data
        private string _Key;
        private string _DisplayName;
        #endregion

        public override string Key => _Key;
        public override string DisplayName(CoreActor actor) => _DisplayName;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        public Detect Detector
            => Source as Detect;

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        /// <summary>
        /// Counts locators with Auras
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _critter = activity.Actor as Creature;
            var _aLoc = _critter.GetLocated()?.Locator;
            foreach (var _loc in Detector.Volume.MapContext.LocatorsInRegion(Detector.Volume.Geometry.Region, _aLoc.PlanarPresence))
            {
                // NOTE: handling line of sight (via awareness) in the GetExtraInformation method
                // TODO: consider adding detection interaction transit
                if (_loc.HasLineOfDetectFromSource(Detector.Volume.Origin.Point3D(), ITacticalInquiryHelper.EmptyArray,
                        _loc.PlanarPresence))
                {
                    foreach (var _divInfo in Detector.GetExtraInformation(activity, _loc))
                    {
                        _critter.ExtraInfoMarkers.Add(_divInfo);
                    }
                }
            }

            return new ExtraInfoStep(activity, Detector);
        }
        #endregion

        /// <summary>
        /// Cannot re-aim, using geometry from the presence sweep
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
