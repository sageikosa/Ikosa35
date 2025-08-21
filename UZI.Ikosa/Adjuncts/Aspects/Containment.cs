using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Provides actions for container (if defined)</summary>
    [Serializable]
    public class Containment : Adjunct, IActionProvider
    {
        // TODO: liquid containment, gas containment

        /// <summary>Provides actions for container (if defined)</summary>
        public Containment(IObjectContainer source)
            : base(source)
        {
        }

        public override bool IsProtected
            => true;

        public IObjectContainer Container
            => Source as IObjectContainer;

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (Anchor is IObjectContainer _container)
            {
                if (budget.Actor is Creature _critter)
                {
                    if ((budget as LocalActionBudget)?.CanPerformBrief ?? false)
                    {
                        if (StoreObject.CanStore(_container, _critter))
                        {
                            yield return new StoreObject(_container, @"201");
                        }

                        if (RetrieveObject.CanRetrieve(_container, _critter))
                        {
                            yield return new LoadObject(_container, @"301");
                        }

                        if ((_container.Count > 0) && RetrieveObject.CanRetrieve(_container, _critter))
                        {
                            yield return new RetrieveObject(_container, @"202");
                            yield return new UnloadObject(_container, @"302");
                        }
                    }
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Containment", ID);
        #endregion

        public override object Clone()
            => new Containment(Container);
    }
}
