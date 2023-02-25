using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for KeyItemEditor.xaml
    /// </summary>
    public partial class KeyItemEditor : UserControl
    {
        public KeyItemEditor()
        {
            InitializeComponent();
            DataContextChanged += KeyItemEditor_DataContextChanged;
        }

        private void KeyItemEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_Key != null)
            {
                _LocalMap = _Key?.GetLocated()?.Locator.Map ?? _Key?.Possessor?.GetLocated()?.Locator.Map;
                RefreshLists();
            }
            else
            {
                _LocalMap = null;
                lstSelected.Items.Clear();
                lstAvailable.Items.Clear();
            }
        }

        private KeyItem _Key => (DataContext as KeyItemVM)?.Thing;
        private LocalMap _LocalMap;

        #region private void RefreshLists()
        private void RefreshLists()
        {
            // fill selected
            lstSelected.Items.Clear();
            foreach (var _key in _Key.Keys)
            {
                if (_LocalMap.NamedKeyGuids.ContainsKey(_key))
                {
                    lstSelected.Items.Add(new ListBoxItem
                    {
                        Content = _LocalMap.NamedKeyGuids[_key],
                        ToolTip = _key.ToString(),
                        Tag = _key
                    });
                }
                else
                {
                    lstSelected.Items.Add(new ListBoxItem
                    {
                        Content = @"- Unnamed Key -",
                        ToolTip = _key.ToString(),
                        Tag = _key
                    });
                }
            }

            // fill available
            lstAvailable.Items.Clear();
            foreach (var _kvp in _LocalMap.NamedKeyGuids.OrderBy(_nkg => _nkg.Value))
            {
                if (!_Key.Keys.Contains(_kvp.Key))
                {
                    lstAvailable.Items.Add(new ListBoxItem
                    {
                        Content = _LocalMap.NamedKeyGuids[_kvp.Key],
                        ToolTip = _kvp.Key.ToString(),
                        Tag = _kvp.Key
                    });
                }
            }
        }
        #endregion

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (lstAvailable.SelectedItem != null)
            {
                _Key?.Keys.Add((Guid)((lstAvailable.SelectedItem as ListBoxItem).Tag));
            }
            RefreshLists();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelected.SelectedItem != null)
            {
                var _k = (Guid)((lstSelected.SelectedItem as ListBoxItem).Tag);
                if (_Key.Keys.Contains(_k))
                    _Key.Keys.Remove(_k);
            }
            RefreshLists();
        }

        private void lstAvailable_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstAvailable.SelectedItem != null)
            {
                var _key = (Guid)((lstAvailable.SelectedItem as ListBoxItem).Tag);
                var _rename = new RenameKey(_key, _LocalMap)
                {
                    Owner = Window.GetWindow(this)
                };
                _rename.ShowDialog();
                RefreshLists();
            }
        }

        private void lstSelected_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstSelected.SelectedItem != null)
            {
                var _key = (Guid)((lstSelected.SelectedItem as ListBoxItem).Tag);
                var _rename = new RenameKey(_key, _LocalMap)
                {
                    Owner = Window.GetWindow(this)
                };
                _rename.ShowDialog();
                RefreshLists();
            }
        }
    }
}
