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
using Uzi.Packaging;
using Uzi.Visualize.Packaging;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using Uzi.Visualize;
using System.IO.Packaging;

namespace CharacterModeler
{
    /// <summary>
    /// Interaction logic for HeadDesigner.xaml
    /// </summary>
    public partial class HeadDesigner : UserControl
    {
        public static RoutedCommand OpenTemplate = new RoutedCommand();
        public static RoutedCommand SelectBrushes = new RoutedCommand();

        public HeadDesigner()
        {
            InitializeComponent();
            this.DataContext = new HeadModel();
        }

        private HeadModel _Model { get { return this.DataContext as HeadModel; } }

        //public CorePackage Package { get; set; }
        public IResolveMaterial Resolver { get; set; }

        private void AlwaysCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbOpenTemplate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // template from file
            var _opener = new System.Windows.Forms.OpenFileDialog();
            _opener.CheckFileExists = true;
            _opener.CheckPathExists = true;
            _opener.Filter = @"Head Template Files (*.head)|*.head";
            _opener.Title = @"Open Head Template...";
            _opener.ValidateNames = true;
            var _rslt = _opener.ShowDialog();
            if (_rslt == System.Windows.Forms.DialogResult.OK)
            {
                using (var _stream = File.OpenRead(_opener.FileName))
                    try
                    {
                        this.DataContext = XamlReader.Load(_stream) as HeadModel;
                        ((this.DataContext) as HeadModel).MaterialResolver = Resolver;
                    }
                    catch
                    {
                        // TODO: exceptions!
                    }
            }
        }

        private void cbSelectBrushes_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // use a common dialog
            var _opener = new System.Windows.Forms.OpenFileDialog();
            _opener.CheckFileExists = true;
            _opener.CheckPathExists = true;
            _opener.Filter = @"Ikosa Files (*.Ikosa)|*.Ikosa";
            _opener.Title = @"Reference File...";
            _opener.ValidateNames = true;
            System.Windows.Forms.DialogResult _rslt = _opener.ShowDialog();
            if (_rslt == System.Windows.Forms.DialogResult.OK)
            {
                // select part
                var _fInfo = new FileInfo(_opener.FileName);
                var _package = new CorePackage(_fInfo, Package.Open(_opener.FileName, FileMode.Open, FileAccess.Read, FileShare.Read));
                var _selector = new SelectPart(_package, (obj) => obj is IResolveMaterial);
                _selector.Owner = Window.GetWindow(this);
                if (_selector.ShowDialog() ?? false)
                {
                    // get resolver
                    Resolver = _selector.BasePart as IResolveMaterial;
                    ((this.DataContext) as HeadModel).MaterialResolver = Resolver;
                    RedrawHead(sender, new RoutedPropertyChangedEventArgs<double>(0, 0));
                }
            }
        }

        private void cbSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter.ToString() == @"Template")
            {
                // template to file
                var _saver = new System.Windows.Forms.SaveFileDialog();
                _saver.Filter = @"Face Template Files (*.head)|*.head";
                _saver.DefaultExt = @"face";
                if (_saver.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var _stream = File.OpenWrite(_saver.FileName))
                        try
                        {
                            XamlWriter.Save(this.DataContext, _stream);
                        }
                        catch
                        {
                        }
                }
            }
            else // fragment
            {
                // model to fragment file
                var _saver = new System.Windows.Forms.SaveFileDialog();
                _saver.Filter = @"XAML (*.xaml)|*.xaml";
                _saver.DefaultExt = @"xaml";
                if (_saver.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var _head = this.DataContext as HeadModel;

                    // save
                    using (var _writer = XmlWriter.Create(_saver.FileName))
                        _head.WriteXml(_writer);
                }
            }
        }

        private void RedrawHead(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var _head = _Model;
            if (_Model != null)
                mdlRenderer.Content = _Model.RenderModel;
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var _head = _Model;
            if (_Model != null)
                mdlRenderer.Content = _Model.RenderModel;
        }

    }
}
