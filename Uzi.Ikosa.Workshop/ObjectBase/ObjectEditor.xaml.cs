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
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.UI;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ObjectEditor.xaml
    /// </summary>
    public partial class ObjectEditor : Window
    {
        #region construction
        public ObjectEditor(ObjectBase objectBase)
        {
            InitializeComponent();
            _Object = objectBase;
            DataContext = this;
        }
        #endregion

        private ObjectBase _Object;

        public ObjectBase ObjectBase => _Object;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cbNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            switch (e.Parameter)
            {
                case @"Searchable":
                default:
                    e.CanExecute = !(ObjectBase?.Adjuncts.OfType<Searchable>().Any() ?? true);
                    break;
            }
            e.Handled = true;
        }

        private void cbNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(ObjectBase?.Adjuncts.OfType<Searchable>().Any() ?? true))
            {
                ObjectBase.AddAdjunct(new Searchable(new Deltable(20), false));
            }
            e.Handled = true;
        }

        private void cbDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !((e.Parameter as Adjunct)?.IsProtected ?? true);
            e.Handled = true;
        }

        private void cbDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectBase?.RemoveAdjunct(e.Parameter as Adjunct);
            e.Handled = true;
        }
    }
}
