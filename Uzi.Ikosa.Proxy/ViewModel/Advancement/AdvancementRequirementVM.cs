using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class AdvancementRequirementVM : ViewModelBase
    {
        public AdvancementRequirementVM(AdvancementVM advancement,
            AdvancementRequirementInfo requirement)
        {
            AdvancementVM = advancement;
            AdvancementRequirementInfo = requirement;

            // command to set this requirement
            var _cmd = new RelayCommand<AdvancementOptionInfo>(
                (advOpt) =>
                {
                    AdvancementVM.AdvanceableCreature =
                        AdvancementVM?.Actor.Proxies.IkosaProxy.Service.SetAdvancementOption(
                            AdvancementVM.AdvanceableCreature.ID,
                            AdvancementRequirementInfo,
                            advOpt);
                });
            Options = (requirement?.AvailableOptions?.Any() ?? false)
                ? requirement.AvailableOptions.Select(_o => new AdvancementOptionVM(_o, _cmd)).ToList()
                : new List<AdvancementOptionVM>();
        }

        public AdvancementVM AdvancementVM { get; private set; }
        public AdvancementRequirementInfo AdvancementRequirementInfo { get; private set; }
        public List<AdvancementOptionVM> Options { get; private set; }
    }
}
