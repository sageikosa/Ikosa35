using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class KeyItem : ItemBase, IActionProvider, IKeyRingMountable
    {
        public KeyItem(string name, IEnumerable<Guid> keyGuids)
            : base(name, Size.Fine)
        {
            _Keys = new HashSet<Guid>(keyGuids.ToArray());
            ItemMaterial = MetalMaterial.CommonStatic;
            BaseWeight = 1d / 16d;
        }

        #region data
        private HashSet<Guid> _Keys;
        #endregion

        public HashSet<Guid> Keys => _Keys;

        public bool IsCompatible(IEnumerable<Guid> inKeys)
        {
            return (from _kig in _Keys
                    join _ik in inKeys
                    on _kig equals _ik
                    select _ik).Count() > 0;
        }

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            if (_budget.CanPerformBrief)
            {
                yield return new UseKey(this, @"101");
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            return GetInfoData.GetInfoFeedback(this, budget.Actor);
        }
        #endregion

        protected override string ClassIconKey => nameof(KeyItem);
    }
}
