using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for OpportunisticPrerequisiteItem.xaml
    /// </summary>
    public partial class OpportunisticPrerequisiteItem : UserControl
    {
        public OpportunisticPrerequisiteItem()
        {
            try { InitializeComponent(); } catch { }
            DataContextChanged += new DependencyPropertyChangedEventHandler(OpportunisticPrerequisiteItem_DataContextChanged);
        }

        void OpportunisticPrerequisiteItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _oppty = OpportunisticPrerequisiteInfo;
            if (_oppty != null)
            {
                // list attacks (and no action)
                var _atks = _oppty.AttackActions.ToList();
                var _noAct = new ActionInfo { ID = Guid.Empty, Key = string.Empty, DisplayName = @"No Attack", TimeCost = @"-None-" };
                _atks.Insert(0, _noAct);
                icResponse.ItemsSource = _atks;

                // start ready
                _oppty.IsReady = true;
            }
        }

        public PrerequisiteModel PrerequisiteModel
            => DataContext as PrerequisiteModel;

        public OpportunisticPrerequisiteInfo OpportunisticPrerequisiteInfo
            => PrerequisiteModel?.Prerequisite as OpportunisticPrerequisiteInfo;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var _oppty = OpportunisticPrerequisiteInfo;
            if (_oppty != null)
            {
                // build activity from action
                var _action = (sender as Button)?.Content as ActionInfo;
                if ((_action.ID == Guid.Empty) && (_action.Key == string.Empty))
                {
                    _oppty.OpportunisticActivity = null;
                    PrerequisiteModel.PrerequisiteProxy.DoSendPrerequisites.Execute(null);
                    PrerequisiteModel.IsSent = true;
                }
                else
                {
                    // TODO: command/event with attack action and target ...
                    var _preReqProxy = PrerequisiteModel.PrerequisiteProxy;
                    if (_preReqProxy != null)
                    {
                        #region stuff
                        // collect stuff (and report)
                        var _proxy = _preReqProxy.Proxies;
                        var _atkScore = _proxy.RollDice(_action.DisplayName, @"Attack Roll", @"1d20", PrerequisiteModel.Fulfiller.ID).Total;
                        var _critScore = _proxy.RollDice(_action.DisplayName, @"Critical", @"1d20", PrerequisiteModel.Fulfiller.ID).Total;
                        #endregion

                        // build attack target
                        var _atkTarget = new AttackTargetInfo
                        {
                            Key = _action.AimingModes.FirstOrDefault().Key,
                            TargetID = _oppty.ActivityInfo.ActorID,
                            AttackScore = _atkScore,
                            CriticalConfirm = _critScore,
                            Impact = AttackImpact.Penetrating,
                            IsNonLethal = false
                        };

                        var _activity = new ActivityInfo
                        {
                            ActionID = _action.ID,
                            ActionKey = _action.Key,
                            ActorID = _preReqProxy.FulfillerID,
                            Targets = new AimTargetInfo[] { _atkTarget }
                        };
                        _oppty.OpportunisticActivity = _activity;
                        PrerequisiteModel.PrerequisiteProxy.DoSendPrerequisites.Execute(null);
                        PrerequisiteModel.IsSent = true;
                    }
                }
            }

        }
    }
}
