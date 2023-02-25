namespace Uzi.Core
{
    public interface ILinkOwner<RefType>
    {
        void LinkAdded(RefType changer);
        void LinkDropped(RefType changer);
    }
}
