using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class LightSensitivity : TraitEffect, IInteractHandler
    {
        public LightSensitivity(ITraitSource traitSource)
            : base(traitSource)
        {
            _BeDazzled = new Delta(-1, typeof(Dazzled), @"Dazzled");
        }

        private Delta _BeDazzled;

        // hook into interaction stream
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as Creature)?.AddIInteractHandler(this);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as Creature)?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }

        // cloning
        public override object Clone()
            => new LightSensitivity(TraitSource);

        public override TraitEffect Clone(ITraitSource traitSource)
            => new LightSensitivity(traitSource);

        // IInteractHandler
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(ApplyLight);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            var _apply = (workSet?.InteractData as ApplyLight);
            if (_apply != null)
            {
                var _critter = Anchor as Creature;
                if (_critter != null)
                {
                    if (_apply.Range >= Tactical.LightRange.VeryBright)
                    {
                        // apply dazzled if not already applied from this source
                        var _dazzled = new Condition(Condition.Dazzled, this);
                        _critter.Conditions.Add(_dazzled);
                        if (_critter.Conditions.Contains(_dazzled))
                        {
                            _critter.BaseAttack.Deltas.Add(_BeDazzled);
                            _critter.Skills[typeof(SearchSkill)].Deltas.Add(_BeDazzled);
                            _critter.Skills[typeof(SpotSkill)].Deltas.Add(_BeDazzled);
                        }
                    }
                    else
                    {
                        // remove dazzled from this source
                        var _dazzled = _critter.Conditions[Condition.Dazzled, this];
                        _BeDazzled.DoTerminate();
                        _critter.Conditions.Remove(_dazzled);
                    }
                }
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
