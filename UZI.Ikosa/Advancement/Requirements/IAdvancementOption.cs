using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    public interface IAdvancementOption
    {
        string Name { get; }
        string Description { get; }
        AdvancementOptionInfo ToAdvancementOptionInfo();
        IAdvancementOption GetOption(AdvancementOptionInfo optInfo);
    }
}
