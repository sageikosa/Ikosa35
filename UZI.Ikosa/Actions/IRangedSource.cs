namespace Uzi.Ikosa.Actions
{
    public interface IRangedSource
    {
        /// <summary>Incremental range at which additional penalties apply</summary>
        int RangeIncrement { get; }
        /// <summary>Maximum range for the weapon</summary>
        int MaxRange { get; }
    }
}
