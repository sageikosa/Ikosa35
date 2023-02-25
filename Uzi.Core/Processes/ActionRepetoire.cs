using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    [Serializable]
    public class ActionRepetoire
    {
        #region Construction
        public ActionRepetoire(CoreActor actor)
        {
            Actor = actor;
            Providers = new Dictionary<object, IActionProvider>();
            Filters = new Dictionary<object, IActionFilter>();
        }
        #endregion

        public CoreActor Actor { get; private set; }

        /// <summary>All things that provide actions the actor may be able to perform</summary>
        public Dictionary<object, IActionProvider> Providers { get; private set; }

        /// <summary>All filters that may suppress available actions</summary>
        public Dictionary<object, IActionFilter> Filters { get; private set; }

        public IEnumerable<IActionProvider> GetActionProviders()
            => Providers.Select(_p => _p.Value);

        #region public IEnumerable<ActionResult> ProvidedActions(CoreActionBudget budget, IEnumerable<IActionProvider> exclusions)
        /// <summary>Provides all actions, with a reference to the IActionProvider</summary>
        public IEnumerable<ActionResult> ProvidedActions(CoreActionBudget budget, IEnumerable<IActionProvider> exclusions)
        {
            if (budget.Actor == Actor)
                foreach (var _pkvp in Providers.Where(_pvp => !exclusions.Contains(_pvp.Value)))
                    foreach (var _action in _pkvp.Value.GetActions(budget))
                        if (_action.CanPerformNow(budget).Success && !SuppressAction(budget, _pkvp.Key, _action))
                            yield return new ActionResult
                            {
                                Provider = _pkvp.Value,
                                Action = _action,
                                IsExternal = (_pkvp.Value is IExternalActionProvider)
                            };
            yield break;
        }
        #endregion

        public bool SuppressAction(CoreActionBudget budget, object source, CoreAction action)
            => Filters.Any(_fkvp => _fkvp.Value.SuppressAction(source, budget, action));
    }
}
