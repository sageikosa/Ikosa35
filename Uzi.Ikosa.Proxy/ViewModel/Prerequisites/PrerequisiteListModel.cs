using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class PrerequisiteListModel
    {
        #region ctor()
        public PrerequisiteListModel(IEnumerable<PrerequisiteInfo> preReqs, IPrerequisiteProxy preReqProxy, CreatureLoginInfo creature)
        {
            var _fulfillers = new Dictionary<Guid, CreatureLoginInfo>
            {
                { creature.ID, creature }
            };
            var _awarenesses = preReqs.OfType<CoreSelectPrerequisiteInfo>().Any()
                ? preReqProxy.GetCoreInfoAwarenesses(preReqs).ToList()
                : new List<AwarenessInfo>();
            _Items = preReqs
                .Where(_p => !(_p is ActionInquiryPrerequisiteInfo))
                .Select(_p => PrerequisiteModel.ToModel(_p, preReqProxy, _awarenesses, _fulfillers)).ToList();
            if (_Items.Count == 1)
                _Items[0].IsSingleton = true;
        }

        public PrerequisiteListModel(IEnumerable<PrerequisiteInfo> preReqs, IPrerequisiteProxy preReqProxy,
            Dictionary<Guid, CreatureLoginInfo> resolveFulfillers)
        {
            var _awarenesses = preReqs.OfType<CoreSelectPrerequisiteInfo>().Any()
                ? preReqProxy.GetCoreInfoAwarenesses(preReqs).ToList()
                : new List<AwarenessInfo>();
            _Items = preReqs
                .Where(_p => !(_p is ActionInquiryPrerequisiteInfo))
                .Select(_p => PrerequisiteModel.ToModel(_p, preReqProxy, _awarenesses, resolveFulfillers)).ToList();
            if (_Items.Count == 1)
                _Items[0].IsSingleton = true;
        }
        #endregion

        #region data
        private List<PrerequisiteModel> _Items;
        #endregion

        public Visibility Visibility
            => _Items.Any() ? Visibility.Visible : Visibility.Collapsed;

        public Visibility SendButtonVisibility
            => _Items.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

        public IPrerequisiteProxy PrerequisiteProxy
            => _Items.FirstOrDefault()?.PrerequisiteProxy;

        public List<PrerequisiteModel> Items => _Items;
    }
}
