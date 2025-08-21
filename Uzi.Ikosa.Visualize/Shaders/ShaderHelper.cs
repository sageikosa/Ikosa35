using System;

namespace Uzi.Visualize
{
    static class ShaderHelper
    {
        public static Uri PackUri(string path)
        {

            var _uri = $@"/{VisualizeAssembly};component/{path}";
            return new Uri(_uri, UriKind.RelativeOrAbsolute);
        }

        private static string _assemblyName;
        private static string VisualizeAssembly
        {
            get
            {
                if (_assemblyName == null)
                {
                    var _asm = typeof(MonochromeEffect).Assembly;
                    _assemblyName = _asm.ToString().Split(',')[0];
                }

                return _assemblyName;
            }
        }
    }
}
