using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
namespace Uzi.Visualize
{
    public class MonochromeEffect : ShaderEffect
    {
        #region Dependency Properties

        /// <summary>
        /// Gets or sets the FilterColor variable within the shader.
        /// </summary>
        public static readonly DependencyProperty FilterColorProperty = DependencyProperty.Register("FilterColor", typeof(Color), typeof(MonochromeEffect), new UIPropertyMetadata(Colors.White, PixelShaderConstantCallback(0)));

        /// <summary>
        /// Gets or sets the Input of the shader.
        /// </summary>
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(MonochromeEffect), 0);

        #endregion

        #region Data
        private static PixelShader pixelShader;
        #endregion

        #region ctor()
        /// <summary>
        /// Creates an instance of the shader from the included pixel shader.
        /// </summary>
        static MonochromeEffect()
        {
            pixelShader = new PixelShader();
            pixelShader.UriSource = ShaderHelper.PackUri(@"Shaders/Monochrome.ps");
        }

        /// <summary>
        /// Creates an instance and updates the shader's variables to the default values.
        /// </summary>
        public MonochromeEffect()
        {
            this.PixelShader = pixelShader;

            UpdateShaderValue(FilterColorProperty);
            UpdateShaderValue(InputProperty);
        }

        #endregion

        /// <summary>
        /// Gets or sets the FilterColor variable within the shader.
        /// </summary>
        public Color FilterColor
        {
            get { return (Color)GetValue(FilterColorProperty); }
            set { SetValue(FilterColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the input used in the shader.
        /// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }
    }
}
