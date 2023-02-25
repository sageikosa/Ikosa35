using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace ModelMaker
{
    public abstract class ModelElement3D : ModelVisual3D
    {
        protected ModelElement3D()
            : base()
        {
            _Content = new GeometryModel3D();
            _Content.Geometry = RegenMesh();
            Content = _Content;
        }

        public static DependencyProperty MaterialProperty = DependencyProperty.Register("Material", typeof(Material),
            typeof(ModelElement3D), new PropertyMetadata(null, new PropertyChangedCallback(OnMaterialChanged)));

        public static DependencyProperty BackMaterialProperty =DependencyProperty.Register("BackMaterial", typeof(Material),
            typeof(ModelElement3D), new PropertyMetadata(null, new PropertyChangedCallback(OnBackMaterialChanged)));

        public Material Material
        {
            get { return (Material)GetValue(MaterialProperty); }
            set { SetValue(MaterialProperty, value); }
        }

        public Material BackMaterial
        {
            get { return (Material)GetValue(BackMaterialProperty); }
            set { SetValue(BackMaterialProperty, value); }
        }

        private static void OnMaterialChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            ModelElement3D _me = ((ModelElement3D)sender);
            _me._Content.Material = _me.Material;
        }

        private static void OnBackMaterialChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            ModelElement3D _me = ((ModelElement3D)sender);
            _me._Content.BackMaterial = _me.BackMaterial;
        }

        protected GeometryModel3D _Content;

        /// <summary>
        /// Return a geometry mesh for the content
        /// </summary>
        /// <returns></returns>
        protected abstract Geometry3D RegenMesh();
    }
}
