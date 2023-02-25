using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Appraisal : Adjunct, IProcessFeedback, IActionProvider
    {
        public Appraisal(object source, decimal cost)
            : base(source)
        {
            _Title = @"Appraisal";
            _Critters = new Dictionary<Guid, string>();
            _Cost = cost;
        }

        #region data
        private string _Title;
        private Dictionary<Guid, string> _Critters;
        private decimal _Cost;
        #endregion

        public string Title { get => _Title; set { _Title = value; DoPropertyChanged(@"Title"); } }
        public Dictionary<Guid, string> CreatureIDs => _Critters;
        public override bool IsProtected => true;
        public decimal Cost { get => _Cost; set { _Cost = value; DoPropertyChanged(@"Cost"); } }

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);

            // add IInteract handler to anchor
            (Anchor as CoreObject)?.AddIInteractHandler(this);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            // remove IInteract handler from anchor
            (Anchor as CoreObject)?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }
        #endregion

        public override object Clone()
            => new Appraisal(Source, Cost);

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // if actor is in the creature list, actor can add other creatures to list as an action
            if (_Critters.ContainsKey(budget.Actor.ID))
            {
                // TODO: determine action type and other conditions (form of communication, steps, prerequisites...)
                // TODO: consider a "sync" info action to synchronize sets of information
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo($@"Appraisal: {Title}", ID);
        #endregion

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            // don't really handle interactions, instead, we process feedback
            return;
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetInfoData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            if (typeof(GetInfoData).Equals(interactType))
            {
                // put at the top of the stack...
                // NOTE: multiple appraisals may be attached, but that's OK
                return true;
            }
            return false;
        }

        #endregion

        #region IProcessFeedback Members

        public void ProcessFeedback(Interaction workSet)
        {
            if ((workSet?.Feedback != null) && (workSet.Feedback.Count > 0))
            {
                if (_Critters.ContainsKey(workSet.Actor.ID))
                {
                    var _infoBack = workSet.Feedback.OfType<InfoFeedback>().FirstOrDefault();
                    if (_infoBack != null)
                    {
                        // add appraisal if creature getting the information has access to it...
                        if (_infoBack.Information is ObjectInfo _objInfo)
                        {
                            _objInfo.AdjunctInfos =
                                _objInfo.AdjunctInfos.Union(new Description(Title, string.Format(@"Cost: {0}", _Cost)).ToEnumerable()).ToArray();
                        }
                    }
                }
            }
        }

        #endregion
    }
}
