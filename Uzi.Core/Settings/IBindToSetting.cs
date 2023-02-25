namespace Uzi.Core
{
    /// <summary>Called after setting binding.  Intended for use when importing or expunging.</summary>
    public interface IBindToSetting
    {
        void BindToSetting();
        void UnbindFromSetting();
    }
}
