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
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;
using System.Diagnostics;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for ImageryIcon.xaml
    /// </summary>
    public partial class ImageryIcon : UserControl
    {
        public ImageryIcon()
        {
            try { InitializeComponent(); } catch { }
        }

        public ImageryInfo ImageryInfo
        {
            get { return (ImageryInfo)GetValue(ImageryInfoProperty); }
            set { SetValue(ImageryInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageryInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageryInfoProperty =
            DependencyProperty.Register(@"ImageryInfo", typeof(ImageryInfo), typeof(ImageryIcon), new UIPropertyMetadata(null, new PropertyChangedCallback(ResolverChanged)));

        public IResolveIcon IconResolver
        {
            get { return (IResolveIcon)GetValue(IconResolverProperty); }
            set { SetValue(IconResolverProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconResolver.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconResolverProperty =
            DependencyProperty.Register(@"IconResolver", typeof(IResolveIcon), typeof(ImageryIcon),
            new UIPropertyMetadata(null, new PropertyChangedCallback(ResolverChanged)));

        static void ResolverChanged(DependencyObject dependecy, DependencyPropertyChangedEventArgs args)
        {
            // NOTE: route to data context changed
            var _control = dependecy as ImageryIcon;
            if (_control != null)
            {
                var _info = _control.ImageryInfo;
                var _resolver = _control.IconResolver;
                if ((_info != null) && (_resolver != null))
                {
                    if (_info.Keys != null)
                    {
                        var _cMap = _info.IconRef;
                        foreach (var _key in _info.Keys)
                        {
                            var _icon = _resolver.GetIconVisual(_key, _cMap);
                            if (_icon != null)
                            {
                                _control.ccIcon.Content = _icon;
                                return;
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine(@"No Keys");
                    }
                }
            }
        }
    }
}
