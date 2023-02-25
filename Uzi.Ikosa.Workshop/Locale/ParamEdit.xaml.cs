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
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Workshop.Locale
{
    /// <summary>
    /// Interaction logic for ParamEdit.xaml
    /// </summary>
    public partial class ParamEdit : Window
    {
        public ParamEdit(IParamCellSpace cellSpace, uint paramData)
        {
            InitializeComponent();
            ccParams.Content = ParamPicker.GetParamControl(cellSpace, paramData);
        }

        public uint ParamData { get { return ParamPicker.ParamData(ccParams); } }

        private void cmdbndOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // parse values and keep them at the ready...
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        #region private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // values
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion
    }
}
