using System;

namespace Uzi.Core
{
    /// <summary>0 = IsClosed</summary>
    [Serializable]
    public readonly struct OpenStatus : ISourcedObject
    {
        public OpenStatus(double value)
        {
            _Src = null;
            _Value = value;
        }

        /// <summary>0 = IsClosed</summary>
        internal OpenStatus(object source, double value)
        {
            if (value < 0) value = 0;
            if (value > 1) value = 1;
            _Value = value;
            _Src = source;
        }

        private readonly double _Value;
        private readonly object _Src;

        public double Value =>_Value;
        public object Source=> _Src; 
        public bool IsClosed=> _Value == 0; 
    }
}
