using System;
namespace CharacterModeler.Generator
{
    public interface IBottomDimensionProvider
    {
        double Thickness { get; }
        double Width { get; }
    }
}
