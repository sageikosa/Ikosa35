namespace Uzi.Core
{
    /// <summary>Generic Value Feedback</summary>
    public class ValueFeedback<ValueType> : InteractionFeedback
    {
        /// <summary>Generic Value Feedback</summary>
        public ValueFeedback(object source, ValueType value)
            : base(source)
        {
            this.Value = value;
        }

        public ValueType Value { get; private set; }
    }
}
