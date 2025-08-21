using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class ObjectSaveHandler : IInteractHandler
    {
        public static readonly ObjectSaveHandler Static = new ObjectSaveHandler();

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(SaveableDamageData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet?.InteractData is SaveableDamageData _saveDamage)
            {
                if (workSet.Target is IStructureDamage _struc)
                {
                    if (_saveDamage.Success(workSet))
                    {
                        foreach (var _dmg in _saveDamage.Damages)
                        {
                            // subtract from each damage
                            _dmg.Amount -= Convert.ToInt32(_dmg.Amount * _saveDamage.SaveFactor);
                        }
                    }
                }
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // land above ObjectDamageHandler
            if (existingHandler.GetType() == typeof(ObjectDamageHandler))
            {
                return true;
            }

            return false;
        }
    }
}
