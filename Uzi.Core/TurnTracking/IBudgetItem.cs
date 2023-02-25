namespace Uzi.Core
{
    public interface IBudgetItem : ISourcedObject
    {
        string Name { get; }
        string Description { get; }
        void Added(CoreActionBudget budget);
        void Removed();
    }
}
