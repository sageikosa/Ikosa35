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
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;
using System.Collections.ObjectModel;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for MoveActionPanel.xaml
    /// </summary>
    public partial class MoveActionPanel : UserControl
    {
        #region RoutedMovement Commands
        public static RoutedCommand PrevMove = new RoutedCommand();
        public static RoutedCommand NextMove = new RoutedCommand();
        public static RoutedCommand PickMove = new RoutedCommand();
        public static RoutedCommand PrevStart = new RoutedCommand();
        public static RoutedCommand NextStart = new RoutedCommand();
        public static RoutedCommand PickStart = new RoutedCommand();
        public static RoutedCommand DoubleCmd = new RoutedCommand();

        // special land-based movements
        public static RoutedCommand Jump = new RoutedCommand();
        public static RoutedCommand DropProne = new RoutedCommand();
        public static RoutedCommand Tumble = new RoutedCommand();
        public static RoutedCommand StandUp = new RoutedCommand();
        public static RoutedCommand Crawl = new RoutedCommand();
        #endregion

        #region construction
        public MoveActionPanel()
        {
            try { InitializeComponent(); } catch { }

            // bind panels outward-facing properties to the constituent properties of the heading control
            hdngFlight.SetBinding(Headings.LookHeadingProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath(@"LookHeading")
            });
            hdngFlight.SetBinding(Headings.InclineProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath(@"Incline")
            });
        }
        #endregion

        public ActorModel ActorModel { get { return DataContext as ActorModel; } }

        #region public MovementInfo SelectedMovement { get; set; } (DEPENDENCY)
        public MovementInfo SelectedMovement
        {
            get { return (MovementInfo)GetValue(SelectedMovementProperty); }
            set { SetValue(SelectedMovementProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedMovement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedMovementProperty =
            DependencyProperty.Register(@"SelectedMovement", typeof(MovementInfo), typeof(MoveActionPanel),
            new UIPropertyMetadata(null, new PropertyChangedCallback(DoSelectedChanged)));

        private static void DoSelectedChanged(DependencyObject depends, DependencyPropertyChangedEventArgs args)
        {
            var _panel = depends as MoveActionPanel;
            if (_panel != null)
            {
                var _flight = args.NewValue as FlightMovementInfo;
                if (_flight != null)
                {
                    _panel.hdngFlight.FreeYaw = _flight.FreeYaw;
                    _panel.hdngFlight.MaxYaw = _flight.MaxYaw;
                    _panel.hdngFlight.YawGap = _flight.YawGap;
                    return;
                }
                _panel.hdngFlight.FreeYaw = 4;
                _panel.hdngFlight.MaxYaw = 4;
                _panel.hdngFlight.YawGap = 0d;
            }
        }
        #endregion

        private bool AnyMovements { get { return (Movements != null) && Movements.Any(); } }

        #region public IEnumerable<ActionInfo> Actions { get; set; } (DEPENDENCY)
        public IEnumerable<ActionInfo> Actions
        {
            get { return (IEnumerable<ActionInfo>)GetValue(ActionsProperty); }
            set { SetValue(ActionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LocalActionPanel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register(@"Actions", typeof(IEnumerable<ActionInfo>), typeof(MoveActionPanel),
            new UIPropertyMetadata(null));
        #endregion

        public bool UseDoubleMove { get { return tbUseDouble.IsChecked ?? false; } }

        #region public int LookHeading { get; set; } (DEPENDENCY)
        public int LookHeading
        {
            get { return (int)GetValue(LookHeadingProperty); }
            set { SetValue(LookHeadingProperty, value); }
        }

        public static readonly DependencyProperty LookHeadingProperty =
            DependencyProperty.Register(@"LookHeading", typeof(int), typeof(MoveActionPanel),
            new UIPropertyMetadata(0));
        #endregion

        #region public double Incline { get; set; } (DEPENDENCY)
        public double Incline
        {
            get { return (double)GetValue(InclineProperty); }
            set { SetValue(InclineProperty, value); }
        }

        public static readonly DependencyProperty InclineProperty =
            DependencyProperty.Register(@"Incline", typeof(double), typeof(MoveActionPanel),
            new UIPropertyMetadata(0d));
        #endregion

        #region public LocalActionBudgetInfo Budget { get; set;} (DEPENDENCY)
        public LocalActionBudgetInfo Budget
        {
            get { return (LocalActionBudgetInfo)GetValue(BudgetProperty); }
            set { SetValue(BudgetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Budget.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BudgetProperty =
            DependencyProperty.Register(@"Budget", typeof(LocalActionBudgetInfo), typeof(MoveActionPanel),
            new UIPropertyMetadata(null, new PropertyChangedCallback(BudgetChanged)));

        #region private static void BudgetChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        private static void BudgetChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            var _moveActions = depObj as MoveActionPanel;
            if (_moveActions != null)
            {
                if ((args != null) && (args.NewValue != null))
                {
                    var _budget = args.NewValue as LocalActionBudgetInfo;
                    if (_budget.MovementRangeBudget != null)
                    {
                        var _range = _budget.MovementRangeBudget;
                        if (_range.Remaining > 1d)
                            _moveActions.prgMoveRemaining.Maximum = 2d;
                        else
                            _moveActions.prgMoveRemaining.Maximum = 1d;
                    }
                }
            }
        }
        #endregion

        #endregion

        #region public Collection<MovementInfo> Movements { get; set; } (DEPENDENCY)
        public List<MovementInfo> Movements
        {
            get { return (List<MovementInfo>)GetValue(MovementsProperty); }
            set { SetValue(MovementsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Movements.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MovementsProperty =
            DependencyProperty.Register(@"Movements", typeof(List<MovementInfo>), typeof(MoveActionPanel),
            new UIPropertyMetadata(null));
        #endregion

        private void cbPickMove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = AnyMovements;
            e.Handled = true;
        }

        private void cbPickMove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: dialog to pick movement
            e.Handled = true;
        }

        private void cbPickStart_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbPickStart_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: dialog to pick start movement
            e.Handled = true;
        }
    }
}
