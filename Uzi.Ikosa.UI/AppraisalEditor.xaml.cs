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
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.UI
{
    /// <summary>
    /// Interaction logic for AppraisalEditor.xaml
    /// </summary>
    public partial class AppraisalEditor : UserControl
    {
        public AppraisalEditor(CoreObject coreObject)
        {
            InitializeComponent();

            _CoreObject = coreObject;
            lstAppraise.ItemsSource = FindAdjunctData.FindAdjuncts(coreObject, typeof(Appraisal));
        }

        private CoreObject _CoreObject;

        private void cmdbndAdd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cmdbndAdd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_CoreObject is ItemBase)
            {
                // cost
                decimal _cost = 0m;
                var _item = (_CoreObject as ItemBase);
                _cost = _item.Price.SellPrice;

                // possessor
                var _appraisal = new Appraisal(typeof(Appraisal), _cost);
                if (_item.Possessor != null)
                {
                    _appraisal.CreatureIDs.Add(_item.Possessor.ID, _item.Possessor.Name);
                }

                _CoreObject.AddAdjunct(_appraisal);
            }

            // refresh
            lstAppraise.ItemsSource = null;
            lstAppraise.ItemsSource = FindAdjunctData.FindAdjuncts(_CoreObject, typeof(Appraisal));
            e.Handled = true;
        }

        private void cmdbndDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((lstAppraise != null) && (lstAppraise.SelectedItem != null));
            e.Handled = true;
        }

        private void cmdbndDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _appraisal = lstAppraise.SelectedItem as Appraisal;
            _appraisal.Eject();

            // refresh
            lstAppraise.ItemsSource = null;
            lstAppraise.ItemsSource = FindAdjunctData.FindAdjuncts(_CoreObject, typeof(Appraisal));
            e.Handled = true;
        }
    }
}
