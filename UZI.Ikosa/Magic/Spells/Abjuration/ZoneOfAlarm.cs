using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Time;
using Uzi.Visualize;
using Uzi.Core.Contracts;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class ZoneOfAlarm : SpellDef, ISpellMode, IDurableCapable, IDurableAnchorCapable, IRegionCapable, IGeometryCapable<SpellSource>
    {
        public override string DisplayName => @"Zone of Alarm";
        public override string Description => @"Alarm when a region is entered without a password spoken.";
        public override MagicStyle MagicStyle => new Abjuration();

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new FocusComponent();
                yield break;
            }
        }
        #endregion

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

        #region ISpellMode Members

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new LocationAim(@"Location", @"Center of Region", LocationAimMode.Any, FixedRange.One, FixedRange.One, new NearRange());
            yield return new CharacterStringAim(@"Password", @"Password", FixedRange.One, new FixedRange(20));
            yield return new OptionAim(@"Type", @"Mental/Audible", true, FixedRange.One, FixedRange.One, AlarmTypes());
            yield break;
        }

        private IEnumerable<OptionAimOption> AlarmTypes()
        {
            yield return new OptionAimOption { Key = @"Mental", Name = @"Mental", Description = @"Personal ping at great distances" };
            yield return new OptionAimOption { Key = @"Audible", Name = @"Audible", Description = @"Public ring at short distances" };
            yield break;
        }

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            var _location = activation.TargetingProcess.GetFirstTarget<LocationTarget>(@"Location");
            var _geoInteract = new GeometryInteract
            {
                ID = Guid.NewGuid(),
                Point3D = _location.SupplyPoint3D(),
                Position = _location.Location.ToCellPosition(),
                AimMode = _location.LocationAimMode
            };
            SpellDef.CarryDurableEffectsToIntersection(activation, _geoInteract, 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // get password and mental/audible option
            var _password = apply.TargetingProcess.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Password")) as CharacterStringTarget;
            var _option = apply.TargetingProcess.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Type")) as OptionTarget;

            // set up virtual object
            var _iTarget = apply.TargetingProcess.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Location")) as LocationTarget;
            var _intObj = new IntersectionObject(DisplayName, new Intersection(_iTarget.Location));
            var _map = _iTarget.MapContext.Map;
            var _loc = new Locator(_intObj, _iTarget.MapContext, _intObj.GeometricSize,
                new Cubic(_iTarget.Location, _intObj.GeometricSize));

            // setup group master
            var _builder = GetApplyGeometry(apply);
            var _master = new AlarmAdjunctGroup(apply.PowerUse.PowerActionSource, _password.CharacterString,
                new Intersection(_iTarget.Location), _builder);

            // add group master to SpellEffect as target
            var _effect = apply.DurableMagicEffects.FirstOrDefault();
            _effect.AllTargets.Add(new ValueTarget<AlarmAdjunctGroup>(@"Master", _master));

            // signal mental alarm
            if (_option.Option.Key.Equals(@"Mental"))
            {
                // setup mental alert and establish the group in the setting immediately 
                // NOTE: this prevents the mental adjunct from disconnecting on deserialization rebinding
                var _mental = new MentalAlarmAdjunct(apply.PowerUse.PowerActionSource, _master);
                apply.Actor?.AddAdjunct(_mental);
            }

            // add the spell effect controlling the alarm adjunct to the intersection object
            DismissibleMagicEffectControl.CreateControl(_effect, apply.Actor, _intObj);
            _intObj.AddAdjunct(_effect);
        }

        #endregion

        #region IDurableMode Members

        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // alarm adjunct is already on the virtual object, this just activates it
            if (source.AnchoredAdjunctObject is AlarmAdjunct _alarm)
            {
                _alarm.IsActive = true;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            // alarm adjunct is already on the virtual object, this just deactivates it
            if (source.AnchoredAdjunctObject is AlarmAdjunct _alarm)
            {
                _alarm.IsActive = false;
            }
        }

        public bool IsDismissable(int subMode)
            => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(2, new Hour(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        #endregion

        #region IDurableAnchorMode Members

        public object OnAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _group = _spellEffect.GetTargetValue<AlarmAdjunctGroup>(@"Master");
                if (_group != null)
                {
                    // create capture zone
                    var _alarm = new AlarmAdjunct(_spellEffect.MagicPowerActionSource, _group) { InitialActive = false };
                    target.AddAdjunct(_alarm);
                    return _alarm;
                }
            }
            return null;
        }

        public void OnEndAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            // when the durable spell effect is removed from the virtual object, the object is removed from context
            var _intersect = target as IntersectionObject;
            _intersect?.UnPath();
            _intersect?.UnGroup();

            // remove the adjunct group and all members from their anchors
            (source as MagicPowerEffect)?.GetTargetValue<AlarmAdjunctGroup>(@"Master")?.EjectMembers();
        }

        #endregion

        // IRegionCapable Members
        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
            => 20d.ToEnumerable();

        // IGeometryCapable Members
        public IGeometryBuilder GetBuilder(IPowerUse<SpellSource> powerUse, CoreActor actor)
        {
            // get radius
            var _radius = powerUse.CapabilityRoot.GetCapability<IRegionCapable>()
                .Dimensions(actor, powerUse.PowerActionSource.CasterLevel)
                .FirstOrDefault();
            return new SphereBuilder(Convert.ToInt32(_radius / 5));
        }
    }

    /// <summary>
    /// Adjunct group for binding the virtual alarm zone to a signalling mechanism
    /// </summary>
    [Serializable]
    public class AlarmAdjunctGroup : AdjunctGroup, IAudible
    {
        #region construction
        public AlarmAdjunctGroup(SpellSource source, string password, Intersection intersect, IGeometryBuilder builder)
            : base(source)
        {
            _Password = password;
            _Geom = new Geometry(builder, intersect, true);
        }
        #endregion

        #region state
        private string _Password;
        private Geometry _Geom;
        private IAdjunctable _Origin;
        #endregion

        public string Name => $@"Zone of Alarm Sound";
        public string Password => _Password;
        public Geometry Geometry => _Geom;
        public SpellSource SpellSource => Source as SpellSource;

        // IAudible
        public Guid SoundGroupID => ID;
        public Guid SourceID => _Origin?.ID ?? Guid.Empty;

        public SoundInfo GetSoundInfo(ISensorHost sensors, SoundAwareness awareness)
        {
            // how well the awareness was perceived
            var _exceed = awareness.CheckExceed;
            var _presence = Math.Max(awareness.SourceRange, 1);
            var _strength = (awareness.Magnitude ?? 0) / _presence;

            var _info = new SoundInfo
            {
                Strength = _strength,
                Description = $@"{SoundInfo.GetStrengthDescription(_presence, _strength, _exceed)}bell ringing"
            };

            return _info;
        }

        public void LostSoundInfo(ISensorHost sensors)
        {
        }

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();

        public bool IsDurable => true;

        public IEnumerable<MentalAlarmAdjunct> MentalAlarms
            => Members.OfType<MentalAlarmAdjunct>().Select(_t => _t);

        #region public void TriggerAlarm(IAdjunctable origin)
        public void TriggerAlarm(IAdjunctable origin)
        {
            var _mentals = MentalAlarms.ToArray();
            if (_mentals.Length > 0)
            {
                // do mental alarms
                foreach (var _m in _mentals.Where(_ment => _ment.IsActive))
                {
                    var _critter = _m.Anchor as Creature;
                    if (_m.Anchor != null)
                    {
                        // NOTE: assuming the critter is within range...
                        _critter.ExtraInfoMarkers.Add(new ExtraInfo(new ExtraInfoSource
                        {
                            ID = ID,
                            Message = SpellSource.DisplayName
                        }, new Informable(new Description(@"Alarm Trigger", Password)), null));
                    }
                }
            }
            else
            {
                // establish audible alarms
                _Origin = origin;
                var _expiry = new Expiry(
                    new SoundParticipant(SpellSource, new SoundGroup(SpellSource, new SoundRef(this, -6, 180, origin.GetSerialState()))),
                    (origin?.GetCurrentTime() ?? 0d) + Round.UnitFactor, TimeValTransition.Entering, Round.UnitFactor);
                origin.AddAdjunct(_expiry);
            }
        }
        #endregion
    }

    /// <summary>
    /// Adjunct attached to the virtual spell object for monitoring the alarm zone
    /// </summary>
    [Serializable]
    public class AlarmAdjunct : GroupMasterAdjunct, ILocatorZone
    {
        public AlarmAdjunct(MagicPowerActionSource source, AlarmAdjunctGroup group)
            : base(source, group)
        {
            _Capture = null;
        }

        private LocatorCapture _Capture;

        public SpellSource SpellSource => Source as SpellSource;
        public AlarmAdjunctGroup Master => Group as AlarmAdjunctGroup;

        #region ILocatorZone Members

        public void Start(Locator locator) { }
        public void End(Locator locator) { }

        public void Enter(Locator locator)
        {
            var _anchorLoc = Locator.FindFirstLocator(Anchor);
            if (_anchorLoc.MapContext == locator.MapContext)
            {
                foreach (var _critter in locator.AllConnectedOf<Creature>())
                {
                    // if the creature is as big or bigger than Tiny ...
                    if (_critter.Body.Sizer.Size.Order >= Size.Tiny.Order)
                    {
                        // ... and the creature's last utterance wasn't the password in the past minute
                        if (!_critter.Languages.LastUtterance.Words.Equals(Master.Password) ||
                            (_critter.Languages.LastUtterance.Time < ((_critter?.GetCurrentTime() ?? 0d) - Minute.UnitFactor)))
                        {
                            Master.TriggerAlarm(Anchor);
                            return;
                        }
                    }
                }
            }
        }

        public void Exit(Locator locator) { }
        public void Capture(Locator locator) { }
        public void Release(Locator locator) { }
        public void MoveInArea(Locator locator, bool followOn) { }

        #endregion

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            // setup the capture zone during anchoring, tear it down during unanchoring
            if (oldAnchor == null)
            {
                // adding (starting in the off position, anchoring is not activation)
                var _loc = Locator.FindFirstLocator(Anchor);
                _Capture = new LocatorCapture(_loc.MapContext, this, Master.Geometry, _loc, this, false, _loc.PlanarPresence);
            }
            else
            {
                // removing
                _Capture.MapContext.LocatorZones.Remove(_Capture);
                _Capture = null;
            }
        }

        public override object Clone()
            => new AlarmAdjunct(SpellSource, Master);
    }

    /// <summary>
    /// Anchors to a caster so the group master can locate the caster on a mental alarm
    /// </summary>
    [Serializable]
    public class MentalAlarmAdjunct : GroupMemberAdjunct
    {
        public MentalAlarmAdjunct(SpellSource source, AlarmAdjunctGroup group)
            : base(source, group)
        {
        }

        public SpellSource SpellSource => Source as SpellSource;
        public AlarmAdjunctGroup Master => Group as AlarmAdjunctGroup;

        public override object Clone()
            => new MentalAlarmAdjunct(SpellSource, Master);
    }
}
