namespace Uzi.Core
{
    /// <summary>
    /// Indicates that the object exposing the interface can handle interactions
    /// </summary>
    public interface IInteract : ICore
    {
        void HandleInteraction(Interaction interact);
    }
}
