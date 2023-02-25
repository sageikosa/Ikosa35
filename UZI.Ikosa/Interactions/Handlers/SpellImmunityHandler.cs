using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Changes spell success to spell failure if the spell in the list of spell types (or similar to)
    /// </summary>
    [Serializable]
    public class SpellImmunityHandler : IProcessFeedback
    {
        public SpellImmunityHandler(bool similarSpells, params Type[] spellTypes)
        {
            _Similar = similarSpells;
            _SpellTypes = new Collection<Type>(spellTypes.ToList());
        }

        private bool _Similar;
        public bool ImmuneToSimilarSpells { get { return _Similar; } }

        private Collection<Type> _SpellTypes;
        public Collection<Type> SpellTypes { get { return _SpellTypes; } }

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            return;
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            // keep an eye on spells and spell-like effects...
            yield return typeof(PowerActionTransit<SpellSource>);
            yield return typeof(MagicPowerEffectTransit<SpellSource>);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // put it before the spell transit handler
            return typeof(SpellTransitHandler).IsAssignableFrom(existingHandler.GetType());
        }

        #endregion

        #region IProcessFeedback Members

        public void ProcessFeedback(Interaction workSet)
        {
            var _transit = workSet.InteractData as PowerActionTransit<SpellSource>;
            if (_transit != null)
            {
                // find transit feedback
                var _feedback = workSet.Feedback.OfType<PowerActionTransitFeedback<SpellSource>>().FirstOrDefault();
                if ((_feedback != null) && _feedback.Success)
                {
                    // look for matches
                    bool _match = false;
                    if (ImmuneToSimilarSpells)
                    {
                        _match = (from _fst in _feedback.PowerTransit.PowerSource.SpellDef.SimilarSpells
                                  join _ist in SpellTypes
                                  on _fst equals _ist
                                  select _fst).Any();
                    }
                    else
                    {
                        _match = (from _ist in SpellTypes
                                  where _ist.Equals(_feedback.PowerTransit.PowerSource.SpellDef.GetType())
                                  select _ist).Any();
                    }

                    // alter the feedback if it's a match
                    if (_match)
                    {
                        _feedback.Success = false;
                    }
                }
            }
        }

        #endregion
    }
}
