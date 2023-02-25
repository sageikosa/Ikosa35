using System;

namespace Uzi.Core
{
    public class ChangeValueEventArgs<T>: EventArgs 
    {
        public ChangeValueEventArgs(T oldVal, T newVal)
        {
            OldValue = oldVal;
            NewValue = newVal;
            Action = string.Empty;
        }

        public ChangeValueEventArgs(T oldVal, T newVal, string action)
        {
            OldValue = oldVal;
            NewValue = newVal;
            Action = action;
        }

        public readonly T OldValue;
        public readonly T NewValue;
        public readonly string Action;
    }
}
