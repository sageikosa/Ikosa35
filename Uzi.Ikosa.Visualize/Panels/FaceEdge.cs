namespace Uzi.Visualize
{
    /// <summary>
    /// Bottom | Top | Left | Right
    /// </summary>
    public enum FaceEdge
    {
        Bottom = 0,
        Top = 1,
        Left = 2,
        Right = 3,
    }

    public static class FaceEdgeHelper
    {
        public static FaceEdge GetLeftRightSwap(this FaceEdge edge)
        {
            switch (edge)
            {
                case FaceEdge.Left: return FaceEdge.Right;
                case FaceEdge.Right: return FaceEdge.Left;
                default: return edge;
            }
        }

        public static FaceEdge GetUpDownSwap(this FaceEdge edge)
        {
            switch (edge)
            {
                case FaceEdge.Top: return FaceEdge.Bottom;
                case FaceEdge.Bottom: return FaceEdge.Top;
                default: return edge;
            }
        }

        public static FaceEdge GetFullSwap(this FaceEdge edge)
        {
            switch (edge)
            {
                case FaceEdge.Bottom: return FaceEdge.Top;
                case FaceEdge.Top: return FaceEdge.Bottom;
                case FaceEdge.Left: return FaceEdge.Right;
                case FaceEdge.Right: return FaceEdge.Left;
                default: return edge;
            }
        }
    }
}