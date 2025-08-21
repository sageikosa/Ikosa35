using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class FurnishingRotateHandler : IInteractHandler
    {
        private static FurnishingRotateHandler _Handler = new FurnishingRotateHandler();

        public static FurnishingRotateHandler Static
            => _Handler;

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(ObjectManipulateData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet?.InteractData is ObjectManipulateData _rotate)
            {
                if (workSet.Target is Furnishing _furnishing)
                {
                    var _orient = _furnishing.Orientation;

                    var _furnishLoc = _furnishing.GetLocated()?.Locator;
                    var _actorLoc = workSet?.Actor?.GetLocated()?.Locator;

                    if ((_furnishLoc?.GeometricRegion != null) && (_actorLoc?.GeometricRegion != null))
                    {
                        // difference between _fHeading and _orientation.Heading 
                        // determines rotation strategies for various directions
                        var _gravity = _orient.GravityFace;
                        var _hFace = _gravity.GetHeadingFaces(_orient.Heading * 2).ToAnchorFaces().FirstOrDefault();
                        var _fHeading = _gravity.GetOrthoHeadingVector(
                            _actorLoc.GeometricRegion.GetPoint3D(),
                            _furnishLoc.GeometricRegion.GetPoint3D());
                        var _right = _gravity.RightFace(_orient.Heading * 2)
                            ?? _gravity.GetHeadingFaces(6).ToAnchorFaces().FirstOrDefault();
                        var _left = _gravity.LeftFace(_orient.Heading * 2)
                            ?? _gravity.GetHeadingFaces(2).ToAnchorFaces().FirstOrDefault();
                        int _getRel(int rel)
                        {
                            var _out = rel % 4;
                            if (_out < 0)
                            {
                                _out += 4;
                            }

                            return _out;
                        };

                        void _leftTurn()
                        {
                            // left push (to right)
                            // change snap if needed, then heading
                            if (!_orient.IsFaceSnapped(_right) && _orient.IsFaceSnapped(_left))
                            {
                                if (_orient.IsHeadingTwisted)
                                {
                                    // untwist only to flatten out object
                                    _orient.SetOrientation(null, _orient.Twist - 1, null);
                                }
                                else
                                {
                                    // snapped to left, but not right, change snap
                                    _orient.SetAxisSnap(_right.GetAxis(), !_right.IsLowFace());
                                }
                            }
                            else
                            {
                                if (_orient.IsHeadingTwisted)
                                {
                                    _orient.SetOrientation(null, _orient.Twist + 1, _orient.Heading - 1);
                                    _orient.SetAxisSnap(_hFace.GetAxis(), _hFace.IsLowFace());
                                }
                                else
                                {
                                    // snap was already at extreme for heading, so set heading
                                    _orient.SetOrientation(null, null, _orient.Heading - 1);
                                }
                            }
                        }

                        void _rightTurn()
                        {
                            // right push (to left)

                            // change snap if needed, then heading
                            if (!_orient.IsFaceSnapped(_left)
                                && _orient.IsFaceSnapped(_right))
                            {
                                if (_orient.IsHeadingTwisted)
                                {
                                    // untwist only to flatten out object
                                    _orient.SetOrientation(null, _orient.Twist + 1, null);
                                }
                                else
                                {
                                    // snapped to right, but not left, change snap
                                    _orient.SetAxisSnap(_left.GetAxis(), !_left.IsLowFace());
                                }
                            }
                            else
                            {
                                if (_orient.IsHeadingTwisted)
                                {
                                    _orient.SetOrientation(null, _orient.Twist - 1, _orient.Heading + 1);
                                    _orient.SetAxisSnap(_hFace.GetAxis(), _hFace.IsLowFace());
                                }
                                else
                                {
                                    // snap was already at extreme for heading, so set heading
                                    _orient.SetOrientation(null, null, _orient.Heading + 1);
                                }
                            }
                        }

                        void _leftTwist()
                        {
                            var _right = _gravity.RightFace(_orient.Heading * 2);
                            if (!_orient.IsFaceSnapped(_right ?? _gravity.ReverseFace()))
                            {
                                _orient.SetOrientation(null, null, _orient.Heading - 1);
                            }
                            else
                            {
                                // multiple inversions
                                _orient.SetOrientation(
                                    (Verticality)(((int)_orient.Upright + 4) % 8),
                                    _orient.Twist + 2,
                                    _orient.Heading + 1);
                            }
                        }

                        void _rightTwist()
                        {
                            var _left = _gravity.LeftFace(_orient.Heading * 2);
                            if (!_orient.IsFaceSnapped(_left ?? _gravity.ReverseFace()))
                            {
                                _orient.SetOrientation(null, null, _orient.Heading + 1);
                            }
                            else
                            {
                                // multiple inversions
                                _orient.SetOrientation(
                                    (Verticality)(((int)_orient.Upright + 4) % 8),
                                    _orient.Twist + 2,
                                    _orient.Heading - 1);
                            }
                        }

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
                                        _rel = _getRel(_rel + 2);
                                        break;
                                    case @"Clock":
                                        _rel = _getRel(_rel - 1);
                                        break;
                                    case @"CounterClock":
                                        _rel = _getRel(_rel + 1);
                                        break;
                                };
                                switch (_orient.Upright)
                                {
                                    case Verticality.OnSideBottomOut:
                                        switch (_rel)
                                        {
                                            case 1: _orient.SetOrientation(null, _orient.Twist + 1, null); break;
                                            case 2: _orient.SetOrientation(Verticality.Inverted, null, null); break;
                                            case 3: _orient.SetOrientation(null, _orient.Twist - 1, null); break;
                                            default: _orient.SetOrientation(Verticality.Upright, null, null); break;
                                        }
                                        break;

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
                                            case 1:
                                                {
                                                    // depends on snap?
                                                    if (_orient.IsFaceSnapped(_left))
                                                    {
                                                        _orient.SetOrientation(
                                                            Verticality.OnSideBottomOut,
                                                            _orient.Twist + 1,
                                                            _orient.Heading + 1);
                                                    }
                                                    else
                                                    {
                                                        _orient.SetOrientation(
                                                            Verticality.OnSideTopOut,
                                                            _orient.Twist - 1,
                                                            _orient.Heading - 1);
                                                    }
                                                }
                                                break;
                                            case 3:
                                                {
                                                    // depends on snap?
                                                    if (_orient.IsFaceSnapped(_right))
                                                    {
                                                        _orient.SetOrientation(
                                                            Verticality.OnSideBottomOut,
                                                            _orient.Twist - 1,
                                                            _orient.Heading - 1);
                                                    }
                                                    else
                                                    {
                                                        _orient.SetOrientation(
                                                            Verticality.OnSideTopOut,
                                                            _orient.Twist + 1,
                                                            _orient.Heading + 1);
                                                    }
                                                }
                                                break;
                                            case 2: _orient.SetOrientation(Verticality.OnSideTopOut, null, null); break;
                                            default: _orient.SetOrientation(Verticality.OnSideBottomOut, null, null); break;
                                        }
                                        break;

                                    case Verticality.Upright:
                                    default:
                                        switch (_rel)
                                        {
                                            case 1:
                                                {
                                                    // depends on snap?
                                                    if (_orient.IsFaceSnapped(_left))
                                                    {
                                                        _orient.SetOrientation(
                                                            Verticality.OnSideTopOut,
                                                            _orient.Twist - 1,
                                                            _orient.Heading + 1);
                                                    }
                                                    else
                                                    {
                                                        _orient.SetOrientation(
                                                            Verticality.OnSideBottomOut,
                                                            _orient.Twist + 1,
                                                            _orient.Heading - 1);
                                                    }
                                                }
                                                break;
                                            case 3:
                                                {
                                                    // depends on snap?
                                                    if (_orient.IsFaceSnapped(_right))
                                                    {
                                                        _orient.SetOrientation(
                                                            Verticality.OnSideTopOut,
                                                            _orient.Twist + 1,
                                                            _orient.Heading - 1);
                                                    }
                                                    else
                                                    {
                                                        _orient.SetOrientation(
                                                            Verticality.OnSideBottomOut,
                                                            _orient.Twist - 1,
                                                            _orient.Heading + 1);
                                                    }
                                                }
                                                break;
                                            case 2: _orient.SetOrientation(Verticality.OnSideBottomOut, null, null); break;
                                            default: _orient.SetOrientation(Verticality.OnSideTopOut, null, null); break;
                                        }
                                        break;
                                }
                                break;

                            case @"Left":
                                _leftTurn();
                                break;

                            case @"Right":
                                _rightTurn();
                                break;

                            case @"TwistLeft":
                                switch (_orient.Upright)
                                {
                                    case Verticality.Upright:
                                        _orient.SetOrientation(null, _orient.Twist - 1, null);
                                        break;
                                    case Verticality.Inverted:
                                        _orient.SetOrientation(null, _orient.Twist + 1, null);
                                        break;
                                    case Verticality.OnSideTopOut:
                                    case Verticality.OnSideBottomOut:
                                    default:
                                        _leftTwist();
                                        break;
                                }
                                break;

                            case @"TwistRight":
                                switch (_orient.Upright)
                                {
                                    case Verticality.Upright:
                                        _orient.SetOrientation(null, _orient.Twist + 1, null);
                                        break;
                                    case Verticality.Inverted:
                                        _orient.SetOrientation(null, _orient.Twist - 1, null);
                                        break;
                                    case Verticality.OnSideTopOut:
                                    case Verticality.OnSideBottomOut:
                                    default:
                                        _rightTwist();
                                        break;
                                }
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
