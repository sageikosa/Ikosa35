using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for LocalActionMiniTile.xaml
    /// </summary>
    public partial class LocalActionMiniTile : UserControl
    {
        public LocalActionMiniTile()
        {
            try { InitializeComponent(); } catch { }
            DataContextChanged += LocalActionMiniTile_DataContextChanged;
        }

        private void LocalActionMiniTile_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _budget = LocalActionBudgetInfo;
            if (CanTakeTurn)
            {
                if (_budget.CanPerformTotal)
                {
                    #region total
                    lblBudgetParts.Background = new LinearGradientBrush(new GradientStopCollection(
                        new GradientStop[]
                        {
                                    new GradientStop(Colors.Yellow, 0),
                                    new GradientStop(Colors.DarkGoldenrod, 0.40),
                                    new GradientStop(Colors.Green, 0.50),
                                    new GradientStop(Colors.DarkTurquoise, 0.60),
                                    new GradientStop(Colors.Turquoise, 1.0)
                        }), 0);
                    lblBudgetParts.Content = @"Total";
                    lblBudgetParts.Foreground = Brushes.Black;
                    #endregion
                }
                else if (_budget.CanPerformRegular)
                {
                    #region regular
                    lblBudgetParts.Background = new LinearGradientBrush(new GradientStopCollection(
                        new GradientStop[]
                        {
                                    new GradientStop(Colors.DarkGray, 0),
                                    new GradientStop(Colors.Yellow, 0.2),
                                    new GradientStop(Colors.DarkTurquoise, 0.4),
                                    new GradientStop(Colors.Turquoise, 1.0)
                        }), 0);
                    lblBudgetParts.Content = @"Regular";
                    lblBudgetParts.Foreground = Brushes.Black;
                    #endregion
                }
                else if (_budget.CanPerformBrief)
                {
                    #region brief
                    lblBudgetParts.Background = new LinearGradientBrush(new GradientStopCollection(
                        new GradientStop[]
                        {
                                    new GradientStop(Colors.Yellow, 0),
                                    new GradientStop(Colors.DarkGoldenrod, 0.6),
                                    new GradientStop(Colors.DarkGray, 1.0)
                        }), 0);
                    lblBudgetParts.Content = @"Brief";
                    lblBudgetParts.Foreground = Brushes.Black;
                    #endregion
                }
                else
                {
                    lblBudgetParts.Background = Brushes.DarkGoldenrod;
                    lblBudgetParts.Content = @"-";
                    lblBudgetParts.Foreground = Brushes.White;
                }
            }
            else
            {
                lblBudgetParts.Background = Brushes.DarkGray;
                lblBudgetParts.Content = @"-";
                lblBudgetParts.Foreground = Brushes.White;
            }
            if (_budget?.MovementRangeBudget != null)
            {
                var _range = _budget.MovementRangeBudget;
                if (_range.Remaining > 1d)
                    prgMoveRemaining.Maximum = 2d;
                else
                    prgMoveRemaining.Maximum = 1d;
            }
        }

        public LocalActionBudgetInfo LocalActionBudgetInfo => DataContext as LocalActionBudgetInfo;

        public bool CanTakeTurn
            => (!(LocalActionBudgetInfo?.IsInitiative ?? true) || (LocalActionBudgetInfo?.IsFocusedBudget ?? false));
    }
}
