using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class ConveyanceRotateHandler : IInteractHandler
    {
        private static ConveyanceRotateHandler _Handler = new ConveyanceRotateHandler();

        public static ConveyanceRotateHandler Static
            => _Handler;

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(ConveyanceRotateData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet?.InteractData is ConveyanceRotateData _rotate)
            {
                if (workSet.Target is Conveyance _conveyance)
                {
                    var _orient = _conveyance.Orientation;

                    var _furnishLoc = _conveyance.GetLocated()?.Locator;
                    var _actorLoc = workSet?.Actor?.GetLocated()?.Locator;

                    if ((_furnishLoc?.GeometricRegion != null) && (_actorLoc?.GeometricRegion != null))
                    {
                        // difference between _fHeading and _orientation.Heading 
                        // determines rotation strategies for various directions
                        var _fHeading = _furnishLoc.GetGravityFace().GetOrthoHeadingVector(
                            _actorLoc.GeometricRegion.GetPoint3D(),
                            _furnishLoc.GeometricRegion.GetPoint3D());
                        int _getRel(int rel)
                        {
                            var _out = rel % 8;
                            if (_out < 0)
                            {
                                _out += 8;
                            }

                            return _out;
                        };
                        var _rel = _getRel(_fHeading - _orient.Heading);
                        switch (_rotate.Direction)
                        {
                            case @"Push":
                            case @"Pull":
                            case @"Clock":
                            case @"CounterClock":
                                switch (_rotate.Direction)
                                {
                                    case @"Pull":
                                        _rel = _getRel(_rel + 4);
                                        break;
                                    case @"Clock":
                                        _rel = _getRel(_rel - 2);
                                        break;
                                    case @"CounterClock":
                                        _rel = _getRel(_rel + 2);
                                        break;
                                };
                                switch (_orient.Upright)
                                {
                                    case Verticality.OnSideTopOut:
                                        switch (_rel)
                                        {
                                            case 1: _orient.SetOrientation(null, _orient.Twist - 1, null); break;
                                            case 2: _orient.SetOrientation(Verticality.Upright, null, null); break;
                                            case 3: _orient.SetOrientation(null, _orient.Twist + 1, null); break;
                                            default: _orient.SetOrientation(Verticality.Inverted, null, null); break;
                                        }
                                        break;

                                    case Verticality.Inverted:
                                        switch (_rel)
                                        {
                                            case 1: _orient.SetOrientation(Verticality.OnSideTopOut, 3, _orient.Heading - 1); break;
                                            case 2: _orient.SetOrientation(Verticality.OnSideTopOut, null, null); break;
                                            case 3: _orient.SetOrientation(Verticality.OnSideTopOut, 1, _orient.Heading + 1); break;
                                            default: _orient.SetOrientation(Verticality.OnSideTopOut, 2, _orient.Heading + 2); break;
                                        }
                                        break;

                                    case Verticality.Upright:
                                    default:
                                        switch (_rel)
                                        {
                                            case 1: _orient.SetOrientation(Verticality.OnSideTopOut, 3, _orient.Heading + 1); break;
                                            case 2: _orient.SetOrientation(Verticality.OnSideTopOut, 2, _orient.Heading + 2); break;
                                            case 3: _orient.SetOrientation(Verticality.OnSideTopOut, 1, _orient.Heading - 1); break;
                                            default: _orient.SetOrientation(Verticality.OnSideTopOut, null, null); break;
                                        }
                                        break;
                                }
                                break;

                            case @"Left":
                                _orient.SetOrientation(null, null, _orient.Heading - 1);
                                break;

                            case @"Right":
                                _orient.SetOrientation(null, null, _orient.Heading + 1);
                                break;
                        }

                        // signal redraw for the locator
                        _furnishLoc.RefreshAppearance(_furnishLoc.PlanarPresence);
                    }
                }
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
