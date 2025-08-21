using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class ObjectEffectiveEnergyDamageFactors : Adjunct, IInteractHandler
    {
        public ObjectEffectiveEnergyDamageFactors(object source, params (EnergyType energy, decimal factor)[] energies)
            : base(source)
        {
            _Factors = [];
            foreach (var _e in energies)
            {
                _Factors.Add(_e.energy, _e.factor);
            }
        }

        #region data
        private Dictionary<EnergyType, decimal> _Factors;
        #endregion

        public IEnumerable<(EnergyType energy, decimal factor)> Factors
            => _Factors.Select(_kvp => (_kvp.Key, _kvp.Value)).ToList();

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as IInteractHandlerExtendable)?.AddIInteractHandler(this);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as IInteractHandlerExtendable)?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new ObjectEffectiveEnergyDamageFactors(Source, Factors.ToArray());

        #region IInteractHandler
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(ObjectEffectiveEnergyDamage);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet?.InteractData is ObjectEffectiveEnergyDamage _dmg)
            {
                // 1 or a higher result
                int _result(decimal factor)
                    => Math.Max(1, (int)Math.Ceiling(_dmg.Damage * factor));

                if (_Factors.ContainsKey(_dmg.EnergyType))
                {
                    workSet.Feedback.Add(new ValueFeedback<int>(this, _result(_Factors[_dmg.EnergyType])));
                }
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => true;
        #endregion
    }
}
