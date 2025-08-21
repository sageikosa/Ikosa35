using System;

namespace Uzi.Ikosa.Time
{
    /// <summary>
    /// Used to indicate the time a magical effect lasts
    /// </summary>
    [Serializable]
    public class Duration
    {
        #region Construction
        public Duration(DurationType timeType)
        {
            DurationTimeType = timeType;
            SpanLength = 0;
        }

        public Duration(double span)
        {
            DurationTimeType = DurationType.Span;
            SpanLength = span;
        }

        public Duration(DurationType timeType, double span)
        {
            DurationTimeType = timeType;
            SpanLength = span;
        }
        #endregion

        #region public double SpanLength { get; set; }
        private double _SpanLength;
        public double SpanLength
        {
            get { return _SpanLength; }
            set
            {
                if (value >= 0)
                {
                    _SpanLength = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
        #endregion

        public DurationType DurationTimeType;
    }
}
