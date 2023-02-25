using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for NamedKeyEditor.xaml
    /// </summary>
    public partial class NamedKeyEditor : UserControl
    {
        public NamedKeyEditor()
        {
            InitializeComponent();
        }

        public IHostTabControl HostTabControl
        {
            get => (IHostTabControl)GetValue(HostedTabControlProperty);
            set => SetValue(HostedTabControlProperty, value);
        }

        // Using a DependencyProperty as the backing store for HostedTabControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HostedTabControlProperty =
            DependencyProperty.Register(nameof(HostTabControl), typeof(IHostTabControl), typeof(NamedKeyEditor),
                new PropertyMetadata(null));

        public NamedKeysPartVM NamedKeysPart => DataContext as NamedKeysPartVM;
    }
}
