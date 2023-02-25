using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    public class AmmoDropHandler : IInteractHandler
    {
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(Drop);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet?.InteractData is Drop _dropData)
                && (workSet?.Target is IAmmunitionBase _ammo))
            {
                var _map = _dropData.Map;
                if (_map != null)
                {
                    var (_trove, _locator, _ethereal) = Trove.GetTrove(_dropData, _map, _ammo);

                    // add the object to the trove
                    if (_trove != null)
                    {
                        // look for ammoSet (but not bundle) compatible with _ammo ...
                        var _bundle = _trove.Objects.OfType<IAmmunitionBundle>()
                            .Where(_ab => !(_ab is IAmmunitionContainer))
                            .FirstOrDefault(_ab => (_ab.AmmunitionType == _ammo.GetType())
                            && (_ab.ItemSizer.ExpectedCreatureSize.Order == _ammo.ItemSizer.ExpectedCreatureSize.Order));
                        if (_bundle != null)
                        {
                            _bundle.MergeAmmo((_ammo, 1));
                            // TODO: GenerateImpactSound(...);
                        }
                        else
                        {
                            _trove.Add(_ammo.ToAmmunitionBundle(@"Bundle"));
                            // TODO: GenerateImpactSound(...);
                        }
                    }

                    if (_locator != null)
                    {
                        var _maxSpeed = _ammo.MaxFallSpeed;
                        FallingStartStep.StartFall(_locator, _maxSpeed, _maxSpeed, @"Dropped",
                            (_dropData.DropGently ? 0.5 : 0) + _ammo.FallReduce);
                    }
                }

                // ensure we don't drop using default DropHandler (even if this all fails)
                workSet.Feedback.Add(new UnderstoodFeedback(this));
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // pre-empt DropHandler
            return existingHandler is DropHandler;
        }
    }
}
