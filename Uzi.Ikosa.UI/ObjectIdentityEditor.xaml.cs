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
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Services;
using Uzi.Ikosa.Items.Wealth;
using Uzi.Ikosa.Items;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.UI
{
    /// <summary>Interaction logic for ObjectIdentityEditor.xaml</summary>
    public partial class ObjectIdentityEditor : UserControl
    {
        public static RoutedCommand RenameCmd = new RoutedCommand();

        public ObjectIdentityEditor(CoreObject coreObject, CoreActor possessor = null)
        {
            InitializeComponent();

            // identity
            _CoreObject = coreObject;
            _Possessor = possessor ?? (_CoreObject as ICoreItem)?.Possessor;
            tvwIdentities.ItemsSource = FindAdjunctData.FindAdjuncts(coreObject, typeof(Identity));

            // base
            var _info = _CoreObject.GetInfo(possessor, true);
            tvwBase.ItemsSource = _info.ToEnumerable();
        }

        private CoreObject _CoreObject;
        private CoreActor _Possessor;

        #region private void cmdbndNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cmdbndNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion

        #region private void cmdbndNew_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cmdbndNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // create new identity 
            var _id = Identity.CreateIdentity(_Possessor, _CoreObject, typeof(Identity));

            // object possessor get to know about it
            if (_Possessor != null)
            {
                _id.CreatureIDs.Add(_Possessor.ID, _Possessor.Name);

                // make this the new active one?
                _id.Users.Add(_Possessor.ID);
                foreach (var _other in _CoreObject.Adjuncts.OfType<Identity>()
                    .Where(_i => _i != _id && _i.Users.Contains(_Possessor.ID)))
                {
                    _other.Users.Remove(_Possessor.ID);
                }
            }

            // bind
            _CoreObject.AddAdjunct(_id);

            // refresh
            tvwIdentities.ItemsSource = FindAdjunctData.FindAdjuncts(_CoreObject, typeof(Identity));
            e.Handled = true;
        }
        #endregion

        #region private void cmdbndDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cmdbndDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (tvwIdentities != null)
            {
                if (tvwIdentities.SelectedItem != null)
                {
                    if (tvwIdentities.SelectedItem is Identity)
                    {
                        e.CanExecute = true;
                    }
                }
            }
            e.Handled = true;
        }
        #endregion

        #region private void cmdbndDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cmdbndDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (tvwIdentities.SelectedItem != null)
            {
                if (tvwIdentities.SelectedItem is Identity)
                {
                    var _id = tvwIdentities.SelectedItem as Identity;
                    _id.Eject();

                    // refresh
                    tvwIdentities.ItemsSource = FindAdjunctData.FindAdjuncts(_CoreObject, typeof(Identity));
                }
            }
            e.Handled = true;
        }
        #endregion

        #region private void cmdbndRename_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cmdbndRename_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = tvwIdentities?.SelectedItem is ObjectInfo;
            e.Handled = true;
        }
        #endregion

        #region private void cmdbndRename_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cmdbndRename_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (tvwIdentities.SelectedItem != null)
            {
                var _info = tvwIdentities.SelectedItem as Info;
                var _dlg = new RenameDialog(_info.Message)
                {
                    Owner = Window.GetWindow(this)
                };
                if (_dlg.ShowDialog() ?? false)
                {
                    _info.Message = _dlg.RenamingName;
                    tvwIdentities.Items.Refresh();
                }
            }
            e.Handled = true;
        }
        #endregion
    }

    public class ObjectIdentityFolderSelector : DataTemplateSelector
    {
        public HierarchicalDataTemplate AdjunctFolder { get; set; }
        public HierarchicalDataTemplate InfoFolder { get; set; }
        public HierarchicalDataTemplate CreatureFolder { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ListableList<Info>)
                return InfoFolder;
            if (item is ListableList<Adjunct>)
                return AdjunctFolder;
            if (item is ListableList<KeyValuePair<Guid, string>>)
                return CreatureFolder;
            return null;
        }
    }
}
