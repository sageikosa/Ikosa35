using System;

namespace Uzi.Core
{
    [Serializable]
    public abstract class InteractionFeedback : ISourcedObject 
    {
        protected InteractionFeedback(object source)
        {
            Source = source;
        }

        public object Source { get; private set; }
    }

    public class UnderstoodFeedback : InteractionFeedback
    {
        public UnderstoodFeedback(object source)
            : base(source)
        {
        }
    }
}
