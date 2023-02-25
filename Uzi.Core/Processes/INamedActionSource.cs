namespace Uzi.Core
{
    public interface INamedActionSource : IActionSource
    {
        string DisplayName { get; }
    }
}
