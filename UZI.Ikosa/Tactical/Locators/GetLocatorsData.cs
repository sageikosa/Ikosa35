using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>MovementBase is source of interaction</summary>
    public class GetLocatorsData : InteractData
    {
        /// <summary>MovementBase is source of interaction</summary>
        public GetLocatorsData(CoreActor actor, StepDestinationTarget stepDest, int stepIndex)
            : base(actor)
        {
            _Gathered = new ConcurrentDictionary<Guid, GetLocatorsResult>();
            _StepDest = stepDest;
            _StepIdx = stepIndex;
        }

        #region data
        private ConcurrentDictionary<Guid, GetLocatorsResult> _Gathered;
        private static GetLocatorsHandler _Static = new GetLocatorsHandler();
        private StepDestinationTarget _StepDest;
        public int _StepIdx;
        #endregion

        public ConcurrentDictionary<Guid, GetLocatorsResult> Gathered => _Gathered;
        public StepDestinationTarget StepDestinationTarget => _StepDest;
        public int StepIndex => _StepIdx;

        public void SetCost(ICoreObject coreObj, double cost)
        {
            if (Gathered.TryGetValue(coreObj?.ID ?? Guid.Empty, out GetLocatorsResult _result))
            {
                _result.MoveCost = cost;
                _result.IsExtraWeight = true;
                Gathered.AddOrUpdate(coreObj.ID, _result, (id, rslt) => _result);
            }
        }

        public double? GetCost(ICoreObject coreObj)
        {
            if (Gathered.TryGetValue(coreObj?.ID ?? Guid.Empty, out GetLocatorsResult _result))
            {
                return _result.MoveCost;
            }
            return null;
        }

        /// <summary>True if added, false if already exists</summary>
        public bool Add(GetLocatorsResult result)
            => _Gathered.TryAdd(result.Locator.ICore.ID, result);

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Static;
            yield break;
        }

        /// <summary>gets locators for the coreObject using this GetLocatorsData context</summary>
        public void AddObject(ICoreObject coreObj, MovementBase movement)
        {
            var _gldi = new Interaction(coreObj as CoreActor, movement, coreObj, this);
            coreObj?.HandleInteraction(_gldi);
        }

        /// <summary>Performs interaction to get points and returns feedback.  MovementBase is source of interaction</summary>
        public static IEnumerable<GetLocatorsResult> GetLocators(CoreObject coreObj, MovementBase movement,
            StepDestinationTarget stepDest, int stepIndex)
        {
            var _gld = new GetLocatorsData(coreObj as CoreActor, stepDest, stepIndex);
            var _gldi = new Interaction(coreObj as CoreActor, movement, coreObj, _gld);
            coreObj.HandleInteraction(_gldi);
            return _gld.Gathered.Select(_kvp => _kvp.Value);
        }
    }
}
