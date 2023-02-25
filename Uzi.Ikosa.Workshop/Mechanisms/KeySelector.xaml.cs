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
using System.Windows.Shapes;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Core;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for KeySelector.xaml
    /// </summary>
    public partial class KeySelector : Window
    {
        public KeySelector(ISecureLock secureLock)
        {
            InitializeComponent();
            _Secure = secureLock;
            _LocalMap = (_Secure as ICoreObject)?.GetLocated().Locator.Map;

            RefreshLists();
        }

        private void RefreshLists()
        {
            // fill selected
            lstSelected.Items.Clear();
            foreach (var _key in _Secure.Keys)
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
                if (!_Secure.Keys.Contains(_kvp.Key))
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

        private ISecureLock _Secure;
        private LocalMap _LocalMap;

        public ISecureLock SecureLock => _Secure;
        public LocalMap LocalMap => _LocalMap;

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (lstAvailable.SelectedItem != null)
            {
                SecureLock?.AddKey((Guid)((lstAvailable.SelectedItem as ListBoxItem).Tag));
            }
            RefreshLists();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelected.SelectedItem != null)
            {
                SecureLock?.RemoveKey((Guid)((lstSelected.SelectedItem as ListBoxItem).Tag));
            }
            RefreshLists();
        }

        private void lstAvailable_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstAvailable.SelectedItem != null)
            {
                var _key = (Guid)((lstAvailable.SelectedItem as ListBoxItem).Tag);
                var _rename = new RenameKey(_key, LocalMap)
                {
                    Owner = this
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
                var _rename = new RenameKey(_key, LocalMap)
                {
                    Owner = this
                };
                _rename.ShowDialog();
                RefreshLists();
            }
        }
    }
}
