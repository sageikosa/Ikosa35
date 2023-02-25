using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class EtherealState : Adjunct, IInteractHandler
    {
        // TODO: !!!
        public EtherealState()
            : base(typeof(EtherealState))
        {
        }

        /// <summary>If no active EtherealEffect left on object, will eject the EtherealState</summary>
        public void EjectIfNotEthereal()
        {
            // check for other ethereal effects, eject if none
            if (!(Anchor?.Adjuncts.OfType<EtherealEffect>().Any(_e => _e.IsActive) ?? false))
            {
                Eject();
            }
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as CoreObject).AddIInteractHandler(this);

            if (Anchor is Creature _critter)
            {
                // senses and movements for creatures
                _critter.AddAdjunct(new EtherealMoveManager(this));
                _critter.Senses.Add(new EtherealVision(60, this));
                // TODO: ethereal sound also limited to 60'
            }

            // find locator
            var _located = Anchor?.GetLocated();
            var _locator = _located?.Locator;
            if (_locator != null)
            {
                // make sure locator not already ethereal
                if (_locator.PlanarPresence != PlanarPresence.Ethereal)
                {
                    // is the locator specifically for this anchor?
                    if (_locator.ICore == Anchor)
                    {
                        // move locator to ethereal
                        _locator.Relocate(_locator.GeometricRegion, PlanarPresence.Ethereal);
                        _located.RefreshPath();
                    }
                    else
                    {
                        // no, so drop object into ethereal
                        if ((Anchor is ICoreObject _obj)
                            && (_locator.ICore is ICoreObject _src))
                        {
                            // unpath the object
                            _obj.UnPath();

                            // drop object (it will find/make a locator with the appropriate planar presence)
                            Drop.DoDropEject(_src, _obj);
                        }
                    }
                }
            }

            // TODO: other group adjuncts that need to be terminated...

            // TODO: ethereal listeners can hear material sources
            // TODO: material listeners cannot hear ethereal sources
        }

        #region private IEnumerable<ICoreObject> GetEjectors(ICoreObject coreObject)
        /// <summary>Look for EtherealStates among connected object, yield the "surface" adjuncts</summary>
        private IEnumerable<ICoreObject> GetEjectors(ICoreObject coreObject)
        {
            if (coreObject != null)
            {
                foreach (var _cObj in coreObject.Connected)
                {
                    var _es = _cObj.Adjuncts.OfType<EtherealState>().FirstOrDefault();
                    if (_es != null)
                    {
                        // if object has ethereal state, no deeper inspection
                        yield return _cObj;
                    }
                    else
                    {
                        // otherwise deeper inspection
                        foreach (var _deep in GetEjectors(_cObj))
                        {
                            yield return _deep;
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // done with ethereal move and senses
                _critter.Senses.Remove(_critter.Senses.AllSenses.OfType<EtherealVision>().FirstOrDefault(_ev => _ev.Ethereal == this));
            }

            // remove ethereal move manager
            Anchor.Adjuncts.OfType<EtherealMoveManager>().FirstOrDefault(_emm => _emm.Ethereal == this)?.Eject();

            var _located = Anchor?.GetLocated();
            var _locator = _located?.Locator;
            if (_locator?.ICore is ICoreObject _src)
            {
                if (_locator.ICore == Anchor)
                {
                    // find things that won't come along for the ride
                    var _ejectors = GetEjectors(Anchor as ICoreObject).ToList();
                    foreach (var _subEther in _ejectors)
                    {
                        // unpath the object
                        _subEther.UnPath();
                    }

                    // TODO: find a geometric region suitable for re-entry, damage if necessary

                    // move locator to non-ethereal
                    _locator.Relocate(_locator.GeometricRegion, PlanarPresence.Material);
                    _located.RefreshPath();

                    foreach (var _subEther in _ejectors)
                    {
                        // and cast the ejectors into the ether
                        Drop.DoDropEject(_src, _subEther);
                    }
                }
                else
                {
                    // nothing else holding it ethereal
                    if (_locator.PlanarPresence == PlanarPresence.Ethereal)
                    {
                        // move object to own locator in non-ethereal (if locator itself is ethereal)
                        if (Anchor is ICoreObject _obj)
                        {
                            // find things that won't come along for the ride
                            var _ejectors = GetEjectors(_obj).ToList();
                            foreach (var _subEther in _ejectors)
                            {
                                // unpath the anchor of the ethereal state
                                _subEther.UnPath();
                            }

                            // unpath the object
                            _obj.UnPath();

                            // TODO: find valid entry location

                            // drop object (it will find/make a locator with the appropriate planar presence)
                            Drop.DoDropFromEthereal(_locator.Location, _src, _obj);

                            // find things that won't come along for the ride
                            foreach (var _subEther in _ejectors)
                            {
                                // and cast it into the ether
                                Drop.DoDropFromEthereal(_locator.Location, _src, _subEther);
                            }
                        }
                    }
                }
            }

            (Anchor as CoreObject).RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }

        //private (IGeometricRegion region, Vector3D offset) DoPostEtherealShunt(IGeometricRegion region)
        //{
        //    // TODO: if positioning can partially succeed, consider damage and adjust
        //    // TODO: if position fails, find cells in current rooms/backgrounds nearby where it can succeed
        //    // TODO: if necessary, move to other rooms/background

        //    // TODO: consider an interface to slap over things for this...
        //    switch (ICore)
        //    {
        //        case IObjectBase _obj:
        //            // TODO: based on FurnishingVolume/ConveyanceVolume (abstracted through IObjectBase?)
        //            break;

        //        case IItemBase _item:
        //            // TODO: based on having a singular cell large enough (abstracted through IItemBase?)
        //            break;

        //        case Creature _critter:
        //            // TODO: use land/flight/swim movement to try and squeeze into cells ("abstracted" through Creature)
        //            break;

        //        default:
        //            break;
        //    }
        //    return (region, IntraModelOffset);
        //}

        public override object Clone()
            => new EtherealState();

        // IInteractHandler
        public void HandleInteraction(Interaction workSet)
        {
            var _target = workSet?.Target as ICoreObject;
            switch (workSet?.InteractData)
            {
                case AttackData _atk:
                    break;

                default:
                    break;
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            // TODO: other damage/interactions...
            yield return typeof(MeleeAttackData);
            yield return typeof(ReachAttackData);
            yield return typeof(RangedAttackData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => true;
    }
}
