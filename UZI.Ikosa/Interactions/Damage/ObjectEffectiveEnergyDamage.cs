using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class ObjectEffectiveEnergyDamage : InteractData
    {
        public ObjectEffectiveEnergyDamage(EnergyType energyType, int damage)
            : base(null)
        {
            _Energy = energyType;
            _Damage = damage;
        }

        #region data
        private EnergyType _Energy;
        private int _Damage;
        private static ObjectEffectiveEnergyDamageHandler _Static = new ObjectEffectiveEnergyDamageHandler();
        #endregion

        public EnergyType EnergyType => _Energy;
        public int Damage => _Damage;

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Static;
            yield break;
        }

        /// <summary>If no overriding handlers, the defaults will be used</summary>
        public int GetEffectiveEnergyDamage(Interaction damageInteract)
        {
            var _interaction = new Interaction(damageInteract.Actor, damageInteract.Source, damageInteract.Target, this);
            damageInteract.Target.HandleInteraction(_interaction);
            return _interaction.Feedback.OfType<ValueFeedback<int>>().FirstOrDefault()?.Value ?? 0;
        }
    }

    [Serializable]
    public class ObjectEffectiveEnergyDamageHandler : IInteractHandler
    {
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
                int _result(decimal divisor)
                    => Math.Max(1, (int)Math.Ceiling(_dmg.Damage / divisor));

                switch (_dmg.EnergyType)
                {
                    case EnergyType.Electric:
                    case EnergyType.Fire:
                        workSet.Feedback.Add(new ValueFeedback<int>(this, _result(2m)));
                        break;

                    case EnergyType.Cold:
                        workSet.Feedback.Add(new ValueFeedback<int>(this, _result(4m)));
                        break;

                    case EnergyType.Positive:
                    case EnergyType.Negative:
                    case EnergyType.Acid:
                    case EnergyType.Sonic:
                    case EnergyType.Force:
                    case EnergyType.Unresistable:
                    default:
                        workSet.Feedback.Add(new ValueFeedback<int>(this, _dmg.Damage));
                        break;
                }
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
