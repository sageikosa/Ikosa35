using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for QualitiesSheet.xaml
    /// </summary>
    public partial class QualitiesSheet : UserControl
    {
        public QualitiesSheet()
        {
            InitializeComponent();
        }

        private void tbLanguages_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var _editor = new LanguageEditor()
            {
                DataContext = DataContext
            };
            _editor.ShowDialog();

            // hard refresh
            var _dc = DataContext;
            DataContext = null;
            DataContext = _dc;
        }
    }
}
