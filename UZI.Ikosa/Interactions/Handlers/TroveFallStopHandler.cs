using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class TroveFallStopHandler : IInteractHandler
    {
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(FallStop);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet.Target is Trove _trove)
                && (workSet.InteractData is FallStop _stop)
                && (workSet is StepInteraction _stepSet))
            {
                var _loc = _stop.Locator;

                // merge or create troves
                var _merge = (from _l in _loc.MapContext.LocatorsInRegion(_loc.GeometricRegion, _loc.PlanarPresence)
                              where (_l.ICore is Trove) && (_l.ICore != _trove)
                              select _l.ICore as Trove).FirstOrDefault();
                if (_merge != null)
                {
                    // add item to trove
                    var _items = _trove.Objects.ToList();
                    foreach (var _item in _items)
                    {
                        _trove.Remove(_item);

                        // handle raw ammo bundles differently
                        if ((_item is IAmmunitionBundle _stopBundle)
                            && !(_stopBundle is IAmmunitionContainer))
                        {
                            // look for ammoSet compatible with _ammo ...
                            var _bundle = _merge.Objects.OfType<IAmmunitionBundle>()
                                .Where(_ab => !(_ab is IAmmunitionContainer))
                                .FirstOrDefault(_ab => (_ab.AmmunitionType == _stopBundle.AmmunitionType)
                                && (_ab.ItemSizer.ExpectedCreatureSize.Order == _stopBundle.ItemSizer.ExpectedCreatureSize.Order));
                            if (_bundle != null)
                            {
                                // merge sets dropped into bundle in trove
                                foreach (var _set in _stopBundle.AmmoSets.ToList())
                                {
                                    var (_ammo, _count) = _bundle.MergeAmmo((_set.Ammunition, _set.Count));
                                    _set.Count = _count;
                                }
                            }
                            else
                            {
                                // add bundle to trove
                                _merge.Add(_stopBundle);
                            }
                        }
                        else
                        {
                            _merge.Add(_item);
                        }
                    }
                    workSet.Feedback.Add(new TroveMergeFeedback(this, _merge));
                }

                workSet.Feedback.Add(new UnderstoodFeedback(this));
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }
    }
}
