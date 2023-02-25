using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(int))]
    public class ExternalVal : ParameterExtension<int>
    {
        [ThreadStatic]
        private static Dictionary<string, int> _Values = new Dictionary<string, int>();

        public static Dictionary<string, int> Values
        {
            get
            {
                if (_Values == null)
                    _Values = new Dictionary<string, int>();
                return _Values;
            }
        }

        #region public static Action<ExternalVal> ReferencedKey { get { return _KeyFound; } set { _KeyFound = value; } }
        [ThreadStatic]
        private static Action<ExternalVal> _KeyReferenced = null;

        /// <summary>Thread static tracking callback</summary>
        public static Action<ExternalVal> ReferencedKey { get { return _KeyReferenced; } set { _KeyReferenced = value; } }
        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_KeyReferenced != null)
                _KeyReferenced(this);

            return Values.ContainsKey(Key)
                ? Values[Key]
                : base.ProvideValue(serviceProvider);
        }
    }
}
