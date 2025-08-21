using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class LiquidFallingStep : RelocationStep
    {
        #region ctor
        public LiquidFallingStep(CoreStep predecessor, Locator locator, IGeometricRegion targetRegion,
            LiquidFallMovement liquidFallMove, AnchorFaceList crossings, AnchorFace baseFace)
            : base(predecessor, locator, targetRegion, liquidFallMove, new Vector3D(), crossings, baseFace)
        {
            // we've already fallen 1 cell, so now need to apply damages
            _ApplyDamage = (LiquidFallMovement.FallTrack == 1);
            if (_ApplyDamage)
            {
                var _fall = LiquidFallMovement.FallMovement;
                _fall.FallReduce += 2;
                _fall.MaxNonLethal += 2;
                _fall.IsSofterNonLethal = true;
                if (LiquidFallMovement.FallMovement.NonLethalRoller == null)
                {
                    // if non-lethal is negated, no need to roll for damage
                    _ApplyDamage = false;
                }
            }

            // step 0
            _DamageStep = 0;
        }
        #endregion

        #region private data
        private bool _ApplyDamage;
        private int _DamageStep;
        #endregion

        public override string Name => @"Falling through liquid";
        public LiquidFallMovement LiquidFallMovement => Movement as LiquidFallMovement;

        #region protected override StepPrerequisite OnNextPrerequisite()
        protected override StepPrerequisite OnNextPrerequisite()
        {
            if (_ApplyDamage)
            {
                switch (_DamageStep)
                {
                    case 0:
                        // non lethal damage roll (by game master)
                        _DamageStep++;
                        var _nonLethal = LiquidFallMovement.FallMovement.NonLethalRoller;
                        if (_nonLethal != null)
                        {
                            if (LiquidFallMovement.FallMovement.DamageRoller == null)
                            {
                                // step past lethal damage, since there won't be any
                                _DamageStep++;
                            }
                            return new RollPrerequisite(this, @"Fall.Damage.Soft", @"Non-Lethal Falling Damage",
                                _nonLethal, false);
                        }
                        break;

                    case 1:
                        // damage roll (by game master)
                        _DamageStep++;
                        var _lethal = LiquidFallMovement.FallMovement.DamageRoller;
                        if (_lethal != null)
                        {
                            return new RollPrerequisite(this, @"Fall.Damage.Hard", @"Falling Damage",
                                _lethal, false);
                        }
                        break;
                }
            }
            return null;
        }
        #endregion

        public override bool IsDispensingPrerequisites
        {
            get
            {
                // true if we're supposed to apply damage, but haven't stepped past the end
                return (_ApplyDamage && (_DamageStep < 2));
            }
        }

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            // track intervals
            LiquidFallMovement.AddInterval(0.5d);

            if (_ApplyDamage)
            {
                // get prerequisites
                var _dmgPre = AllPrerequisites<RollPrerequisite>(@"Fall.Damage.Hard").FirstOrDefault();
                var _nonPre = AllPrerequisites<RollPrerequisite>(@"Fall.Damage.Soft").FirstOrDefault();

                // get status messages
                var _msg = new List<string>();
                if (_dmgPre != null)
                {
                    _msg.Add(string.Format(@"Damage: {0}", _dmgPre.RollValue));
                }
                if (_nonPre != null)
                {
                    _msg.Add(string.Format(@"Non-Lethal: {0}", _nonPre.RollValue));
                }

                // must have accumulated fall dice, and not reducing, no need to 
                foreach (var _interact in Locator.ICoreAs<IInteract>())
                {
                    // nonlethal damage
                    if (_nonPre != null)
                    {
                        var _non = new DamageData(_nonPre.RollValue, true, @"Fall", 0);
                        var _deliver = new DeliverDamageData(null, _non.ToEnumerable(), false, false);
                        _interact.HandleInteraction(new Interaction(null, this, _interact, _deliver));
                    }

                    // damage
                    if (_dmgPre != null)
                    {
                        var _dmg = new DamageData(_dmgPre.RollValue, false, @"Fall", 0);
                        var _deliver = new DeliverDamageData(null, _dmg.ToEnumerable(), false, false);
                        _interact.HandleInteraction(new Interaction(null, this, _interact, _deliver));
                    }
                }
            }

            // must do the base before calling NextRegion, since it updates location
            var _return = base.OnDoStep();
            if (_return)
            {
                // see if next cube will block transit movement, 
                var _region = LiquidFallMovement.NextRegion();
                var _critter = LiquidFallMovement.CoreObject as Creature;

                // if not, continue fall
                if (_region != null)
                {
                    if (LiquidFallMovement.MaxFall > LiquidFallMovement.FallTrack)
                    {
                        // continue liquid fall
                        var _grav = Locator.GetGravityFace();
                        new LiquidFallingStep(this, Locator, _region, LiquidFallMovement,
                            AnchorFaceListHelper.Create(_grav), _grav);
                    }
                    else
                    {
                        // start sinking
                        if (_critter != null)
                        {
                            EnqueueNotify(new BadNewsNotify(_critter.ID, @"Movement", new Description(@"Liquid Falling", @"going to sink")),
                                _critter.ID);
                        }

                        AppendFollowing(new SinkingStartStep(Process, Locator, 5));
                    }
                }
                else
                {
                    // hypothetical falling movement to check
                    var _fall = new FallMovement(LiquidFallMovement.CoreObject, this, 500, 0, 0);
                    var _fallRegion = _fall.NextRegion();
                    if (_fallRegion != null)
                    {
                        // start falling again
                        if (_critter != null)
                        {
                            EnqueueNotify(new BadNewsNotify(_critter.ID, @"Movement", new Description(@"Liquid Falling", @"fell out of liquid")),
                                _critter.ID);
                        }

                        new FallingStartStep(this, Locator, 500, 0, 1);
                    }
                    else
                    {
                        // liquid falling stop step
                        new LiquidFallingStopStep(this, Locator, LiquidFallMovement);
                    }
                }
            }
            return base.OnDoStep();
        }
        #endregion
    }
}
