using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Uzi.Visualize
{
    public class ResourceContent : ContentControl
    {
        public ResourceDictionary ResourceDictionary
        {
            get { return (ResourceDictionary)GetValue(ResourceDictionaryProperty); }
            set { SetValue(ResourceDictionaryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ResourceDictionary.  This enables animation, styling, binding, etc... 
        public static readonly DependencyProperty ResourceDictionaryProperty =
            DependencyProperty.Register(@"ResourceDictionary", typeof(ResourceDictionary), typeof(ResourceContent), 
            new UIPropertyMetadata(null, new PropertyChangedCallback(ResourceChanged)));

        static void ResourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _rc = d as ResourceContent;
            _rc.Resources = e.NewValue as ResourceDictionary;
        }
    }
}
