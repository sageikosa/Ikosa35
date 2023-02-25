using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    /// <summary>GroupMasterAdjunct added to the evocation holder</summary>
    [Serializable]
    public class SpiritualWeaponEvocation : GroupMasterAdjunct, ITrackTime, ISimpleStep, IAttackSource
    {
        /// <summary>GroupMasterAdjunct added to the evocation holder</summary>
        public SpiritualWeaponEvocation(MagicPowerEffect effect, SpiritualWeaponGroup group)
            : base(effect, group)
        {
            _Zero = new DeltableQualifiedDelta(0, @"Touch", this);
            _Attacking = false;
        }

        #region state
        private double _AttackNow;
        private double _AttackAuto;
        private bool _Attacking;
        private int _MaxAttacks;
        private DeltableQualifiedDelta _Zero;
        private WeaponHeadAttribute _WeaponHead;
        #endregion

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            base.OnAnchorSet(oldAnchor, oldSetting);
            if (Anchor != null)
            {
                // NOTE: attack time is set when the spell is cast
                _AttackAuto = (Anchor?.GetCurrentTime() ?? 0d);
                _AttackNow = (Anchor?.GetCurrentTime() ?? 0d);

                _Attacking = true;
                Anchor?.StartNewProcess(new SimpleStep(null, this), @"Spiritual Weapon");

                // when first anchored, max attacks = 1
                _MaxAttacks = 1;
            }
        }

        public SpiritualWeaponGroup SpiritualWeaponGroup => Group as SpiritualWeaponGroup;
        public MagicPowerEffect MagicPowerEffect => Source as MagicPowerEffect;
        public SpellSource SpellSource => MagicPowerEffect.MagicPowerActionSource as SpellSource;


        /// <summary>
        /// Time automatic attack will happen if not redirected, or if directed to attack now (when allowed).
        /// Will skew from AttackNow as additional processes are created for attacks.
        /// </summary>
        public double AttackAuto => _AttackAuto;

        /// <summary>Time attack now can be used.</summary>
        public double AttackNow => _AttackNow;

        public int MaxAttacks { get => _MaxAttacks; set => _MaxAttacks = value; }

        public override object Clone()
            => new SpiritualWeaponEvocation(MagicPowerEffect, SpiritualWeaponGroup);

        // ITrackTime
        public double Resolution => Round.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            // when attack time comes, make the attack, then reset for next time
            if (!_Attacking)
            {
                if (((timeVal >= _AttackAuto) && (direction == TimeValTransition.Leaving))
                    || (timeVal > _AttackAuto))
                {
                    _Attacking = true;
                    Anchor?.StartNewProcess(new SimpleStep(null, this), @"Spiritual Weapon");
                }
            }
        }

        // ISimpleStep
        public bool DoStep(CoreStep actualStep)
        {
            _Attacking = false;
            var _locator = Anchor?.GetLocated().Locator;
            if (_locator != null)
            {
                // controller needed as fallback and to calculate max attacks
                var _actor = SpiritualWeaponGroup.ControlCreature;

                // must have a target to attack
                if (SpiritualWeaponGroup.Target != null)
                {
                    // controller locator
                    var _aLoc = _actor?.GetLocated()?.Locator;

                    // try to get target locator
                    var _tLoc = SpiritualWeaponGroup.Target.Anchor?.GetLocated().Locator;
                    if (_tLoc == null)
                    {
                        // ... relocate to actor locator cell closest to current holder location
                        var _location = new Cubic(
                            _aLoc.GeometricRegion.AllCellLocations()
                            .OrderBy(_cl => _locator.GeometricRegion.NearDistanceToCell(_cl))
                            .FirstOrDefault()?.ToCellPosition(), (Anchor as MagicEffectHolder).GeometricSize);
                        _locator.Relocate(_location, _locator.PlanarPresence);

                        // eject target
                        SpiritualWeaponGroup.Target?.Eject();
                    }
                    else
                    {
                        // find target, make sure overlapped
                        if (!_locator.OverlappedLocators(_locator.PlanarPresence).Contains(_tLoc))
                        {
                            // since it moved, try to follow it
                            var _mode = SpellSource.SpellDef.SpellModes.FirstOrDefault();
                            if (_mode != null)
                            {
                                // need to know who is controlling it
                                var _aim = _mode.AimingMode(_actor, _mode)
                                    .OfType<AwarenessAim>().FirstOrDefault();
                                if (_aim != null)
                                {
                                    // figure out how far it is allowed to go
                                    var _casterLevel = SpiritualWeaponGroup.SpellSource.CasterLevel;
                                    var _range = _aim.Range.EffectiveRange(_actor, _casterLevel);
                                    var _distance = _tLoc.GeometricRegion.NearDistance(_locator.GeometricRegion);
                                    if (_distance <= _range)
                                    {
                                        // relocate to target locator cell closest to actor
                                        var _location = new Cubic(
                                            _tLoc.GeometricRegion.AllCellLocations()
                                            .OrderBy(_cl => _aLoc.GeometricRegion.NearDistanceToCell(_cl))
                                            .FirstOrDefault()?.ToCellPosition(), (Anchor as MagicEffectHolder).GeometricSize);
                                        _locator.Relocate(_location, _locator.PlanarPresence);
                                    }
                                    else
                                    {
                                        // target too far ...
                                        // ... relocate to actor locator cell closest to target
                                        var _location = new Cubic(
                                            _aLoc.GeometricRegion.AllCellLocations()
                                            .OrderBy(_cl => _tLoc.GeometricRegion.NearDistanceToCell(_cl))
                                            .FirstOrDefault()?.ToCellPosition(), (Anchor as MagicEffectHolder).GeometricSize);
                                        _locator.Relocate(_location, _locator.PlanarPresence);

                                        // break relation to target
                                        SpiritualWeaponGroup.Target?.Eject();

                                        // TODO: notify actor
                                        return true;
                                    }
                                }
                                else
                                {
                                    // shouldn't happen, but couldn't get an aim target, so de-target
                                    var _location = new Cubic(
                                       _aLoc.GeometricRegion.AllCellLocations()
                                       .OrderBy(_cl => _tLoc.GeometricRegion.NearDistanceToCell(_cl))
                                       .FirstOrDefault()?.ToCellPosition(), (Anchor as MagicEffectHolder).GeometricSize);
                                    _locator.Relocate(_location, _locator.PlanarPresence);

                                    // break relation to target
                                    SpiritualWeaponGroup.Target?.Eject();
                                }
                            }
                            else
                            {
                                // no spell mode, shouldn't happen, but just roll with it
                                SpiritualWeaponGroup.Target?.Eject();
                            }
                        }
                    }
                }

                // if still active (after potential adjustments), and if still targeting something
                if (IsActive && (SpiritualWeaponGroup.Target != null))
                {
                    // queue up attack sequence based on current max attacks
                    for (var _ax = 0; _ax < MaxAttacks; _ax++)
                    {
                        new SpiritualWeaponAttackStep(actualStep, SpiritualWeaponGroup, _ax);
                    }

                    // attack time only calculated anew if an attack step is started
                    _AttackAuto = (Anchor?.GetCurrentTime() ?? _AttackAuto) + Round.UnitFactor;
                    _AttackNow += Round.UnitFactor;
                }

                // set max attacks to full attack count (redirect will set it to 1)
                MaxAttacks = _actor.BaseAttack.FullAttackCount;
            }
            return true;
        }

        private WeaponHeadAttribute GetWeaponHead()
        {
            if (_WeaponHead == null)
            {
                // get weapon head attribute from specified type
                static WeaponHeadAttribute _getAttib(Type type)
                    => type?.GetCustomAttributes(typeof(WeaponHeadAttribute), true)
                    .OfType<WeaponHeadAttribute>()
                    .Where(_wha => _wha.Main)
                    .FirstOrDefault();

                // use either the controller's devotional weapon, or Club as a basic weapon head
                _WeaponHead = _getAttib(SpiritualWeaponGroup.ControlCreature?.Devotion?.WeaponType)
                    ?? _getAttib(typeof(Club));
            }
            return _WeaponHead;
        }

        // IAttackSource
        public DeltableQualifiedDelta AttackBonus => _Zero;

        public int CriticalRange
            => 21 - GetWeaponHead().CriticalLow;

        public DeltableQualifiedDelta CriticalRangeFactor
            => new DeltableQualifiedDelta(1, @"Critical Range", typeof(WeaponHead));

        public DeltableQualifiedDelta CriticalDamageFactor
            => new DeltableQualifiedDelta(GetWeaponHead().CriticalMultiplier, @"Multi", typeof(WeaponHead));

        public AdjunctSet Adjuncts
            => Anchor?.Adjuncts;

        public IVolatileValue ActionClassLevel
            => SpiritualWeaponGroup.SpellSource.ActionClassLevel;

        public bool IsSourceChannel(IAttackSource source)
            => source == this;

        public void AttackResult(AttackResultStep result, Interaction attack)
        {
            // first, damage against the target
            var _atkFB = attack.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            if (_atkFB?.Hit ?? false)
            {
                // sub modes depends on critical hit
                var _subModes = _atkFB.CriticalHit
                    ? Enumerable.Range(0, CriticalDamageFactor.EffectiveValue).ToArray()
                    : 0.ToEnumerable().ToArray();

                // spell mode as root
                var _root = SpellSource.SpellDef.SpellModes.FirstOrDefault();
                SpellDef.ApplyDamage(SpellSource, SpiritualWeaponGroup.ControlCreature, _root, attack,
                    result, result, false, _subModes);
            }
        }

        /// <summary>Called in AttackResultStep constuctor</summary>
        public IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction attack)
        {
            // first, damage against the target
            var _atkFB = attack.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            if (_atkFB?.Hit ?? false)
            {
                // sub modes depends on critical hit
                var _subModes = _atkFB.CriticalHit
                    ? Enumerable.Range(0, CriticalDamageFactor.EffectiveValue).ToArray()
                    : 0.ToEnumerable().ToArray();

                // spell mode as root
                var _root = SpellSource.SpellDef.SpellModes.FirstOrDefault();

                // regular spell damage mode prerequisites (criticality calculated by sub-mode array)
                return SpellDef.GetDamageModePreRequisites(
                    SpellSource, SpiritualWeaponGroup.ControlCreature, _root,
                    attack, _subModes, 1, 1, false);
            }
            return Enumerable.Empty<StepPrerequisite>();
        }
    }
}
