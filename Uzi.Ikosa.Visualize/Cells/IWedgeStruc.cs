namespace Uzi.Visualize
{
    public interface IWedgeStruc
    {
        bool CornerStyle { get; }
        bool HasPlusTiling { get; }
        bool HasTiling { get; }
        bool IsInvisible { get; }
        bool IsPlusInvisible { get; }
        double Offset1 { get; }
        double Offset2 { get; }

        BuildableMaterial GetOrthoFaceMaterial(Axis axis, bool isPlusFace, VisualEffect effect);
        BuildableMaterial GetOtherFaceMaterial(int index, VisualEffect effect);
        BuildableMaterial GetPlusOrthoFaceMaterial(Axis axis, bool isPlusFace, VisualEffect effect);
        BuildableMaterial GetPlusOtherFaceMaterial(int index, VisualEffect effect);
    }
}