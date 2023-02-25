using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(Int32))]
    public class IntVal : ParameterExtension<Int32>
    {
        #region public static Action<IntVal> ReferencedKey { get { return _KeyFound; } set { _KeyFound = value; } }
        [ThreadStatic]
        private static Action<IntVal> _KeyReferenced = null;

        /// <summary>Thread static tracking callback</summary>
        public static Action<IntVal> ReferencedKey { get { return _KeyReferenced; } set { _KeyReferenced = value; } }
        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_KeyReferenced != null)
                _KeyReferenced(this);

            if (MetaModelResolutionStack.Any())
            {
                // get specific mapped IntVal (if any)
                var _current = MetaModelResolutionStack.Peek();
                var _iVal = _current.IntRefs[Key];
                if (_iVal != null)
                {
                    if (!_iVal.UseMaster)
                        return _iVal.Value;
                    if (MetaModelResolutionStack.State != null)
                    {
                        _iVal = MetaModelResolutionStack.State.MasterIntReferences[Key];
                        if (_iVal != null)
                            return _iVal.Value;
                    }
                }
            }

            return base.ProvideValue(serviceProvider);
        }
    }
}
