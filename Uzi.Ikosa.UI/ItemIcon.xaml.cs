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
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using System.Diagnostics;
using Uzi.Ikosa.Items;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.UI
{
    /// <summary>
    /// Interaction logic for ItemIcon.xaml
    /// </summary>
    public partial class ItemIcon : UserControl
    {
        public ItemIcon()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(ItemIcon_DataContextChanged);
        }

        void ItemIcon_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _coreItem = DataContext as PresentableContext;
            if (_coreItem != null)
            {
                var _resources = _coreItem.VisualResources;
                if (_resources != null)
                {
                    foreach (var _key in _coreItem.CoreObject.IconKeys)
                    {
                        var _icon = _resources.ResolveIconVisual(_key, _coreItem.CoreObject);
                        if (_icon != null)
                        {
                            ccIcon.Content = _icon;
                            return;
                        }
                    }
                }
            }
        }
    }
}
