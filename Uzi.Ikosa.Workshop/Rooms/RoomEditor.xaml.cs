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
using Uzi.Ikosa.Tactical;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.UI;
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for RoomEditor.xaml
    /// </summary>
    public partial class RoomEditor : TabItem, IPackageItem, IHostedTabItem
    {
        #region construction
        public RoomEditor(Room room, IHostTabControl host)
        {
            InitializeComponent();
            Content = new RoomEditorControl(room);
            DataContext = room;
            _Room = room;
            _Host = host;
        }
        #endregion

        private IHostTabControl _Host;

        private Room _Room;
        public Room Room { get { return _Room; } }

        public object PackageItem { get { return _Room; } }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region IHostedTabItem Members
        public void CloseTabItem()
        {
        }
        #endregion
    }
}
