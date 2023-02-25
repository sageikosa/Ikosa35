using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Proxy.VisualizationSvc;
using Uzi.Ikosa.Proxy.ViewModel;
using System.Diagnostics;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for LocalActionPanel.xaml
    /// </summary>
    public partial class LocalActionPanel : UserControl
    {
        public static RoutedCommand AdjustTargetIntersectCmd = new RoutedCommand();

        public static RoutedCommand SearchCommand = new RoutedCommand();
        public static RoutedCommand GraspCommand = new RoutedCommand();
        public static RoutedCommand ProbeCommand = new RoutedCommand();

        public LocalActionPanel()
        {
            try { InitializeComponent(); } catch { }
        }

        #region public LocalActionBudgetInfo Budget { get; set;} (DEPENDENCY)
        public LocalActionBudgetInfo Budget
        {
            get { return (LocalActionBudgetInfo)GetValue(BudgetProperty); }
            set { SetValue(BudgetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Budget.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BudgetProperty =
            DependencyProperty.Register("Budget", typeof(LocalActionBudgetInfo), typeof(LocalActionPanel),
            new UIPropertyMetadata(null, new PropertyChangedCallback(BudgetChanged)));

        #region private static void BudgetChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        private static void BudgetChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is LocalActionPanel _localActions)
            {
                if (args.NewValue != null)
                {
                    var _budget = args.NewValue as LocalActionBudgetInfo;
                    if (_localActions.CanTakeTurn)
                    {
                        if (_budget.CanPerformTotal)
                        {
                            #region total
                            _localActions.lblBudgetParts.Background = new LinearGradientBrush(new GradientStopCollection(
                                new GradientStop[]
                                {
                                    new GradientStop(Colors.Yellow, 0),
                                    new GradientStop(Colors.DarkGoldenrod, 0.40),
                                    new GradientStop(Colors.Green, 0.50),
                                    new GradientStop(Colors.DarkTurquoise, 0.60),
                                    new GradientStop(Colors.Turquoise, 1.0)
                                }), 0);
                            _localActions.lblBudgetParts.Content = @"Total";
                            _localActions.lblBudgetParts.Foreground = Brushes.Black;
                            #endregion
                        }
                        else if (_budget.CanPerformRegular)
                        {
                            #region regular
                            _localActions.lblBudgetParts.Background = new LinearGradientBrush(new GradientStopCollection(
                                new GradientStop[]
                                {
                                    new GradientStop(Colors.DarkGray, 0),
                                    new GradientStop(Colors.Yellow, 0.2),
                                    new GradientStop(Colors.DarkTurquoise, 0.4),
                                    new GradientStop(Colors.Turquoise, 1.0)
                                }), 0);
                            _localActions.lblBudgetParts.Content = @"Regular";
                            _localActions.lblBudgetParts.Foreground = Brushes.Black;
                            #endregion
                        }
                        else if (_budget.CanPerformBrief)
                        {
                            #region brief
                            _localActions.lblBudgetParts.Background = new LinearGradientBrush(new GradientStopCollection(
                                new GradientStop[]
                                {
                                    new GradientStop(Colors.Yellow, 0),
                                    new GradientStop(Colors.DarkGoldenrod, 0.6),
                                    new GradientStop(Colors.DarkGray, 1.0)
                                }), 0);
                            _localActions.lblBudgetParts.Content = @"Brief";
                            _localActions.lblBudgetParts.Foreground = Brushes.Black;
                            #endregion
                        }
                        else
                        {
                            _localActions.lblBudgetParts.Background = Brushes.DarkGoldenrod;
                            _localActions.lblBudgetParts.Content = @"-";
                            _localActions.lblBudgetParts.Foreground = Brushes.White;
                        }
                    }
                    else
                    {
                        _localActions.lblBudgetParts.Background = Brushes.DarkGray;
                        _localActions.lblBudgetParts.Content = @"-";
                        _localActions.lblBudgetParts.Foreground = Brushes.White;
                    }
                }
                else
                {
                    _localActions.lblBudgetParts.Background = Brushes.DarkGray;
                    _localActions.lblBudgetParts.Content = @"-";
                    _localActions.lblBudgetParts.Foreground = Brushes.White;
                }
            }
        }
        #endregion

        #endregion

        #region public AimPointActivation AimPointActivation { get; set; }
        public AimPointActivation AimPointActivation
        {
            get { return (AimPointActivation)GetValue(AimPointActivationProperty); }
            set { SetValue(AimPointActivationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AimPointActivation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AimPointActivationProperty =
            DependencyProperty.Register(@"AimPointActivation", typeof(AimPointActivation), typeof(LocalActionPanel),
            new UIPropertyMetadata(AimPointActivation.Off, new PropertyChangedCallback(AimPointActivationChanged)));

        private static void AimPointActivationChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is LocalActionPanel _lap)
            {
                _lap.actAimPoint.Visibility =
                    ((_lap.AimPointActivation != AimPointActivation.TargetCell) && (_lap.AimPointActivation != AimPointActivation.TargetIntersection))
                    ? Visibility.Visible
                    : Visibility.Hidden;
                _lap.ptrCell.Visibility =
                    (_lap.AimPointActivation == AimPointActivation.TargetCell)
                    ? Visibility.Visible
                    : Visibility.Hidden;
                _lap.ptrIntersect.Visibility =
                    (_lap.AimPointActivation == AimPointActivation.TargetIntersection)
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
        }
        #endregion

        public ActorModel Actor => DataContext as ActorModel;
        public MoveActionPanel MoveActionPanel => actMove;

        public bool CanTakeTurn
            => (Actor != null)
            && (!Actor.Budget.IsInitiative || Actor.Budget.IsFocusedBudget);

        public void PerformAction(ActivityInfo activity)
        {
            try
            {
                Actor.Proxies.IkosaProxy.Service.DoAction(activity);
                Actor.UpdateActions();
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region public void RefreshActions()
        public void RefreshActions()
        {
            Actor.UpdateActions();
        }
        #endregion

        #region private void cbSearch_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbSearch_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Actor != null)
                e.CanExecute = CanTakeTurn
                    && Actor.Budget.CanPerformTotal
                    && (Actor.Actions.Any(_a => _a.Key == @"Search.Area"));
            e.Handled = true;
        }
        #endregion

        #region private void cbSearch_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbSearch_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            var _act = Actor.Actions.FirstOrDefault(_a => _a.Key == @"Search.Area");
            if (_act != null)
            {
                // found an action (good!)
                var _aim = _act.AimingModes.OfType<LocationAimInfo>().FirstOrDefault();
                if (_aim != null)
                {
                    // found an aim (better!)
                    var _proxy = Actor.Proxies;
                    var _sensors = Actor.ObservableActor.LocaleViewModel.Sensors;
                    var _tCell = Actor.ObservableActor.LocaleViewModel.AdjacentFacingCell;
                    var _cellTarget = new LocationTargetInfo
                    {
                        Key = _aim.Key,
                        TargetID = null,
                        LocationAimMode = Visualize.LocationAimMode.Cell,
                        CellInfo = new Visualize.Contracts.CellInfo(Actor.ObservableActor.LocaleViewModel.TargetCell)
                    };

                    // grasp/probe action
                    var _activity = new ActivityInfo
                    {
                        ActionID = _act.ID,
                        ActionKey = _act.Key,
                        ActorID = Actor.CreatureLoginInfo.ID,
                        Targets = new[] { _cellTarget }
                    };
                    PerformAction(_activity);
                }
            }
        }
        #endregion

        #region private void cbGrasp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbGrasp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Actor != null)
                e.CanExecute = CanTakeTurn
                    && (Actor.Actions.Any(_a => _a.Key == @"Grasp" || _a.Key == @"Probe"));
            e.Handled = true;
        }
        #endregion

        #region private void cbGrasp_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbGrasp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if ((e.Parameter != null) && (e.Parameter.ToString() == @"Full"))
            {
                // TODO: FULL grasp/probe, allow hand selection(?), allow cell selection (if reach>1)
            }
            else
            {
                var _act = Actor.Actions.FirstOrDefault(_a => _a.Key == @"Grasp");
                if (_act == null)
                    _act = Actor.Actions.FirstOrDefault(_a => _a.Key == @"Probe");
                // TODO: stuff

                if (_act != null)
                {
                    // found an action (good!)
                    var _aim = _act.AimingModes.OfType<AttackAimInfo>().FirstOrDefault(_a => _a.Key == @"Cell");
                    if (_aim != null)
                    {
                        // found an aim (better!)
                        var _proxy = Actor.Proxies;
                        var _sensors = Actor.ObservableActor.LocaleViewModel.Sensors;
                        var _tCell = Actor.ObservableActor.LocaleViewModel.AdjacentFacingCell;
                        var _atk = new AttackTargetInfo
                        {
                            Key = _aim.Key,
                            TargetID = null,
                            AttackScore = _proxy.RollDice(_act.Key, _act.DisplayName, @"1d20", Actor.FulfillerID).Total,
                            CriticalConfirm = null,
                            Impact = AttackImpact.Touch,
                            IsNonLethal = true,
                            TargetZ = _tCell.Z,
                            TargetY = _tCell.Y,
                            TargetX = _tCell.X
                        };
                        if (_aim.RangeInfo is StrikeZoneRangeInfo)
                        {
                            var _strike = _aim.RangeInfo as StrikeZoneRangeInfo;
                            if (_strike.MinimumReach > 1)
                                return;
                        }

                        // grasp/probe action
                        var _activity = new ActivityInfo
                        {
                            ActionID = _act.ID,
                            ActionKey = _act.Key,
                            ActorID = Actor.CreatureLoginInfo.ID,
                            Targets = new[] { _atk }
                        };
                        PerformAction(_activity);
                    }
                }
            }
        }
        #endregion
    }
}
