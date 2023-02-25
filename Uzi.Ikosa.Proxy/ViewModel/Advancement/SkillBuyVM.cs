using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class SkillBuyVM : ViewModelBase
    {
        public SkillBuyVM(AdvancementVM advancement, SkillBuyInfo info)
        {
            _Advancement = advancement;
            _Buy = info;
        }

        #region data
        private readonly AdvancementVM _Advancement;
        private SkillBuyInfo _Buy;
        #endregion

        public double EffectiveValue
            => SkillInfo?.EffectiveValue ?? 0;

        public SkillBuyInfo SkillBuyInfo
        {
            get => _Buy;
            set
            {
                _Buy = value;
                DoPropertyChanged(nameof(SkillBuyInfo));
                DoPropertyChanged(nameof(SkillInfo));
                DoPropertyChanged(nameof(EffectiveValue));
            }
        }

        public SkillInfo SkillInfo
            => _Buy?.Skill;

        public RelayCommand<SkillBuyInfo> DoIncreaseSkillBuy => _Advancement?.DoIncreaseSkillBuy;
        public RelayCommand<SkillBuyInfo> DoDecreaseSkillBuy => _Advancement?.DoDecreaseSkillBuy;
    }
}
