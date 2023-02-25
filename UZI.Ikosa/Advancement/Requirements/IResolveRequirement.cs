namespace Uzi.Ikosa
{
    /// <summary>
    /// Specifies the interface against which the SetOption should be made.  
    /// Typically this will be the advancement requirement's OptionSetter delegate.
    /// Used by the user interface when selecting options.
    /// </summary>
    public interface IResolveRequirement
    {
        bool SetOption(IAdvancementOption advOption);
    }
}
