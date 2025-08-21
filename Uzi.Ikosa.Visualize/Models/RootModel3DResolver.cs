using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Uzi.Visualize
{
    public class RootModel3DResolver : IResolveModel3D
    {
        #region construction
        private RootModel3DResolver()
        {
        }
        #endregion

        #region static construction
        private static DiffuseMaterial SubMaterial(ImageBrush brush, byte seed)
        {
            var _mat = new DiffuseMaterial(brush)
            {
                AmbientColor = Color.FromArgb(255, seed, seed, seed)
            };
            _mat.Freeze();
            return _mat;
        }

        static RootModel3DResolver()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
             {

                 _Materials = [];
                 var _image = new BitmapImage(new Uri(@"pack://application:,,,/Uzi.Visualize;component/Images/NoModel.png"));
                 var _brush = new ImageBrush(_image);
                 _brush.Freeze();

                 // normal material
                 var _material = new DiffuseMaterial(_brush);
                 _material.Freeze();
                 _Materials.Add(VisualEffect.Normal, _material);

                 // other brushes and materials
                 foreach (var (_effect, _render) in _image.RenderImages())
                 {
                     // brush
                     var _newBrush = new ImageBrush(_render);
                     _newBrush.Freeze();

                     // material
                     _material = new DiffuseMaterial(_newBrush);
                     _Materials.Add(_effect, _material);
                     _material.Freeze();
                     if (_effect == VisualEffect.Monochrome)
                     {
                         _Materials.Add(VisualEffect.MonoSub1, SubMaterial(_newBrush, 225));
                         _Materials.Add(VisualEffect.MonoSub2, SubMaterial(_newBrush, 195));
                         _Materials.Add(VisualEffect.MonoSub3, SubMaterial(_newBrush, 165));
                         _Materials.Add(VisualEffect.MonoSub4, SubMaterial(_newBrush, 135));
                         _Materials.Add(VisualEffect.MonoSub5, SubMaterial(_newBrush, 105));
                     }
                     if (_effect == VisualEffect.FormOnly)
                     {
                         _Materials.Add(VisualEffect.FormSub1, SubMaterial(_newBrush, 235));
                         _Materials.Add(VisualEffect.FormSub2, SubMaterial(_newBrush, 215));
                         _Materials.Add(VisualEffect.FormSub3, SubMaterial(_newBrush, 195));
                         _Materials.Add(VisualEffect.FormSub4, SubMaterial(_newBrush, 175));
                         _Materials.Add(VisualEffect.FormSub5, SubMaterial(_newBrush, 155));
                     }
                 }

                 // static model
                 _Static = GenerateModel();
             });
        }
        #endregion

        private static Dictionary<VisualEffect, System.Windows.Media.Media3D.Material> _Materials;
        private static Model3D _Static;

        public static IResolveModel3D Root
        {
            get { return new RootModel3DResolver(); }
        }

        #region private static Model3D GenerateModel()
        private static Model3D GenerateModel()
        {
            var _bump = new TranslateTransform3D(-2.5d, -2.5d, 0d);
            var _model = new Model3DGroup();
            _model.Children.Add(HedralGenerator.GeometryModel3DTransform(CellSpaceFaces.Mesh,
                _Materials[BottomSenseEffectExtension.EffectValue], null, HedralGenerator.ZMTransform, _bump));
            _model.Children.Add(HedralGenerator.GeometryModel3DTransform(CellSpaceFaces.Mesh,
                _Materials[TopSenseEffectExtension.EffectValue], null, HedralGenerator.ZPTransform, _bump));

            _model.Children.Add(HedralGenerator.GeometryModel3DTransform(CellSpaceFaces.Mesh,
                _Materials[LeftSenseEffectExtension.EffectValue], null, HedralGenerator.YPTransform, _bump));
            _model.Children.Add(HedralGenerator.GeometryModel3DTransform(CellSpaceFaces.Mesh,
                _Materials[RightSenseEffectExtension.EffectValue], null, HedralGenerator.YMTransform, _bump));

            _model.Children.Add(HedralGenerator.GeometryModel3DTransform(CellSpaceFaces.Mesh,
                _Materials[FrontSenseEffectExtension.EffectValue], null, HedralGenerator.XPTransform, _bump));
            _model.Children.Add(HedralGenerator.GeometryModel3DTransform(CellSpaceFaces.Mesh,
                _Materials[BackSenseEffectExtension.EffectValue], null, HedralGenerator.XMTransform, _bump));
            return _model;
        }
        #endregion

        #region IResolveModel3D Members

        public Model3D GetPrivateModel3D(object key)
        {
            return GenerateModel();
        }

        public bool CanResolveModel3D(object key)
        {
            return false;
        }

        public IResolveModel3D IResolveModel3DParent
        {
            get { return null; }
        }

        public IEnumerable<Model3DPartListItem> ResolvableModels
        {
            get { yield break; }
        }

        #endregion
    }
}
