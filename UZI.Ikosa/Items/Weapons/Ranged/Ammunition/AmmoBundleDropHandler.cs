using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class AmmoBundleDropHandler : IInteractHandler
    {
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(Drop);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet?.InteractData is Drop _dropData)
                && (workSet?.Target is IAmmunitionBundle _dropBundle))
            {
                var _map = _dropData.Map;
                if (_map != null)
                {
                    var (_trove, _locator, _ethereal) = Trove.GetTrove(_dropData, _map, _dropBundle);

                    // add the object to the trove
                    if (_trove != null)
                    {
                        // look for ammoSet compatible with _ammo ...
                        var _bundle = _trove.Objects.OfType<IAmmunitionBundle>()
                            .Where(_ab => !(_ab is IAmmunitionContainer))
                            .FirstOrDefault(_ab => (_ab.AmmunitionType == _dropBundle.AmmunitionType)
                            && (_ab.ItemSizer.ExpectedCreatureSize.Order == _dropBundle.ItemSizer.ExpectedCreatureSize.Order));
                        if (_bundle != null)
                        {
                            // merge sets dropped into bundle in trove
                            foreach (var _set in _dropBundle.AmmoSets.ToList())
                            {
                                var (ammo, count) = _bundle.MergeAmmo((_set.Ammunition, _set.Count));
                                _set.Count = count;
                            }
                            // TODO: GenerateImpactSound(...);
                        }
                        else
                        {
                            // add bundle to trove
                            _trove.Add(_dropBundle);
                            // TODO: GenerateImpactSound(...);
                        }
                    }

                    if (_locator != null)
                    {
                        var _maxSpeed = 500;
                        var _reduce = (_dropData.DropGently ? 0.5 : 0);
                        if (workSet.Target is IObjectBase _ob)
                        {
                            _reduce += _ob.FallReduce;
                            _maxSpeed = _ob.MaxFallSpeed;
                        }
                        else if (workSet.Target is IItemBase _ib)
                        {
                            _reduce += _ib.FallReduce;
                            _maxSpeed = _ib.MaxFallSpeed;
                        }
                        FallingStartStep.StartFall(_locator, _maxSpeed, _maxSpeed, @"Dropped", _reduce);
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
