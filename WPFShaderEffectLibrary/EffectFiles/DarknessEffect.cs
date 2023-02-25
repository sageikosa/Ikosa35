using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ShaderEffectLibrary
{
    public class DarknessEffect : ShaderEffect
    {
        #region Dependency Properties

        // Brush-valued properties turn into sampler-property in the shader.
        // This helper sets "ImplicitInput" as the default, meaning the default
        // sampler is whatever the rendering of the element it's being applied to is.
        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(DarknessEffect), 0);


        // Scalar-valued properties turn into shader constants with the register
        // number sent into PixelShaderConstantCallback().
        public static readonly DependencyProperty DarknessFactorProperty =
            DependencyProperty.Register("DarknessFactor", typeof(double), typeof(DarknessEffect),
                    new UIPropertyMetadata(1.0, PixelShaderConstantCallback(0)));

        #endregion

        #region Member Data

        private static PixelShader _pixelShader = new PixelShader();

        #endregion

        #region Constructors

        static DarknessEffect()
        {
            _pixelShader.UriSource = Global.MakePackUri("ShaderSource/Darkness.ps");
        }

        public DarknessEffect()
        {
            this.PixelShader = _pixelShader;

            // Update each DependencyProperty that's registered with a shader register.  This
            // is needed to ensure the shader gets sent the proper default value.
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(DarknessFactorProperty);
        }

        #endregion

        [System.ComponentModel.BrowsableAttribute(false)]
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public double DarknessFactor
        {
            get { return (double)GetValue(DarknessFactorProperty); }
            set { SetValue(DarknessFactorProperty, value); }
        }

    }
}
