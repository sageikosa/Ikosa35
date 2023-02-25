namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Interface to restrict the exposure of the adjunct.</summary>
    public interface IProtectable
    {
        bool IsExposedTo(Creature critter);
    }
}
