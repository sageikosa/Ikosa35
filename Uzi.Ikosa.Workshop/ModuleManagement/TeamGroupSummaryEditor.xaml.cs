using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for TeamGroupSummaryEditor.xaml
    /// </summary>
    public partial class TeamGroupSummaryEditor : UserControl
    {
        public TeamGroupSummaryEditor()
        {
            InitializeComponent();
        }

        public IHostTabControl HostTabControl
        {
            get { return (IHostTabControl)GetValue(HostedTabControlProperty); }
            set { SetValue(HostedTabControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HostedTabControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HostedTabControlProperty =
            DependencyProperty.Register(nameof(HostTabControl), typeof(IHostTabControl), typeof(TeamGroupSummaryEditor),
                new PropertyMetadata(null));
    }
}
