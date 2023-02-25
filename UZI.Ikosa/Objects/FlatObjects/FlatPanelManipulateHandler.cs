using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class FlatPanelManipulateHandler : IInteractHandler
    {
        private static FlatPanelManipulateHandler _Handler = new FlatPanelManipulateHandler();

        public static FlatPanelManipulateHandler Static
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
                if (workSet.Target is FlatPanel _flatObject)
                {
                    var _heading = _flatObject.Orientation.Heading;

                    var _flatObjLoc = _flatObject.GetLocated()?.Locator;
                    var _actorLoc = workSet?.Actor?.GetLocated()?.Locator;

                    if ((_flatObjLoc?.GeometricRegion != null) && (_actorLoc?.GeometricRegion != null))
                    {
                        switch (_rotate.Direction)
                        {
                            case @"Ball":
                                {
                                    if (_flatObject is FlexibleFlatPanel _flexFlat)
                                    {
                                        _flexFlat.FlexState =
                                            _flexFlat.FlexState == FlexibleFlatState.Flat
                                            ? FlexibleFlatState.Balled
                                            : FlexibleFlatState.Flat;
                                    }
                                }
                                break;

                            case @"Fold":
                                {
                                    if (_flatObject is FlexibleFlatPanel _flexFlat)
                                    {
                                        _flexFlat.FlexState =
                                            _flexFlat.FlexState == FlexibleFlatState.Flat
                                            ? FlexibleFlatState.Folded
                                            : FlexibleFlatState.Flat;
                                    }
                                }
                                break;

                            case @"Roll":
                                {
                                    if (_flatObject is FlexibleFlatPanel _flexFlat)
                                    {
                                        _flexFlat.FlexState =
                                            _flexFlat.FlexState == FlexibleFlatState.Flat
                                            ? FlexibleFlatState.Rolled
                                            : FlexibleFlatState.Flat;
                                    }
                                }
                                break;

                            case @"Flip":
                                {
                                    if (_flatObject.CanFlip)
                                    {
                                        // flip always adjusts twist
                                        _flatObject.Orientation.SetOrientation(null, _flatObject.Orientation.Twist + 2, null);
                                    }
                                    else if (_flatObject is FlexibleFlatPanel _flexFlat)
                                    {
                                        // must be in a non-flat state
                                        switch (_flexFlat.Orientation.Upright)
                                        {
                                            case Verticality.Upright:
                                                _flexFlat.Orientation.SetOrientation(Verticality.Inverted, null, null);
                                                break;

                                            case Verticality.OnSideTopOut:
                                                _flexFlat.Orientation.SetOrientation(Verticality.OnSideBottomOut, null, null);
                                                break;

                                            case Verticality.Inverted:
                                                _flexFlat.Orientation.SetOrientation(Verticality.Upright, null, null);
                                                break;

                                            case Verticality.OnSideBottomOut:
                                                _flexFlat.Orientation.SetOrientation(Verticality.OnSideTopOut, null, null);
                                                break;
                                        }
                                    }
                                }
                                break;
                        }

                        // signal redraw for the locator
                        _flatObjLoc.RefreshAppearance(_flatObjLoc.PlanarPresence);
                    }
                }
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
