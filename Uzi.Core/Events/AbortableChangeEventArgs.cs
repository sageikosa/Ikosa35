namespace Uzi.Core
{
    public class AbortableChangeEventArgs<T>: AbortableEventArgs
    {
        public AbortableChangeEventArgs(T newVal, T oldVal)
            : base()
        {
            _Abort = false;
            NewValue = newVal;
            OldValue = oldVal;
        }

        public AbortableChangeEventArgs(T newVal, T oldVal, string action)
            : base(action)
        {
            _Abort = false;
            NewValue = newVal;
            OldValue = oldVal;
        }

        public T NewValue{get; protected set;}
        public T OldValue{get; protected set;}
    }
}
