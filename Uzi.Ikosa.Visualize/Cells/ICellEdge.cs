namespace Uzi.Visualize
{
    public interface ICellEdge
    {
        string EdgeMaterial { get; }
        string EdgeTiling { get; }
        double Width { get; }
        BuildableMeshKey GetBuildableMeshKey(AnchorFace face, VisualEffect effect);
        BuildableMaterial GetOrthoFaceMaterial(Axis axis, bool isPlusFace, VisualEffect effect);
        BuildableMaterial GetOtherFaceMaterial(int index, VisualEffect effect);
    }
}