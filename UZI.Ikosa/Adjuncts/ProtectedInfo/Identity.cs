using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Represents information and adjunct exposure status for its anchor.
    /// Does not guarantee the information and adjuncts remain valid, as object
    /// info and adjuncts can shift due to in-game actions, and knowledge doesn't
    /// automatically shift to follow.
    /// </summary>
    [Serializable]
    public class Identity : Adjunct, IExposeProtectable, IInteractHandler, IActionProvider, IProcessFeedback
    {
        #region ctor()
        /// <summary>
        /// Represents information and adjunct exposure status for its anchor.
        /// Does not guarantee the information and adjuncts remain valid, as object
        /// info and adjuncts can shift due to in-game actions, and knowledge doesn't
        /// automatically shift to follow.
        /// </summary>
        public Identity(object source)
            : base(source)
        {
            _InfoID = Guid.NewGuid();
            _Title = @"Identity";
            _Users = new HashSet<Guid>();
            _Critters = new Dictionary<Guid, string>();
            _Adjuncts = new List<Adjunct>();
            _Infos = new List<Info>();
        }
        #endregion

        #region data
        private Guid _InfoID;
        private string _Title;
        private HashSet<Guid> _Users;
        private Dictionary<Guid, string> _Critters;
        private List<Adjunct> _Adjuncts;
        private List<Info> _Infos;
        #endregion

        public override Guid? MergeID => _InfoID;
        public string Title { get { return _Title; } set { _Title = value; } }
        public HashSet<Guid> Users
        {
            get
            {
                if (_Users == null)
                {
                    _Users = new HashSet<Guid>();
                    foreach (var _kvp in CreatureIDs)
                    {
                        _Users.Add(_kvp.Key);
                    }
                }
                return _Users;
            }
        }
        public Dictionary<Guid, string> CreatureIDs => _Critters;
        public IEnumerable<Adjunct> Adjuncts => _Adjuncts.Select(_a => _a);
        public IEnumerable<Info> Infos => _Infos.Select(_i => _i);
        public override bool IsProtected => true;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);

            // add IInteract handler to anchor
            if (Anchor is CoreObject _core)
            {
                _core.AddIInteractHandler(this);
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            // remove IInteract handler from anchor
            if (Anchor is CoreObject _core)
            {
                _core.RemoveIInteractHandler(this);
            }
            base.OnDeactivate(source);
        }
        #endregion

        #region public override object Clone()
        public override object Clone()
        {
            var _id = new Identity(Source);
            foreach (var _critter in _Critters)
                _id._Critters.Add(_critter.Key, _critter.Value);
            _id._InfoID = _InfoID;
            _id._Adjuncts.AddRange(_Adjuncts);
            _id._Infos.AddRange(_Infos);
            return _id;
        }
        #endregion

        #region IExposeProtectable Members

        public bool DoesExpose(Adjunct target, Creature actor)
        {
            // match adjunct and actor via lists
            return (_Adjuncts.Contains(target) && _Critters.ContainsKey(actor.ID));
        }

        #endregion

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
            => new AdjunctInfo($@"Identity: {Title}", ID);

        #endregion

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet?.InteractData is GetInfoData)
            {
                // provide iInfos, but only to creatures that use this info
                if (Users.Contains(workSet.Actor.ID))
                {
                    workSet.Feedback.Add(new InfoFeedback(this, _Infos.Select(_i => _i.Clone() as Info).FirstOrDefault()));
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetIdentityData);
            yield return typeof(GetInfoData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            if (typeof(GetInfoData).Equals(interactType))
            {
                // new identity goes before other identities
                return existingHandler is Identity;
            }
            else if (typeof(GetIdentityData).Equals(interactType))
            {
                // any new identity goes on the top
                return true;
            }
            return false;
        }

        public void ProcessFeedback(Interaction workSet)
        {
            // add identity to bucket if actor is aware of it
            if (workSet?.InteractData is GetIdentityData _identityData)
            {
                if (workSet.Feedback.FirstOrDefault() is GetIdentityDataFeedback _feedback)
                {
                    if (CreatureIDs.ContainsKey(_identityData.Actor?.ID ?? Guid.Empty))
                    {
                        _feedback.Identities.Add(this);
                    }
                }
            }
        }

        #endregion

        #region public static Identity CreateIdentity(ICoreObject target, object source)
        /// <summary>Creates an identity with all IInfos and IProtectables</summary>
        public static Identity CreateIdentity(CoreActor actor, ICoreObject target, object source)
        {
            var _id = new Identity(source);

            // snapshot IInfos for items
            _id._Infos.Add(target.GetInfo(actor, false));

            // add IProtectable adjuncts
            _id._Adjuncts.AddRange(target.Adjuncts.OfType<IProtectable>().Select(_a => _a as Adjunct));

            return _id;
        }
        #endregion

        public void MergeCreatureIDs(Dictionary<Guid, string> mergeIn)
        {
            foreach (var _key in (from _k in mergeIn.Keys
                                  where !_Critters.Keys.Contains(_k)
                                  select _k).ToList())
            {
                _Critters.Add(_key, mergeIn[_key]);
            }
        }

        public IEnumerable<object> SubFolders
        {
            get
            {
                yield return new ListableList<Info>(Infos.ToList());
                yield return new ListableList<Adjunct>(Adjuncts.ToList());
                yield return new ListableList<KeyValuePair<Guid, string>>(CreatureIDs.ToList());
                yield break;
            }
        }
    }
}
