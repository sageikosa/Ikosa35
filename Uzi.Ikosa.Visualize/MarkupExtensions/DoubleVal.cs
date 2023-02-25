using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(Double))]
    public class DoubleVal : ParameterExtension<Double>
    {
        #region public static Action<DoubleVal> ReferencedKey { get { return _KeyFound; } set { _KeyFound = value; } }
        [ThreadStatic]
        private static Action<DoubleVal> _KeyReferenced = null;

        /// <summary>Thread static tracking callback</summary>
        public static Action<DoubleVal> ReferencedKey { get { return _KeyReferenced; } set { _KeyReferenced = value; } }
        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_KeyReferenced != null)
                _KeyReferenced(this);

            if (MetaModelResolutionStack.Any())
            {
                // get specific mapped DoubleVal (if any)
                var _current = MetaModelResolutionStack.Peek();
                var _dVal = _current.DoubleRefs[Key];
                if (_dVal != null)
                {
                    if (!_dVal.UseMaster)
                        return _dVal.Value;
                    if (MetaModelResolutionStack.State != null)
                    {
                        _dVal = MetaModelResolutionStack.State.MasterDoubleReferences[Key];
                        if (_dVal != null)
                            return _dVal.Value;
                    }
                }
            }

            return base.ProvideValue(serviceProvider);
        }
    }
}
