namespace Uzi.Core
{
    public interface IActionFilter
    {
        bool SuppressAction(object source, CoreActionBudget budget, CoreAction action);
    }
}
