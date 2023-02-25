using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Uzi.Core
{
    [Serializable]
    public class MultiActionFilter : Adjunct, IActionFilter
    {
        public MultiActionFilter(object source, string reason, params Type[] actionTypes)
            : base(source)
        {
            _Blocked = actionTypes.ToList();
            _Reason = reason;
        }

        #region data
        private List<Type> _Blocked;
        private string _Reason;
        #endregion

        public IEnumerable<Type> Blocked => _Blocked.Select(_b => _b);
        public string Reason => _Reason;

        public override object Clone()
            => new MultiActionFilter(Source, Reason, _Blocked.ToArray());

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (source == this)
                (Anchor as CoreActor)?.Actions.Filters.Add(this, this);
        }

        protected override void OnDeactivate(object source)
        {
            if (source == this)
                (Anchor as CoreActor)?.Actions.Filters.Remove(this);
            base.OnDeactivate(source);
        }

        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
            => IsActive && _Blocked.Contains(action.GetType());
    }
}
