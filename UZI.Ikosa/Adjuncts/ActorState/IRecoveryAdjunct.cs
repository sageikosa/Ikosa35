namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Recovery adjuncts are removed if the actor takes damage.</summary>
    public interface IRecoveryAdjunct
    {
        /// <summary>True if this adjunct indicates the actor is barely recovering.  Assisted healing removes this condition.</summary>
        bool IsBarelyRecovering { get; }
    }
}
