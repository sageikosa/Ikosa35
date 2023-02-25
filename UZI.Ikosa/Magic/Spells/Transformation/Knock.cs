using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Knock : SpellDef, ISpellMode, IRegionCapable
    {
        public override MagicStyle MagicStyle => new Transformation();
        public override IEnumerable<SpellComponent> ArcaneComponents => new VerbalComponent().ToEnumerable();
        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();
        public override string DisplayName => @"Knock";
        public override string Description => @"Open doors and containers.  Suppress arcane lock for 10 minutes.";
        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Openable", @"Openable", FixedRange.One, FixedRange.One, new MediumRange(), new ObjectTargetType());
            yield break;
        }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverSpell(deliver, 0, deliver.TargetingProcess.Targets[0], 0);
        }

        #region public void ApplySpell(PowerApplyStep<SpellSource> apply)
        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // size limits...
            var _actor = apply.Actor;
            var _powerSource = apply.PowerUse.PowerActionSource;
            var _rMode = apply.PowerUse.CapabilityRoot.GetCapability<IRegionCapable>();
            var _maxArea = _rMode.Dimensions(_actor, _powerSource.CasterLevel).First();

            #region void _applyKnock(ICoreObject coreObject)
            void _applyKnock(ICoreObject coreObject)
            {
                var _use = 0;

                // suppress arcane lock for 10 minutes
                var _arcaneLocks = MagicPowerEffect.GetMagicPowerEffects<ArcaneLockEffect>(coreObject).Where(_dme => _dme.IsActive).ToList();
                if (_arcaneLocks.Count > 0)
                {
                    _use++;
                    coreObject.AddAdjunct(new KnockSuppression());
                }

                // dispel holdShuts (should be portals only)
                var _holdShuts = MagicPowerEffect.GetMagicPowerEffects<HoldShutAdjunct>(coreObject).Where(_mpe => _mpe.IsActive).ToList();
                if (_holdShuts.Count > 0)
                {
                    _use++;
                    foreach (var _hs in _holdShuts)
                    {
                        _hs.Eject();
                    }
                }

                // eject stuck (container-items, portals, and furnishing compartments)
                if (_use < 2)
                {
                    _use++;
                    void _unstick(IAdjunctable adjunctable)
                    {
                        // eject stuck directly on openables
                        foreach (var _stuck in adjunctable.Adjuncts.OfType<StuckAdjunct>().ToList())
                        {
                            _stuck.Eject();
                        }
                    }

                    if (coreObject is Furnishing _furnishing)
                    {
                        // any compartments that are stuck get unstuck
                        foreach (var _comp in _furnishing.Connected.OfType<IOpenable>())
                        {
                            _unstick(_comp);
                        }
                    }
                    else
                    {
                        // eject stuck directly on openables
                        _unstick(coreObject);
                    }
                }

                // unlock/open: direct mechanisms...
                if (_use < 2)
                {
                    var _mechs = coreObject.Connected.OfType<Mechanism>().ToList();
                    foreach (var _mech in _mechs)
                    {
                        // process locks and fasteners
                        switch (_mech)
                        {
                            case Keyhole _keyhole:
                                {
                                    _keyhole.UnsecureLock(_actor, _powerSource, true);
                                }
                                break;

                            case LockKnob _lockKnob:
                                {
                                    apply.AppendFollowing(
                                        new StartOpenCloseStep(
                                            apply.Process, _lockKnob, _actor, _powerSource, 1));
                                }
                                break;

                            case PadLock _padLock:
                                {
                                    _padLock.UnsecureLock(_actor, _powerSource, true);
                                    Drop.DoDropEject(coreObject, _padLock);
                                }
                                break;

                            case Hasp _hasp:
                                {
                                    // remove anything fastening the hasp
                                    var _hl = _hasp.Connected.OfType<PadLock>().FirstOrDefault();
                                    if (_hl != null)
                                    {
                                        _hl.UnsecureLock(_actor, _powerSource, true);
                                        Drop.DoDropEject(coreObject, _hl);
                                    }

                                    // then open the hasp
                                    apply.AppendFollowing(
                                       new StartOpenCloseStep(
                                           apply.Process, _hasp, _actor, _powerSource, 1));
                                }
                                break;

                            case ThrowBolt _throwBolt:
                                {
                                    apply.AppendFollowing(
                                       new StartOpenCloseStep(
                                           apply.Process, _throwBolt, _actor, _powerSource, 1));
                                }
                                break;

                            default:
                                // TODO: LockActivationMechanism
                                break;
                        }

                        // increase usage count
                        _use++;
                    }

                    foreach (var _mech in _mechs)
                    {
                        // process openers
                        switch (_mech)
                        {
                            case RelockingOpener _relocker:
                                {
                                    apply.AppendFollowing(
                                       new StartOpenCloseStep(
                                           apply.Process, _relocker.Openable, _actor, _powerSource, 1));
                                }
                                break;

                            case OpenerCloser _openCloser:
                                {
                                    apply.AppendFollowing(
                                       new StartOpenCloseStep(
                                           apply.Process, _openCloser.Openable, _actor, _powerSource, 1));
                                }
                                break;

                            default:
                                // TODO: openCloseTriggerable?: trigger activates open/close
                                break;
                        }

                        // increase usage count
                        _use++;
                    }
                }

                // open (try to anyway...); portals, container-items, or "first" compartment on furnishing
                if (coreObject is Furnishing _furnish)
                {
                    var _compartment = _furnish.Connected.OfType<IOpenable>().FirstOrDefault();
                    if (_compartment != null)
                    {
                        apply.AppendFollowing(
                          new StartOpenCloseStep(
                              apply.Process, _compartment, _actor, _powerSource, 1));
                    }
                }
                else
                {
                    if (coreObject is IOpenable _openable)
                    {
                        apply.AppendFollowing(
                          new StartOpenCloseStep(
                              apply.Process, _openable, _actor, _powerSource, 1));
                    }
                }
            }
            #endregion

            if ((apply.DeliveryInteraction.Target is PortalBase _portal) && _portal.OpenState.IsClosed)
            {
                // if a portal
                if (_maxArea >= _portal.Area)
                {
                    // apply knock
                    _applyKnock(_portal);
                }
            }
            else if (apply.DeliveryInteraction.Target is Furnishing _furnish)
            {
                // furnishing with openable compartments
                var _openable = _furnish.Connected.OfType<IOpenable>().ToList();
                if (_openable.Any())
                {
                    if ((_maxArea > _furnish.Width * _furnish.Height)
                        && (_maxArea > _furnish.Length * _furnish.Height)
                        && (_maxArea > _furnish.Length * _furnish.Width))
                    {
                        // apply knock to furnishing
                        _applyKnock(_furnish);
                    }
                }
            }
            else if (apply.DeliveryInteraction.Target is ContainerItemBase _container)
            {
                // container items: apply knock
                _applyKnock(_container);
            }
            else if (apply.DeliveryInteraction.Target is SlottedContainerItemBase _slottedContainer)
            {
                // container items: apply knock
                _applyKnock(_slottedContainer);
            }
            else if (apply.DeliveryInteraction.Target is CloseableContainerObject _closeable)
            {
                _applyKnock(_closeable);
            }
        }
        #endregion

        #region IRegionMode Members
        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
        {
            yield return Convert.ToDouble(10 * casterLevel);
            yield break;
        }
        #endregion
    }

    /// <summary>Suppresses arcane lock for 10 minutes</summary>
    [Serializable]
    public class KnockSuppression : Adjunct, ITrackTime
    {
        /// <summary>Suppresses arcane lock for 10 minutes</summary>
        public KnockSuppression()
            : base(typeof(KnockSuppression))
        {
        }

        #region data
        private double? _EndTime;
        private List<MagicPowerEffect> _Durables;
        #endregion

        public override object Clone()
            => new KnockSuppression();

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);

            // capture end-time first time activated (should be only time)
            _EndTime ??= (Anchor?.GetCurrentTime() ?? 0) + (Minute.UnitFactor * 10);

            // capture arcane locks first time activated (should be only time)
            _Durables ??= MagicPowerEffect.GetMagicPowerEffects<ArcaneLockEffect>(Anchor)
                .Where(_dme => _dme.IsActive).ToList();

            foreach (var _dme in _Durables)
            {
                _dme.IsActive = false;
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            if (_Durables != null)
            {
                // re-enable any durable magic effect we disabled
                foreach (var _dme in _Durables)
                {
                    if (_dme.Anchor == Anchor)
                    {
                        _dme.IsActive = true;
                    }
                }
                _Durables.Clear();
                _Durables = null;
            }
            base.OnDeactivate(source);
        }
        #endregion

        public double Resolution => Minute.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _EndTime) && (direction == TimeValTransition.Entering))
            {
                Eject();
            }
        }
    }
}
