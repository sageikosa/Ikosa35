using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class AdvancementVM : ViewModelBase
    {
        #region ctor()
        public AdvancementVM(ActorModel actor, AdvanceableCreature advanceableCreature)
        {
            // params
            Actor = actor;
            _Advanceable = advanceableCreature;

            // initialize
            _Workflow = new Stack<WorkflowStep>();
            AdvancementStep = AdvancementStep.ClassSelection;
            _PowerLevel = null;
            _Classes = Actor.Proxies?.IkosaProxy.Service
                .GetAvailableClasses(_Advanceable.ID).OrderBy(_c => _c.ClassName).ToList()
                ?? new List<ClassInfo>();
            _Abilities = null;
            _Feats = null;
            _Requirements = null;

            // define
            DoNextStep = new RelayCommand(OnDoNextStep, OnCanDoNextStep);
            DoPrevStep = new RelayCommand(OnDoPrevStep, OnCanDoPrevStep);
            DoIncreaseSkillBuy = new RelayCommand<SkillBuyInfo>(OnDoIncreaseSkillBuy);
            DoDecreaseSkillBuy = new RelayCommand<SkillBuyInfo>(OnDoDecreaseSkillBuy);
            DoConfirm = new RelayCommand(() =>
            {
                AdvanceableCreature = Actor.Proxies?.IkosaProxy.Service.LockClassLevel(_Advanceable.ID);
            });
        }
        #endregion

        #region data
        private AdvanceableCreature _Advanceable;
        private readonly Stack<WorkflowStep> _Workflow;
        private AdvancementStep _Step;
        private readonly List<ClassInfo> _Classes;
        private AbilitySetInfo _Abilities;
        private List<AdvancementOptionVM> _Feats;
        private AdvancementOptionVM _FeatOption;
        private List<SkillBuyVM> _BasicSkills;
        private List<SkillBuyVM> _ParameterizedSkills;
        private List<AdvancementRequirementVM> _Requirements;
        #endregion

        private AdvancementLogInfo _CurrentLogInfo
            => AdvanceableCreature.AdvancementLogInfos.LastOrDefault(_li => !_li.IsLocked);

        public ActorModel Actor { get; private set; }

        public List<WorkflowStep> Workflow => _Workflow.Reverse().ToList();

        #region workflow status
        private void AddWorkflow(string description)
        {
            _Workflow.Push(new WorkflowStep
            {
                Description = description
            });
            DoPropertyChanged(nameof(Workflow));
        }

        private void UpdateWorkflow(string description)
        {
            var _workflow = _Workflow.Peek();
            if (_workflow != null)
                _workflow.Description = description;
        }

        private void RollbackWorkflow()
        {
            // double pop (current + previous so it can be re-added in initial state)
            if (_Workflow.Any())
                _Workflow.Pop();
            if (_Workflow.Any())
                _Workflow.Pop();
            DoPropertyChanged(nameof(Workflow));
        }
        #endregion

        #region public AdvanceableCreature AdvanceableCreature { get; private set; }
        public AdvanceableCreature AdvanceableCreature
        {
            get => _Advanceable;
            internal set
            {
                if (value != null)
                {
                    _Advanceable = value;
                    DoPropertyChanged(nameof(AdvanceableCreature));
                    DoPropertyChanged(nameof(CurrentPowerDie));

                    // update creature also
                    Actor.UpdateCreature();
                }
            }
        }
        #endregion

        public IEnumerable<ClassInfo> AvailableClasses => _Classes;

        #region public ClassInfo SelectedClass { get; set; }
        public ClassInfo SelectedClass
        {
            get => _Classes.FirstOrDefault(_c => _c.FullName == _CurrentLogInfo?.Class.FullName);
            set
            {
                if (AdvancementStep == AdvancementStep.ClassSelection)
                {
                    if (SelectedClass != null)
                    {
                        // remove class level already added and unlocked
                        AdvanceableCreature = Actor.Proxies.IkosaProxy.Service
                            .PopClassLevel(AdvanceableCreature.ID);
                        _Requirements = new List<AdvancementRequirementVM>();
                    }

                    if (value != null)
                    {
                        // push new class level
                        AdvanceableCreature = Actor.Proxies.IkosaProxy.Service
                            .PushClassLevel(AdvanceableCreature.ID, value);
                        if (_CurrentLogInfo?.Requirements?.Any() ?? false)
                        {
                            _Requirements = _CurrentLogInfo.Requirements
                                .Select(_r => new AdvancementRequirementVM(this, _r))
                                .ToList();
                        }
                    }

                    // property updated
                    DoPropertyChanged(nameof(SelectedClass));
                    DoPropertyChanged(nameof(Requirements));
                }
            }
        }
        #endregion

        #region private int? _PowerLevel { get; set; }
        private int? _PowerDieLevel;
        private int? _PowerLevel
        {
            get => _PowerDieLevel;
            set
            {
                // power die itself
                _PowerDieLevel = value;
                DoPropertyChanged(nameof(CurrentPowerDie));

                // abilities
                _Abilities = Actor.Proxies.IkosaProxy.Service.GetBoostableAbilities(
                    AdvanceableCreature.ID, _PowerDieLevel ?? 0);
                DoPropertyChanged(nameof(BoostedAbility));
                DoPropertyChanged(nameof(AvailableAbilityBoosts));

                // health points
                DoPropertyChanged(nameof(HealthPoints));

                // skills (initialize collections)
                var _available = Actor.Proxies.IkosaProxy.Service.GetAvailableSkills(
                    AdvanceableCreature.ID, _PowerDieLevel ?? 0);
                var _skillBuys = CurrentPowerDie?.SkillsAssigned;
                SkillBuyInfo _getSkillBuy(SkillVM skill)
                {
                    var _avail = _available.FirstOrDefault(_a => _a.SkillName == skill.Skill.SkillName);
                    var _buy = _skillBuys?.FirstOrDefault(_sb => _sb.Skill.SkillName == skill.Skill.SkillName);
                    return new SkillBuyInfo
                    {
                        Skill = skill.Skill,
                        PointsUsed = _buy?.PointsUsed ?? 0,
                        IsClassSkill = _avail?.IsClassSkill ?? false
                    };
                };
                _BasicSkills = Actor.CreatureModel.BasicSkills
                    .Select(_s => new SkillBuyVM(this, _getSkillBuy(_s)))
                    .ToList();
                _ParameterizedSkills = Actor.CreatureModel.ParameterizedSkills
                    .Select(_s => new SkillBuyVM(this, _getSkillBuy(_s)))
                    .ToList();
                DoPropertyChanged(nameof(BasicSkills));
                DoPropertyChanged(nameof(ParameterizedSkills));

                // feats (converted to view models)
                var _cmd = new RelayCommand<AdvancementOptionInfo>(
                    (advOpt) =>
                    {
                        var _pd = CurrentPowerDie;
                        if ((_pd?.IsFeatPowerDie ?? false) && !string.IsNullOrWhiteSpace(advOpt?.FullName))
                        {
                            AdvanceableCreature = Actor.Proxies.IkosaProxy.Service.SetFeat(
                                AdvanceableCreature.ID, _PowerLevel ?? 0, advOpt);
                            DoPropertyChanged(nameof(SelectedFeat));
                        }
                    });
                _Feats = Actor.Proxies.IkosaProxy.Service
                    .GetAvailableFeats(AdvanceableCreature.ID, _PowerDieLevel ?? 0)
                    .Select(_ao => new AdvancementOptionVM(_ao, _cmd))
                    .ToList();

                // notifications
                DoPropertyChanged(nameof(SelectableFeats));
                DoPropertyChanged(nameof(SelectedFeat));
            }
        }
        #endregion

        public PowerDieInfo CurrentPowerDie
            => _CurrentLogInfo?.PowerDice.FirstOrDefault(_pd => _pd.PowerLevel == _PowerLevel);

        #region public List<AbilityInfo> AvailableAbilityBoosts { get; }
        public List<AbilityInfo> AvailableAbilityBoosts
        {
            get
            {
                var _pd = CurrentPowerDie;
                if (_pd?.IsAbilityBoostPowerDie ?? false)
                {
                    return _Abilities?.AllAbilities() ?? new List<AbilityInfo>();
                }
                return new List<AbilityInfo>();
            }
        }
        #endregion

        #region public AbilityInfo BoostedAbility { get; set; }
        public AbilityInfo BoostedAbility
        {
            get
            {
                var _pd = CurrentPowerDie;
                return (_pd?.IsAbilityBoostPowerDie ?? false)
                    ? _pd.IsAbilityBoostMissing ? null : AvailableAbilityBoosts.FirstOrDefault(_b => _b.Mnemonic == _pd.BoostedAbility.Mnemonic)
                    : null;
            }
            set
            {
                var _pd = CurrentPowerDie;
                if ((_pd?.IsAbilityBoostPowerDie ?? false) && !string.IsNullOrWhiteSpace(value?.Mnemonic))
                {
                    AdvanceableCreature = Actor.Proxies.IkosaProxy.Service.SetAbilityBoost(
                        AdvanceableCreature.ID, _PowerLevel ?? 0, value.Mnemonic);
                    DoPropertyChanged(nameof(BoostedAbility));
                }
            }
        }
        #endregion

        #region public int HealthPoints { get; set; }
        public int HealthPoints
        {
            get => CurrentPowerDie?.HealthPoints ?? 0;
            set
            {
                var _pd = CurrentPowerDie;
                if ((value > 0) && (value <= (_CurrentLogInfo?.Class.PowerDieSize ?? 0)))
                {
                    AdvanceableCreature = Actor.Proxies.IkosaProxy.Service.SetHealthPoints(
                        AdvanceableCreature.ID, _PowerLevel ?? 0, value);
                    DoPropertyChanged(nameof(HealthPoints));
                }
            }
        }
        #endregion

        public List<SkillBuyVM> BasicSkills => _BasicSkills;
        public List<SkillBuyVM> ParameterizedSkills => _ParameterizedSkills;

        public RelayCommand<SkillBuyInfo> DoIncreaseSkillBuy { get; private set; }
        public RelayCommand<SkillBuyInfo> DoDecreaseSkillBuy { get; private set; }

        #region private void OnDoIncreaseSkillBuy(SkillBuyInfo skillBuy)
        private void OnDoIncreaseSkillBuy(SkillBuyInfo skillBuy)
        {
            var _pd = CurrentPowerDie;
            if ((_pd?.SkillPointsLeft > 0) && (skillBuy != null))
            {
                AdvanceableCreature = Actor.Proxies.IkosaProxy.Service.SetSkillPoints(
                    AdvanceableCreature.ID, _PowerLevel ?? 0, skillBuy.Skill, skillBuy.PointsUsed + 1);

                // conformulate
                var _available = Actor.Proxies.IkosaProxy.Service.GetAvailableSkills(
                    AdvanceableCreature.ID, _PowerDieLevel ?? 0);
                ConformulateSkills(_available, _BasicSkills);
                ConformulateSkills(_available, _ParameterizedSkills);
            }
        }
        #endregion

        #region private void OnDoDecreaseSkillBuy(SkillBuyInfo skillBuy)
        private void OnDoDecreaseSkillBuy(SkillBuyInfo skillBuy)
        {
            var _pd = CurrentPowerDie;
            if ((_pd != null) && (skillBuy?.PointsUsed > 0))
            {
                AdvanceableCreature = Actor.Proxies.IkosaProxy.Service.SetSkillPoints(
                    AdvanceableCreature.ID, _PowerLevel ?? 0, skillBuy.Skill, skillBuy.PointsUsed - 1);

                // conformulate
                var _available = Actor.Proxies.IkosaProxy.Service.GetAvailableSkills(
                    AdvanceableCreature.ID, _PowerDieLevel ?? 0);
                ConformulateSkills(_available, _BasicSkills);
                ConformulateSkills(_available, _ParameterizedSkills);
            }
        }
        #endregion

        #region private void ConformulateSkills(List<SkillInfo> available, List<SkillBuyVM> tracked)
        private void ConformulateSkills(List<SkillInfo> available, List<SkillBuyVM> tracked)
        {
            var _skillBuys = CurrentPowerDie.SkillsAssigned;
            SkillBuyInfo _getSkillBuy(SkillInfo skill)
            {
                var _avail = available.FirstOrDefault(_a => _a.SkillName == skill.SkillName);
                var _buy = _skillBuys.FirstOrDefault(_sb => _sb.Skill.SkillName == skill.SkillName);
                return new SkillBuyInfo
                {
                    Skill = _avail ?? skill,
                    PointsUsed = _buy?.PointsUsed ?? 0,
                    IsClassSkill = _avail?.IsClassSkill ?? false
                };
            };
            foreach (var _trackedSkill in tracked.ToList())
            {
                _trackedSkill.SkillBuyInfo = _getSkillBuy(_trackedSkill.SkillBuyInfo.Skill);
            }
        }
        #endregion

        public List<AdvancementOptionVM> SelectableFeats => _Feats;

        #region public AdvancementOptionVM SelectedFeatOption { get; set; }
        public AdvancementOptionVM SelectedFeatOption
        {
            get => _FeatOption;
            set
            {
                _FeatOption = value;
                if (_FeatOption != null)
                {
                    if (!_FeatOption.AvailableParameters.Any())
                    {
                        // leaf node
                        _FeatOption.Selector?.Execute(_FeatOption.SelectionValue);
                    }
                }
                DoPropertyChanged(nameof(SelectedFeatOption));
            }
        }
        #endregion

        #region public FeatInfo SelectedFeat { get; }
        public FeatInfo SelectedFeat
        {
            get
            {
                var _pd = CurrentPowerDie;
                return (_pd?.IsFeatPowerDie ?? false)
                    ? _pd.Feat
                    : null;
            }
        }
        #endregion

        public List<AdvancementRequirementVM> Requirements => _Requirements;

        public RelayCommand DoConfirm { get; private set; }

        public RelayCommand DoNextStep { get; private set; }
        public RelayCommand DoPrevStep { get; private set; }

        #region private bool OnCanDoNextStep()
        private bool OnCanDoNextStep()
        {
            switch (AdvancementStep)
            {
                case AdvancementStep.ClassSelection:
                    return SelectedClass != null;

                case AdvancementStep.AbilityBoost:
                    return !(CurrentPowerDie?.IsAbilityBoostMissing ?? true);

                case AdvancementStep.HealthPoints:
                    return !(CurrentPowerDie?.IsHealthPointCountLow ?? true);

                case AdvancementStep.SkillAssignment:
                    return (CurrentPowerDie?.SkillPointsLeft == 0);

                case AdvancementStep.FeatSelection:
                    return !(CurrentPowerDie?.IsFeatMissing ?? true);

                case AdvancementStep.ClassRequirements:
                    return _CurrentLogInfo?.Requirements?.All(_r => _r.CurrentValue != null) ?? false;

                case AdvancementStep.Complete:
                    return true;
            }
            return false;
        }
        #endregion

        #region private void OnDoNextStep()
        private void OnDoNextStep()
        {
            // used when starting a new power die sub-flow
            AdvancementStep _nextPowerDie()
                => (CurrentPowerDie?.IsAbilityBoostPowerDie ?? false)
                ? AdvancementStep.AbilityBoost
                : AdvancementStep.HealthPoints;

            // used when all power dice are done and we need to check class requirements
            AdvancementStep _noMorePowerDice()
                => (_CurrentLogInfo?.Requirements.Any() ?? false)
                ? AdvancementStep.ClassRequirements
                : AdvancementStep.Complete;

            // when a single power die is complete, loop back or end power dice
            AdvancementStep _powerDieEnd()
            {
                if (!_CurrentLogInfo.IsMaxPowerDie(CurrentPowerDie))
                {
                    // more power dice
                    _PowerLevel++;
                    return _nextPowerDie();
                }
                else
                {
                    return _noMorePowerDice();
                }
            }

            switch (AdvancementStep)
            {
                case AdvancementStep.ClassSelection:
                    UpdateWorkflow($@"Class: {SelectedClass?.ClassName}");
                    // finished selecting a class
                    if (_CurrentLogInfo?.PowerDice.Any() ?? false)
                    {
                        // class level has power dice, so figure out which step is next
                        _PowerLevel = _CurrentLogInfo.PowerDice.Min(_pd => _pd.PowerLevel);
                        AdvancementStep = _nextPowerDie();
                    }
                    else
                    {
                        // no power dice, but if requirements, that's the next step
                        AdvancementStep = _noMorePowerDice();
                    }
                    break;

                case AdvancementStep.AbilityBoost:
                    // health points always after ability boost
                    UpdateWorkflow($@"@PD[{_PowerLevel}] Boosted: {BoostedAbility?.Name}");
                    AdvancementStep = AdvancementStep.HealthPoints;
                    break;

                case AdvancementStep.HealthPoints:
                    // skill assignments always after health points
                    UpdateWorkflow($@"@PD[{_PowerLevel}] Health: {HealthPoints}");
                    AdvancementStep = AdvancementStep.SkillAssignment;
                    break;

                case AdvancementStep.SkillAssignment:
                    UpdateWorkflow($@"@PD[{_PowerLevel}] Skills Assigned");
                    if (CurrentPowerDie.IsFeatPowerDie)
                    {
                        // feat, if power die is expecting it
                        AdvancementStep = AdvancementStep.FeatSelection;
                    }
                    else
                    {
                        // done with this power die
                        AdvancementStep = _powerDieEnd();
                    }
                    break;

                case AdvancementStep.FeatSelection:
                    // done with this power die
                    UpdateWorkflow($@"@PD[{_PowerLevel}] Feat: {SelectedFeat?.FeatName}");
                    AdvancementStep = _powerDieEnd();
                    break;

                case AdvancementStep.ClassRequirements:
                    // class requirements complete
                    UpdateWorkflow($@"Requirements Selected");
                    AdvancementStep = AdvancementStep.Complete;
                    break;

                case AdvancementStep.Complete:
                default:
                    UpdateWorkflow($@"Done");
                    break;
            }
        }
        #endregion

        #region private bool OnCanDoPrevStep()
        private bool OnCanDoPrevStep()
        {
            // (almost) always can back up
            switch (AdvancementStep)
            {
                case AdvancementStep.ClassSelection:
                    return false;
            }
            return true;
        }
        #endregion

        #region private void OnDoPrevStep()
        private void OnDoPrevStep()
        {
            // when stepping back go to feat or skill
            AdvancementStep _prevPowerDie()
                => (CurrentPowerDie?.IsFeatPowerDie ?? false)
                ? AdvancementStep.FeatSelection
                : AdvancementStep.SkillAssignment;

            // when a single power die is complete, loop back or end power dice
            AdvancementStep _powerDieStart()
            {
                if (!_CurrentLogInfo.IsMinPowerDie(CurrentPowerDie))
                {
                    // more power dice
                    _PowerLevel--;
                    return _prevPowerDie();
                }
                else
                {
                    // no more power dice to step back through...
                    return AdvancementStep.ClassSelection;
                }
            }

            // requirements predecessor
            AdvancementStep _beforeRequirements()
                => (_CurrentLogInfo?.PowerDice.Any() ?? false)
                ? _prevPowerDie()
                : AdvancementStep.ClassSelection;

            switch (AdvancementStep)
            {
                case AdvancementStep.AbilityBoost:
                    RollbackWorkflow();
                    // may need to loop back to feat selection/skill assignment
                    AdvancementStep = _powerDieStart();
                    break;

                case AdvancementStep.HealthPoints:
                    RollbackWorkflow();
                    if (CurrentPowerDie.IsAbilityBoostPowerDie)
                        AdvancementStep = AdvancementStep.AbilityBoost;
                    else
                        AdvancementStep = _powerDieStart();
                    break;

                case AdvancementStep.SkillAssignment:
                    RollbackWorkflow();
                    AdvancementStep = AdvancementStep.HealthPoints;
                    break;

                case AdvancementStep.FeatSelection:
                    RollbackWorkflow();
                    AdvancementStep = AdvancementStep.SkillAssignment;
                    break;

                case AdvancementStep.ClassRequirements:
                    RollbackWorkflow();
                    AdvancementStep = _beforeRequirements();
                    break;

                case AdvancementStep.Complete:
                    RollbackWorkflow();
                    if (_CurrentLogInfo?.Requirements.Any() ?? false)
                        AdvancementStep = AdvancementStep.ClassRequirements;
                    else
                        AdvancementStep = _beforeRequirements();
                    break;

                default:
                    break;
            }
        }
        #endregion

        #region public AdvancementStep AdvancementStep { get; private set; }
        public AdvancementStep AdvancementStep
        {
            get => _Step;
            private set
            {
                _Step = value;

                // recalc all visibilities
                DoPropertyChanged(nameof(ClassSelectionVisibility));
                DoPropertyChanged(nameof(AbilityBoostSelectionVisibility));
                DoPropertyChanged(nameof(HealthPointsAssignmentVisibility));
                DoPropertyChanged(nameof(SkillAssignmentVisibility));
                DoPropertyChanged(nameof(FeatSelectionVisibility));
                DoPropertyChanged(nameof(ClassRequirementsVisibility));
                DoPropertyChanged(nameof(ConfirmVisibility));
                switch (_Step)
                {
                    case AdvancementStep.ClassSelection:
                        AddWorkflow(@"Select Class...");
                        break;
                    case AdvancementStep.AbilityBoost:
                        AddWorkflow(@"Select Ability...");
                        break;
                    case AdvancementStep.HealthPoints:
                        AddWorkflow(@"Set Health Points...");
                        break;
                    case AdvancementStep.SkillAssignment:
                        AddWorkflow(@"Assign Skill Points...");
                        break;
                    case AdvancementStep.FeatSelection:
                        AddWorkflow(@"Select Feat...");
                        break;
                    case AdvancementStep.ClassRequirements:
                        AddWorkflow(@"Select Class Requirements...");
                        break;
                    case AdvancementStep.Complete:
                        AddWorkflow(@"Confirm Advancement...");
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion

        private Visibility VisibleMatch(AdvancementStep step)
            => AdvancementStep == step ? Visibility.Visible : Visibility.Collapsed;

        public Visibility ClassSelectionVisibility => VisibleMatch(AdvancementStep.ClassSelection);
        public Visibility AbilityBoostSelectionVisibility => VisibleMatch(AdvancementStep.AbilityBoost);
        public Visibility HealthPointsAssignmentVisibility => VisibleMatch(AdvancementStep.HealthPoints);
        public Visibility SkillAssignmentVisibility => VisibleMatch(AdvancementStep.SkillAssignment);
        public Visibility FeatSelectionVisibility => VisibleMatch(AdvancementStep.FeatSelection);
        public Visibility ClassRequirementsVisibility => VisibleMatch(AdvancementStep.ClassRequirements);
        public Visibility ConfirmVisibility => VisibleMatch(AdvancementStep.Complete);
    }
}
