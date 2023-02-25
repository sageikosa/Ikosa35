using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    /// <summary>tracks parents for chain selection</summary>
    public class AdvancementOptionVM : ViewModelBase
    {
        #region ctor()
        /// <summary>tracks parents for chain selection</summary>
        public AdvancementOptionVM(AdvancementOptionInfo self, RelayCommand<AdvancementOptionInfo> selector)
            : this(null, self)
        {
            _Selector = selector;
        }

        /// <summary>tracks parents for chain selection</summary>
        public AdvancementOptionVM(AdvancementOptionVM parent, AdvancementOptionInfo self)
        {
            AdvancementOptionInfo = self;
            Parent = parent;
            if (self?.AvailableParameters?.Any() ?? false)
            {
                AvailableParameters = self.AvailableParameters
                    .Select(_p => new AdvancementOptionVM(this, _p))
                    .ToList();
            }
            else
            {
                AvailableParameters = new List<AdvancementOptionVM>();
            }
        }
        #endregion

        #region data
        private readonly RelayCommand<AdvancementOptionInfo> _Selector = null;
        #endregion

        public AdvancementOptionVM Parent { get; private set; }
        public List<AdvancementOptionVM> AvailableParameters { get; private set; }

        public RelayCommand<AdvancementOptionInfo> Selector
            => _Selector ?? Parent?.Selector;

        /// <summary>Direct option suitable for display.</summary>
        public AdvancementOptionInfo AdvancementOptionInfo { get; private set; }

        protected AdvancementOptionInfo EnfoldSelection(AdvancementOptionInfo selection)
            => Parent?.EnfoldSelection(Parent?.AdvancementOptionInfo.EnfoldParameter(selection))
            ?? selection;

        /// <summary>Rooted AdvancementOptionInfo suitable for path following service parameter</summary>
        public AdvancementOptionInfo SelectionValue
            => EnfoldSelection(AdvancementOptionInfo);
    }
}
